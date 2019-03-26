using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nucoris.ioc.container;

namespace nucoris.webapp
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IHostingEnvironment _environment;
        private readonly ILogger<Startup> _logger;
        private readonly bool _useAuthentication;

        public Startup(IConfiguration configuration, IHostingEnvironment environment, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _environment = environment;
            _logger = logger;

            var nucorisConfiguration = Configuration.GetSection("nucoris");
            if (nucorisConfiguration != null)
            {
                var authenticationMode = nucorisConfiguration.GetValue<string>("AuthenticationMode");
                _useAuthentication = (authenticationMode == "AzureAd");
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // We use Autofac IoC container:
            var container = new ContainerBuilder();

            Action<MvcOptions> setupMvcAuthentication = ConfigureAuthentication(services, container);
            if (setupMvcAuthentication != null)
            {
                services.AddMvc(options => setupMvcAuthentication(options)).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            }
            else
            {
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            }

            // Set up container:
            container.Populate(services); // register services configured before this line
            container.RegisterModule(new MediatorAutofacModule()); // register application command and event handlers

            // register repositories and DB access objects:
            var settingsTask = GetDatabaseSettings();
            var (dbEndpoint, dbAuthKey) = settingsTask.Result;
            _logger.LogInformation($"Database endpoint: {dbEndpoint}");
            container.RegisterModule(new ApplicationAutofacModule(dbEndpoint, dbAuthKey));


            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                _logger.LogInformation("Development environment");
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                _logger.LogInformation("Non-development environment");
            }

            app.UseHttpsRedirection();
            app.UseStatusCodePages();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            if( _useAuthentication) app.UseAuthentication();

            app.UseMvc();
        }

        private Action<MvcOptions> ConfigureAuthentication(IServiceCollection services, ContainerBuilder container)
        {
            Action<MvcOptions> setupMvcAuthentication;

            if (_useAuthentication)
            {
                services.AddHttpContextAccessor(); // required by AADUserIdentityProvider

                container.RegisterType<AADUserIdentityProvider>().
                    As<application.interfaces.IUserIdentityProvider>().
                    InstancePerLifetimeScope();

                services.Configure<CookiePolicyOptions>(options =>
                {
                    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                    options.CheckConsentNeeded = context => true;
                    options.MinimumSameSitePolicy = SameSiteMode.None;
                });

                ///// Note: AUTHENTICATION CODE ADAPTED FROM
                // https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-v2-aspnet-core-webapp
                services.AddAuthentication(AzureADDefaults.AuthenticationScheme)
                    .AddAzureAD(options => Configuration.Bind("AzureAd", options));

                services.Configure<OpenIdConnectOptions>(AzureADDefaults.OpenIdScheme, options =>
                {
                    options.Authority = options.Authority + "/v2.0/";

                    // Per the code below, this application signs in users in any Work and School
                    // accounts and any Microsoft Personal Accounts.
                    // If you want to direct Azure AD to restrict the users that can sign-in, change 
                    // the tenant value of the appsettings.json file in the following way:
                    // - only Work and School accounts => 'organizations'
                    // - only Microsoft Personal accounts => 'consumers'
                    // - Work and School and Personal accounts => 'common'

                    // If you want to restrict the users that can sign-in to only one tenant
                    // set the tenant value in the appsettings.json file to the tenant ID of this
                    // organization, and set ValidateIssuer below to true.

                    // If you want to restrict the users that can sign-in to several organizations
                    // Set the tenant value in the appsettings.json file to 'organizations', set
                    // ValidateIssuer, above to 'true', and add the issuers you want to accept to the
                    // options.TokenValidationParameters.ValidIssuers collection
                    options.TokenValidationParameters.ValidateIssuer = false;
                });

                setupMvcAuthentication = (options) =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                                                .RequireAuthenticatedUser()
                                                .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                };

                _logger.LogInformation("Application configured for Authentication with Azure AD");
            }
            else
            {
                // Default user identity provider returns Guest User
                container.RegisterType<application.interfaces.DefaultUserIdentityProvider>().
                    As<application.interfaces.IUserIdentityProvider>().
                    InstancePerLifetimeScope();

                setupMvcAuthentication = null;

                _logger.LogInformation("Application NOT configured for Authentication");
            }

            return setupMvcAuthentication;
        }

        private async Task<(string dbEndpoint, string dbAuthKey)> GetDatabaseSettings()
        {
            var webSiteName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
            _logger.LogInformation($"Running from {webSiteName ?? "local host"}");

            if ( String.IsNullOrWhiteSpace(webSiteName))
            {
                // Assume local host connected to local emulator:
                return ("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==");
            }
            else
            {
                // Assume Azure with settings in key vault:
                try
                {
                    var tokenProvider = new AzureServiceTokenProvider();
                    var keyVaultClient = new KeyVaultClient(
                        new KeyVaultClient.AuthenticationCallback(tokenProvider.KeyVaultTokenCallback));
                    var endpoint = await keyVaultClient.GetSecretAsync("https://nucoris.vault.azure.net/secrets/nucorisDbEndpoint").
                        ConfigureAwait(false);
                    var key = await keyVaultClient.GetSecretAsync("https://nucoris.vault.azure.net/secrets/nucorisDbAuthKey").
                        ConfigureAwait(false);
                    return (endpoint.Value, key.Value);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"Cannot read database configuration from Key Vault. Error: {ex.ToString()} ");
                    throw;
                }                
            }
        }
    }
}

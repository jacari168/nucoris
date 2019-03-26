using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace nucoris.application.tests
{
    [TestClass]
    public class ApplicationInitialize
    {
        internal static IContainer AutofacContainer { get; private set; }

        // We assume in the DB there is a user with this id
        internal static readonly Guid TestUserId = Guid.Parse("a247196b-4f1d-4e2a-9fa7-f80432706e7f");


        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ioc.container.MediatorAutofacModule());
            builder.RegisterModule(new ioc.container.ApplicationAutofacModule(
                dbEndpoint: "https://localhost:8081", dbAuthKey: "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="));

            builder.RegisterType<application.interfaces.DefaultUserIdentityProvider>().
                As<application.interfaces.IUserIdentityProvider>().
                InstancePerLifetimeScope();

            AutofacContainer = builder.Build();
        }
    }
}

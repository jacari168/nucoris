using Autofac;
using nucoris.application.commands;
using nucoris.application.interfaces;
using nucoris.persistence;

namespace nucoris.ioc.container
{
    /// <summary>
    /// This module configures in the Autofac IoC container the repositories and other database access code
    /// </summary>
    public class ApplicationAutofacModule : Module
    {
        private readonly string _dbEndpoint;
        private readonly string _dbAuthKey;

        public ApplicationAutofacModule(string dbEndpoint, string dbAuthKey)
        {
            _dbEndpoint = dbEndpoint;
            _dbAuthKey = dbAuthKey;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CosmosDBGateway>().
                AsSelf().
                SingleInstance().
                WithParameter("endpoint", _dbEndpoint).
                WithParameter("authKey", _dbAuthKey);

            var dbSessionConfig = DbSessionConfigurationFactory.New();
            builder.RegisterInstance(dbSessionConfig).
                SingleInstance().
                As<IDbSessionConfiguration>();

            builder.RegisterType<DbSession>().
                AsSelf().
                As<IUnitOfWork>().
                InstancePerLifetimeScope();

            builder.RegisterType<CommandSession>().
                As<ICommandSession>().
                InstancePerLifetimeScope();

            // Register all repositories:
            // Note that we have those dealing with patient data,
            //  and those dealing with reference (non-patient) data, such as users.
            builder.RegisterAssemblyTypes(typeof(PatientRepository).Assembly).
                AsClosedTypesOf(typeof(IPatientDescendentRepository<>)).
                InstancePerLifetimeScope();
            builder.RegisterAssemblyTypes(typeof(UserRepository).Assembly).
                AsClosedTypesOf(typeof(IReferenceDataRepository<,>)).
                InstancePerLifetimeScope();

            // In queries there is only a generic repository:
            builder.RegisterGeneric(typeof(MaterializedViewRepository<,>)).
                As(typeof(IMaterializedViewRepository<,>)).
                InstancePerLifetimeScope();
        }
    }
}

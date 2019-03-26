using System.Reflection;
using Autofac;
using MediatR;

namespace nucoris.ioc.container
{
    /// <summary>
    /// This Autofac module associates command and event handlers with their related commands/events using MediatR.
    /// </summary>
    public class MediatorAutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly)
                .AsImplementedInterfaces();

            // Register all the Command classes (they implement IRequestHandler) in assembly holding the Commands
            builder.RegisterAssemblyTypes(typeof(application.commands.CreatePatientCommand).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(IRequestHandler<,>));

            // Register the DomainEventHandler classes (they implement INotificationHandler<>) in assembly holding the Domain Event handlers
            builder.RegisterAssemblyTypes(typeof(application.eventHandlers.PatientStatusChangeEventHandler).GetTypeInfo().Assembly)
                .AsClosedTypesOf(typeof(INotificationHandler<>));

            builder.Register<ServiceFactory>(context =>
            {
                var componentContext = context.Resolve<IComponentContext>();
                return t => { return (componentContext.TryResolve(t, out object o) ? o : null); };
            });
        }
    }
}

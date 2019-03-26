This module contains the configuration of the IoC (dependency injection) container of nucoris.

I've placed it in a separate project because it's referenced by both the application tests and the ASP.NET Core web application.

As you can see, I use Autofac (https://autofac.org/) as container.
I've split the configuration into two Autofac modules for clarity:
- A module to configure the command and event handlers invoked by our mediator object (https://github.com/jbogard/MediatR)
- A module to configure the repositories and other application database and session objects.
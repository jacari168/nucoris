# nucoris
nucoris is the exploratory prototype of a cloud-native healthcare information system (HIS) that I have developed and used as a learning platform during a sabbatical leave.

It’s built with SOLID principles and a domain-driven approach in the Clean Architecture style. The application layer is organized around the CQRS pattern, with events being used to support business rules and for interoperability.

In the frontend, an ASP.NET Core web app hosted by Azure App Service renders Razor Pages styled with Bootstrap. The applications exposes also a REST API that some pages use to post simple commands and do partial view updates.
In the backend, data is stored in Azure Cosmos DB document database. Domain events are also persisted. An Azure Function is bound to Cosmos DB Change Feed and picks and publishes events to an Azure Service Bus topic which I call the “application” bus.
Azure Functions and Logic Apps subscribe to those domain event messages in the bus. They implement custom logic as needed to extend the core functionality, and may publish integration events into an “integration” bus to which other systems can subscribe. They may also access other resources, including the REST API exposed by the application.
For business analysis, Azure Data Factory periodically transfers data from nucoris Cosmos DB into a data warehouse supported by an Azure SQL Database. 

# Repository Structure
The repository is structured in three folders: doc, src, test.
Their contents can be easily inferred from the name.

The doc folder includes documents describing the architecture:
- nucoris Architecture Diagram.pdf: a chart and a brief description of the architecture.
- nucoris Architecture.pdf: evaluation of the qualities we expect from an HIS, Azure features and a number of reflections on the architecture of nucoris.
- nucoris Persistence.pdf: evaluation of Cosmos DB as the database for an HIS, and how I've modeled nucoris to work around some limitations of document DBs.
- nucoris CQRS Implementation.pdf: description of my implementation of the CQRS pattern in nucoris.

# Value
nucoris is not a component you can reuse in your project, nor it provides the foundation of a production-ready healthcare information system.
I believe though that you might find some valuable ideas in the architecture documents, especially in the way I've implemented the persistence layer when looking for a trade-off among the features of relational and document databases.



README for nucoris.application.tests project:

This MSTest project contains test cases on the application layer commands.
There is at least one test case per command, so you can debug its handler implementation.

It's designed to be run locally, with the following assumptions:
1) Cosmos DB emulator is installed: https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator
2) There is a database called "nucorisDb"
3) The database has a collection "nucorisCol" set up with partition key "/partitionKey"
4) In this collection there is a document for the test user, with this contents:

    "id": "a247196b-4f1d-4e2a-9fa7-f80432706e7f",
    "partitionKey": "R_User",
    "docType": "Reference",
    "docSubType": "nucoris.domain.User",
    "docContents": {
        "Username": "test@nucoris.com",
        "GivenName": "Test",
        "FamilyName": "User",
        "Id": "a247196b-4f1d-4e2a-9fa7-f80432706e7f"
    }

I use Autofac as the container to inject dependencies. It's set up from ApplicationInitialize.cs, 
which calls two modules you can find in project nucoris.ioc.container:
- MediatorAutofacModule: binds events and their event handlers, and commands and their command handlers
- ApplicatinoAutofacModule: binds all other staff: repositories, database, etc.


#r "Microsoft.Azure.DocumentDB.Core"
#r "..\\bin\\Microsoft.Azure.ServiceBus.dll" 

using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.ServiceBus;

// This function consumes document change feed from Cosmos DB
//  and sends the documents representing domain events to the application service bus
public static void Run(IReadOnlyList<Document> input, ILogger log, ICollector<Message> messages)
{
    if (input != null && input.Count > 0)
    {
        log.LogInformation("Documents modified " + input.Count);        

        foreach( var doc in input)
        {
            var docType = doc.GetPropertyValue<string>("docType");
            log.LogInformation($"Document with id {doc.Id} has docType='{docType}'");
            if( docType == "Event")
            {
                var partitionKey = doc.GetPropertyValue<string>("partitionKey");
                var message = new Message(System.Text.Encoding.UTF8.GetBytes(doc.ToString()));
                message.SessionId = partitionKey;
                message.Label = docType;             
                messages.Add(message);
                log.LogInformation($"Added Event document {doc.Id} to service bus 'application'");
            }
        }
    }
}

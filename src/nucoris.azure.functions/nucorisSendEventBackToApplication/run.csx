#r "..\\bin\\Microsoft.Azure.ServiceBus.dll" 

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

// Create a single, static HttpClient, following advice from:
// https://docs.microsoft.com/en-us/azure/azure-functions/manage-connections
static HttpClient _httpClient = new HttpClient();

// This function listens to events in the application bus and sends them back to the application.
// This allows the application to process incoming user requests quickly and defer handling of non-urgent events, if need be.
// See nucoris Persistence document for further information.
public static async Task Run(Message message, ILogger log)
{
    log.LogInformation($"Received service bus message with id {message.MessageId} and label {message.Label}");

    if (message.Label == "Event")
    {
        var byteContent = new ByteArrayContent(message.Body);
        byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var result = await _httpClient.PostAsync("https://nucoris.azurewebsites.net/api/applicationEvents", byteContent);

        if (result.IsSuccessStatusCode)
        {
            log.LogInformation($"Successfully processed service bus message {message.MessageId} with result {result.StatusCode.ToString()}");
        }
        else
        {
            string errorMsg = await result.Content.ReadAsStringAsync();
            log.LogError(   $"{result.StatusCode} when processing service bus message {message.MessageId}. Error: {errorMsg}");
        }
    }
}

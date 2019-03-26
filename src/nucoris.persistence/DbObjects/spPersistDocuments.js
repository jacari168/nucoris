// Adapted from Microsoft's sample:
// https://raw.githubusercontent.com/Azure/azure-cosmos-dotnet-v2/master/samples/code-samples/ServerSideScripts/JS/BulkImport.js

/**
 * This script is a Cosmos DB stored procedure to insert/update/delete one or more documents
 *  (nucoris objects) in one batch.
 * The script sets response body to the number of docs imported.
 * This allows the client to call this sp multiple times 
 *  until total number of docs desired by the client is imported.
 * @param  {Object[]} docProperties - Array of document properties to import. Each object has 3 properties:
 *      state: one of "Added", "Modified", "Deleted"
 *      document: the document to persist
 *      documentLink: a link to the existing document when we are updating/deleting an existing one (null otherwise)
*/
function spPersistDocuments(docProperties) {
    var collection = getContext().getCollection();
    var collectionLink = collection.getSelfLink();

    // The count of imported docs, also used as current doc index.
    var count = 0;

    // Validate input.
    if (!docProperties) throw new Error("The array is undefined or null.");

    var docsLength = docProperties.length;
    if (docsLength == 0) {
        getContext().getResponse().setBody(0);
    }

    // Since we know all our docs have an id property,
    //  we can instruct Cosmos DB not to check for it.
    // This improves performance.
    var options = {
        disableAutomaticIdGeneration: true
    };

    // Call the CRUD API to create/update/delete a document.
    tryAction(docProperties[count], callback);

    // Note that there are 2 exit conditions:
    // 1) The createDocument/replaceDocument/deleteDocument request was not accepted. 
    //    In this case the callback will not be called, we just call setBody and we are done for this iteration.
    // 2) The callback was called docProperties.length times.
    //    In this case all documents were created and we don't need to call tryAction anymore. Just call setBody and we are done.
    function tryAction(properties, callback) {
        let isAccepted = false;
        if (properties.state === "Added") {
            isAccepted = collection.createDocument(collectionLink, properties.document, options, callback);
        }
        else if (properties.state === "Modified") {
            isAccepted = collection.replaceDocument(properties.documentLink, properties.document, options, callback);
        }
        else if (properties.state === "Deleted") {
            isAccepted = collection.deleteDocument(properties.documentLink, {}, callback);
        }

        // If the request was accepted, callback will be called.
        // Otherwise report current count back to the client, 
        // which will call the script again with remaining set of docs.
        // This condition will happen when this stored procedure has been running too long
        // and is about to get cancelled by the server. This will allow the calling client
        // to resume this batch from the point we got to before isAccepted was set to false
        if (! isAccepted) {
            getContext().getResponse().setBody(count);
        }
    }

    // This is called when collection.createDocument/replaceDocument is done and the document has been persisted.
    function callback(err, doc, options) {
        if (err) throw err;

        // One more document has been imported, increment the count.
        count++;

        if (count >= docsLength) {
            // If we have created all documents, we are done. Just set the response.
            getContext().getResponse().setBody(count);
        } else {
            // Create/update next document.
            tryAction(docProperties[count], callback);
        }
    }
}
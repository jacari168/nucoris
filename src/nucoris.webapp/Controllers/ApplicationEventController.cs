using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using nucoris.application.interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace nucoris.webapp.Controllers
{
    /// <summary>
    /// The architecture of nucoris supports what we call deferred events, which are domain events that are processed asynchronously.
    /// All domain events are sent to a service bus topic, and a function reads them and sends them back to the controller below.
    /// This controller then uses the mediator to publish them to the application layer.
    /// For further info see the comments on the DeferredDomainEvent class or the nucoris Persistence document.
    /// </summary>

    [Route("api/applicationEvents")]
    public class ApplicationEventsController : ControllerBase
    {
        private readonly MediatR.IMediator _mediator;
        private readonly ILogger<ApplicationEventsController> _logger;
        private readonly Assembly _domainTypesAssembly;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public ApplicationEventsController(MediatR.IMediator mediator, IUnitOfWork unitOfWork, ILogger<ApplicationEventsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
            _domainTypesAssembly = Assembly.GetAssembly(typeof(domain.DomainEvent));
        }


        [HttpPost]
        public async Task<IActionResult> NewApplicationEvent()
        {
            try
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(this.HttpContext.Request.Body, System.Text.Encoding.UTF8))
                {
                    var serializedDbDoc = await reader.ReadToEndAsync();
                    var dbDoc = JsonConvert.DeserializeObject<DbDocument<dynamic>>(serializedDbDoc);

                    if (dbDoc.docType == "Event")
                    {
                        var deferredEvent = GetDeferredEvent(dbDoc);
                        await _mediator.Publish(deferredEvent);
                    }
                }

                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Failed to handle new asynchronous application event. Error: {ex.ToString()}");

                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError,
                    $"Failed to handle new asynchronous application event. Reason: {ex.Message}");
            }
        }

        private object GetDeferredEvent(DbDocument<dynamic> dbDoc)
        {
            var eventType = _domainTypesAssembly.GetType(dbDoc.docSubType); // we know docSubType stores the full type name, by convention.
            var domainEvent = JsonConvert.DeserializeObject(dbDoc.docContents.ToString(), eventType, _jsonSettings);

            var deferredEventType = typeof(DeferredDomainEvent<>);
            Type[] typeArgs = { eventType };
            var constructedType = deferredEventType.MakeGenericType(typeArgs);
            var deferredEvent = Activator.CreateInstance(constructedType, domainEvent);
            return deferredEvent;
        }
    }
}

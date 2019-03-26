using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.webapp.Controllers
{
    [Route("api/patients/{patientId:Guid}/allergies")]
    public class AllergiesController : ControllerBase
    {
        private readonly MediatR.IMediator _mediator;
        private readonly ILogger<AllergiesController> _logger;

        public AllergiesController(MediatR.IMediator mediator, ILogger<AllergiesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("{allergy}")]
        public async Task<IActionResult> Post(Guid patientId, string allergy)
        {
            if (patientId == default(Guid))
            {
                return BadRequest($"Incorrect patientId: '{patientId}'");
            }
            else if (String.IsNullOrWhiteSpace(allergy))
            {
                return BadRequest($"Empty allergy");
            }

            try
            {
                var cmd = new application.commands.AddAllergiesCommand(patientId, new List<string>() { allergy });
                var result = await _mediator.Send(cmd);

                if (result.Successful)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status304NotModified,
                            $"Unable to add allergy {allergy} to patient {patientId}");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Failed to add allergy {allergy} to patient {patientId}. Error: {ex.ToString()}");

                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError,
                    $"Failed to add allergy {allergy} to patient {patientId}");
            }
        }

        [HttpDelete("{allergy}")]
        public async Task<IActionResult> Delete(Guid patientId, string allergy)
        {
            if( patientId == default(Guid))
            {
                return BadRequest($"Incorrect patientId: '{patientId}'");
            }
            else if(String.IsNullOrWhiteSpace(allergy))
            {
                return BadRequest($"Empty allergy");
            }

            try
            {
                var cmd = new application.commands.RemoveAllergiesCommand(patientId, new List<string>() { allergy });
                var result = await _mediator.Send(cmd);

                if(result.Successful)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status304NotModified,
                            $"Unable to remove allergy {allergy} of patient {patientId}");
                }
            }
            catch(System.Exception ex)
            {
                _logger.LogError($"Failed to add allergy {allergy} to patient {patientId}. Error: {ex.ToString()}");

                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError,
                    $"Failed to remove allergy {allergy} of patient {patientId}");
            }
        }
    }
}

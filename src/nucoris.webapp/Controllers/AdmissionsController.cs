using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace nucoris.webapp.Controllers
{
    [Route("api/patients/{patientId:Guid}/admissions")]
    public class AdmissionsController : ControllerBase
    {
        private readonly MediatR.IMediator _mediator;
        private readonly ILogger<AdmissionsController> _logger;

        public AdmissionsController(MediatR.IMediator mediator, ILogger<AdmissionsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpDelete("{admissionId:Guid}")]
        public async Task<IActionResult> Discharge(Guid patientId, Guid admissionId)
        {
            // Note that we consider a "Delete" method on an admission equivalent to end it.
            if( patientId == default(Guid))
            {
                return BadRequest($"Incorrect patientId: '{patientId}'");
            }
            else if(admissionId == default(Guid))
            {
                return BadRequest($"Incorrect admissionId: '{admissionId}'");
            }

            try
            {
                var cmd = new application.commands.EndAdmissionCommand(patientId, admissionId, DateTime.UtcNow);
                var result = await _mediator.Send(cmd);

                if(result.Successful)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status304NotModified,
                            $"Unable to discharge admission {admissionId} of patient {patientId}");
                }
            }
            catch(System.Exception ex)
            {
                _logger.LogError($"Failed to discharge admission {admissionId} of patient {patientId}. Error: {ex.ToString()}");

                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError,
                    $"Failed to discharge admission {admissionId} of patient {patientId}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Admit(Guid patientId)
        {
            // Note that we consider a "Post" method on patient/{id}/admissions equivalent to admit the patient
            //  (create a new admission)
            if (patientId == default(Guid))
            {
                return BadRequest($"Incorrect patientId: '{patientId}'");
            }
        
            try
            {
                var cmd = new application.commands.AdmitPatientCommand(patientId, DateTime.UtcNow);
                var result = await _mediator.Send(cmd);

                if (result.Successful)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status304NotModified,
                            $"Unable to admit patient {patientId}");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Failed to admit patient {patientId}. Error: {ex.ToString()}");

                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError,
                    $"Failed to admit patient {patientId}");
            }
        }
    }
}

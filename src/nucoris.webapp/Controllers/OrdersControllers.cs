using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using nucoris.application.interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.webapp.Controllers
{
    [Route("api/patients/{patientId:Guid}/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly MediatR.IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(MediatR.IMediator mediator, IUnitOfWork unitOfWork, ILogger<OrdersController> logger)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }


        [HttpPatch("{orderId:Guid}")]
        public async Task<IActionResult> AssignOrder(Guid patientId, Guid orderId, Guid? assignedUserId)
        {
            if (patientId == default(Guid))
            {
                return BadRequest($"Incorrect patientId: '{patientId}'");
            }
            else if (orderId == default(Guid))
            {
                return BadRequest($"Incorrect orderId: '{orderId}'");
            }

            try
            {
                application.commands.CommandResult result;

                if ( assignedUserId == null)
                {
                    var cmd = new application.commands.UnassignOrderCommand(patientId, orderId, _unitOfWork.CurrentUser.Id);
                    result = await _mediator.Send(cmd);
                }
                else
                {
                    if (assignedUserId.Value != default(Guid))
                    {
                        var cmd = new application.commands.AssignOrderCommand(patientId, orderId, assignedUserId.Value,
                            _unitOfWork.CurrentUser.Id);
                        result = await _mediator.Send(cmd);
                    }
                    else
                    {
                        return BadRequest($"Incorrect assigned user id '{assignedUserId}' for order '{orderId}'");
                    }
                }

                if (result.Successful)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status304NotModified,
                            $"Unable to assign order {orderId} of patient {patientId}");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Failed to assign order {orderId} of patient {patientId}. Error: {ex.ToString()}");
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError,
                    $"Failed to assign order {orderId} of patient {patientId}");
            }
        }

        [HttpDelete("{orderId:Guid}")]
        public async Task<IActionResult> StopOrder(Guid patientId, Guid orderId, string cancellationReason)
        {
            // We consider "deleting" an order is equivalent to stopping (cancelling) it

            if( patientId == default(Guid))
            {
                return BadRequest($"Incorrect patientId: '{patientId}'");
            }
            else if(orderId == default(Guid))
            {
                return BadRequest($"Incorrect orderId: '{orderId}'");
            }

            try
            {
                var cmd = new application.commands.CancelOrderCommand(patientId, orderId, cancellationReason, _unitOfWork.CurrentUser.Id);
                var result = await _mediator.Send(cmd);

                if(result.Successful)
                {
                    return Ok();
                }
                else
                {
                    return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status304NotModified,
                            $"Unable to stop order {orderId} of patient {patientId}");
                }
            }
            catch(System.Exception ex)
            {
                _logger.LogError($"Failed to stop order {orderId} of patient {patientId}. Error: {ex.ToString()}");

                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError,
                    $"Failed to stop order {orderId} of patient {patientId}");
            }
        }
    }
}

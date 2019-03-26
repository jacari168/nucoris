using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using nucoris.application.interfaces.repositories;
using nucoris.application.queries;
using nucoris.webapp.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.webapp.Pages
{
    public class PatientDetailsModel : PageModel
    {
        private readonly MediatR.IMediator _mediator;
        private readonly IOrderRepository _orderRepository;

        public PatientVM Patient { get; set; }

        public PatientDetailsModel(MediatR.IMediator mediator, IOrderRepository orderRepository)
        {
            _mediator = mediator;
            _orderRepository = orderRepository;
        }

        public async Task OnGetAsync(Guid patientId)
        {
            // Get all data of a given patient:
            var query = new PatientDetailsQuery(patientId);
            var patientDetails = await _mediator.Send(query);

            if(patientDetails != null)
            {
                this.Patient = new PatientVM(patientDetails);
            }
        }

        // Method to support partial view updates of just Allergies:
        public async Task<PartialViewResult> OnGetAllergies(Guid patientId)
        {
            // Get just allergies:
            var query = new PatientDetailsQuery(patientId);
            var patientDetails = await _mediator.Send(query);

            return new PartialViewResult
            {
                ViewName = "AllergyList",
                ViewData = new ViewDataDictionary<List<string>>(ViewData, patientDetails?.Allergies)
            };
        }

        // Method to support partial view updates of just an order:
        public async Task<PartialViewResult> OnGetOrder(Guid patientId, Guid orderId)
        {
            var order = await _orderRepository.GetAsync(patientId, orderId);
            var orderVM = new OrderVM(order);
 
            return new PartialViewResult
            {
                ViewName = "Order",
                ViewData = new ViewDataDictionary<OrderVM>(ViewData, orderVM)
            };
        }
    }
}
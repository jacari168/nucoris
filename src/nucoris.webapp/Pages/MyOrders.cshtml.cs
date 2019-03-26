using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nucoris.application.interfaces;
using nucoris.application.queries;
using nucoris.queries.ActiveOrdersView;
using nucoris.webapp.Models;

namespace nucoris.webapp.Pages
{
    public class MyOrdersModel : PageModel
    {
        private readonly MediatR.IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;

        public List<PatientVM> OrdersAssignedToMe { get; set; }

        public MyOrdersModel(MediatR.IMediator mediator, IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        public async Task OnGetAsync()
        {
            var user = _unitOfWork.CurrentUser;

            var specification = ActiveOrdersViewSpecification.ForUser(user.Id);
            var query = new ActiveOrdersQuery(specification);
            var orders = await _mediator.Send(query);

            this.OrdersAssignedToMe = new List<PatientVM>();

            var patientOrders = orders.GroupBy(o => o.QueryOrder.PatientId);

            foreach(var group in patientOrders)
            {
                var patient = new PatientVM(group.First());
                var order = group.First().QueryOrder;

                patient.ActiveAdmission.Orders = group.Select(o => new OrderVM(o)).ToList();

                this.OrdersAssignedToMe.Add(patient);
            }
        }
    }
}
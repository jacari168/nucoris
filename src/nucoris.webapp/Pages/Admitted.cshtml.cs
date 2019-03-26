using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nucoris.application.queries;
using nucoris.queries.PatientStateView;
using nucoris.webapp.Models;

namespace nucoris.webapp.Pages
{
    public class AdmittedModel : PageModel
    {
        private readonly MediatR.IMediator _mediator;

        public List<PatientVM> AdmittedPatients { get; set; }

        public AdmittedModel(MediatR.IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGetAsync()
        {
            // Get all admitted patients:
            var specification = new PatientStateViewSpecification(PatientAdmissionState.Admitted);
            var query = new PatientStateQuery(specification);
            var patients = await _mediator.Send(query);

            this.AdmittedPatients = patients.Select(qp => new PatientVM(qp)).
                OrderBy(vp => vp.DisplayName).ToList();
        }
    }
}
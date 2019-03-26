using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nucoris.application.queries;
using nucoris.webapp.Models;


namespace nucoris.webapp.Pages
{
    public class PatientAuditTrailModel : PageModel
    {
        private readonly MediatR.IMediator _mediator;

        public nucoris.queries.PatientAuditTrail.Patient PatientAuditTrail { get; set; }

        public PatientAuditTrailModel(MediatR.IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnGetAsync(Guid patientId)
        {
            var query = new PatientAuditTrailQuery(patientId);
            this.PatientAuditTrail = await _mediator.Send(query);
        }
    }
}
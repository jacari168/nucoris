using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nucoris.application.queries;
using nucoris.queries.PatientStateView;
using nucoris.webapp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nucoris.webapp.Pages
{
    public class SearchModel : PageModel
    {
        private readonly MediatR.IMediator _mediator;

        public List<PatientVM> FoundPatients { get; set; }

        public SearchModel(MediatR.IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task OnPostAsync(string mrn, string givenName, string familyName)
        {
            // Note that if user specifies no search criteria we'll return all patients.

            var specification = new PatientStateViewSpecification(PatientAdmissionState.Any, mrn, givenName, familyName);
            var query = new PatientStateQuery(specification);
            var patients = await _mediator.Send(query);

            this.FoundPatients = patients.OrderBy(qp => qp.QueryPatient.UpperFamilyName).ThenBy(qp => qp.QueryPatient.UpperGivenName).
                Select(qp => new PatientVM(qp)).ToList();
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using nucoris.application.queries;
using nucoris.queries.PatientStateView;
using nucoris.webapp.Models;

namespace nucoris.webapp.Pages
{
    public class NewAdmissionModel : PageModel
    {
        private readonly MediatR.IMediator _mediator;

        public List<PatientVM> FoundPatients { get; set; }

        [BindProperty]
        public NewPatientVM NewPatient { get; set; }

        public NewAdmissionModel(MediatR.IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> OnPostSearchAsync(string mrn)
        {
            if( ! String.IsNullOrWhiteSpace(mrn) )
            {
                // Search by MRN:
                var specification = new PatientStateViewSpecification(mrn);
                var query = new PatientStateQuery(specification);
                var patients = await _mediator.Send(query);

                if( patients.Any())
                {
                    this.FoundPatients = patients.Select(qp => new PatientVM(qp)).ToList();
                }      
                else
                {
                    // Initialize NewPatient to transfer Mrn to form to enter new patient's name and DOB.
                    this.NewPatient = new NewPatientVM() { Mrn = mrn };
                }

                return new PageResult();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAdmitAsync(Guid patientId)
        {
            if (patientId != default(Guid))
            {
                // Create Admit Command:
                var cmd = new application.commands.AdmitPatientCommand(patientId, DateTime.UtcNow);
                var result = await _mediator.Send(cmd);

                if (result.Successful)
                {
                    return RedirectToPage("PatientDetails", new { patientId });
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostNewPatientAsync()
        {
            if (ModelState.IsValid)
            {
                // Create New Patient and New Admit Commands:
                var patientCmd = new application.commands.CreatePatientCommand(
                    this.NewPatient.Mrn, this.NewPatient.GivenName, this.NewPatient.FamilyName, this.NewPatient.DateOfBirth);
                var patientResult = await _mediator.Send(patientCmd);

                if (patientResult.Successful)
                {
                    var patientId = patientResult.CreatedEntityId;
                    var admissionCmd = new application.commands.AdmitPatientCommand(patientId, DateTime.UtcNow);
                    var result = await _mediator.Send(admissionCmd);

                    if (result.Successful)
                    {
                        return RedirectToPage("PatientDetails", new { patientId });
                    }
                }
            }

            return Page();
        }
    }
}
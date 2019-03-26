using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using nucoris.application.interfaces;
using nucoris.application.interfaces.ReferenceData;
using nucoris.application.interfaces.repositories;
using nucoris.webapp.Models;

namespace nucoris.webapp.Pages
{
    public class NewOrderModel : PageModel
    {
        private readonly MediatR.IMediator _mediator;
        private readonly IPatientRepository _patientRepository;
        private readonly IReferenceDataRepository<MedicationForm, Guid> _medicationFormRepository;
        private readonly IReferenceDataRepository<AdministrationFrequency, Guid> _medicationFrequencyRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PatientVM Patient { get; private set; }
        public SelectList MedicationForms { get; private set; }
        public SelectList AdministrationFrequencies { get; private set; }

        [BindProperty]
        public NewOrderVM NewOrder { get; set; }

        public NewOrderModel(MediatR.IMediator mediator,
            IPatientRepository patientRepository,
            IReferenceDataRepository<MedicationForm,Guid> medicationFormRepository,
            IReferenceDataRepository<AdministrationFrequency, Guid> medicationFrequencyRepository,
            IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _patientRepository = patientRepository;
            _medicationFormRepository = medicationFormRepository;
            _medicationFrequencyRepository = medicationFrequencyRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task OnGetAsync(Guid patientId, Guid admissionId)
        {
            var patient = await _patientRepository.GetAsync(patientId);

            if (patient != null)
            {
                this.Patient = new PatientVM()
                {
                    Id = patient.Id,
                    DisplayName = patient.DisplayName,
                    ActiveAdmission = new AdmissionVM() { Id = admissionId}
                };

                var medforms = await _medicationFormRepository.GetManyAsync(whereConditions: null);
                this.MedicationForms = new SelectList(medforms.OrderBy(i => i.DisplayName),
                    nameof(MedicationForm.Id), nameof(MedicationForm.DisplayName));

                var medfrequencies = await _medicationFrequencyRepository.GetManyAsync(whereConditions: null);
                this.AdministrationFrequencies = new SelectList(medfrequencies.OrderBy(i => i.DisplayName),
                    nameof(AdministrationFrequency.Id), nameof(AdministrationFrequency.DisplayName));

                var now = DateTime.Now; // TODO: use UTC and translate to user's time zone
                var suggestedStartTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0) + new TimeSpan(1, 0, 0);

                this.NewOrder = new NewOrderVM()
                {
                    PatientId = patientId,
                    AdmissionId = admissionId,
                    StartTime = suggestedStartTime
                };
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Create New Order Command:
                var medform = await _medicationFormRepository.GetAsync(this.NewOrder.MedicationId);
                var medfrequency = await _medicationFrequencyRepository.GetAsync(this.NewOrder.FrequencyId);

                var cmd = new application.commands.PrescribeMedicationCommand(
                    this.NewOrder.PatientId, this.NewOrder.AdmissionId,
                    medform.Medication.Name, medform.Dose.Amount, medform.Dose.Unit.Name, medfrequency.Frequency.Name,
                    this.NewOrder.StartTime, this.NewOrder.StartTime.AddDays(100), // In this prototype we ignore EndTime
                    _unitOfWork.CurrentUser.Id);
                var result = await _mediator.Send(cmd);

                if (result.Successful)
                {
                    return RedirectToPage("PatientDetails", new { this.NewOrder.PatientId });
                }
            }

            return Page();
        }

    }
}
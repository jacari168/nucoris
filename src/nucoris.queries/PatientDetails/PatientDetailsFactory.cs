using System;
using System.Collections.Generic;
using System.Linq;

namespace nucoris.queries.PatientDetails
{
    public static class PatientDetailsFactory
    {
        public static Patient FromDomain(domain.Patient patient,
            List<domain.Admission> admissions, List<domain.Order> activeAdmissionOrders)
        {
            var patientDetails = new Patient()
            {
                Id = patient.Id,
                Admissions = admissions?.Select(domainAdmission => CreateAdmission(domainAdmission)).ToList(),
                Allergies = patient.Allergies?.Select(domainAllergy => domainAllergy.Name).ToList(),
                DateOfBirth = patient.DateOfBirth,
                FamilyName = patient.FamilyName,
                GivenName = patient.GivenName,
                DisplayName = patient.DisplayName,
                Mrn = patient.PatientIdentity.Mrn,
            };

            patientDetails.ActiveAdmission = CreateAdmissionDetails(admissions?.FirstOrDefault(a => a.IsActive), 
                                                                activeAdmissionOrders);

            return patientDetails;
        }

        private static Admission CreateAdmission(domain.Admission domainAdmission)
        {
            if (domainAdmission == null) return null;

            return new Admission()
            {
                Id = domainAdmission.Id,
                Started = domainAdmission.Started,
                Ended = domainAdmission.Ended
            };
        }

        private static AdmissionDetails CreateAdmissionDetails(domain.Admission domainAdmission, 
            List<domain.Order> activeAdmissionOrders)
        {
            if (domainAdmission == null) return null;

            return new AdmissionDetails()
            {
                Id = domainAdmission.Id,
                Started = domainAdmission.Started,
                Ended = domainAdmission.Ended,
                Orders = activeAdmissionOrders?.Select(domainOrder => CreateOrder(domainOrder)).
                    OrderBy(i => !i.IsActive).ThenBy(i => i.Description).
                    ToList()
            };

        }

        private static Order CreateOrder(domain.Order domainOrder)
        {
            if( domainOrder == null) return null;

            var order = new Order()
            {
                Id = domainOrder.Id,
                IsActive = domain.OrderStateExtensions.IsActive(domainOrder.State),
                Description = domainOrder.Description,
                AssignedUser = domainOrder.AssignedTo?.DisplayName,
            };

            var prescription = domainOrder as domain.MedicationPrescription;
            if( prescription != null)
            {
                order.Administrations = prescription.Administrations?.Select(domainAdministration =>
                            new MedicationAdministration()
                            {
                                AdministeredAt = domainAdministration.AdministeredAt,
                                AdministeredBy = domainAdministration.AdministeredBy.DisplayName
                            }).ToList();
            }

            return order;
        }
    }
}

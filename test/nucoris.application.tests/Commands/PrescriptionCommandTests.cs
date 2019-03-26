using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nucoris.application.commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace nucoris.application.tests
{
    [TestClass]
    public class PrescriptionCommandTests
    {
        [TestMethod]
        public async Task PrescribeMedicationCommand_WhenReceived_NewPrescriptionCreated()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_PrescribeMedicationCommand_WhenReceived_NewPrescriptionCreated_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.UtcNow
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);
                var patientId = result.CreatedEntityId;

                var startTime = DateTime.UtcNow;
                var admissionCmd = new AdmitPatientCommand(patientId, startTime);
                var admissionResult = await mediator.Send(admissionCmd);
                admissionResult.Successful.ShouldBe(true);
                var admissionId = admissionResult.CreatedEntityId;

                var endTime = startTime.AddDays(5);
                var prescriptionCmd = new PrescribeMedicationCommand(
                    patientId, admissionId,
                    "Diazepam", 5, "mg", "Q24H", startTime, endTime, ApplicationInitialize.TestUserId);
                var prescriptionResult = await mediator.Send(prescriptionCmd);
                prescriptionResult.Successful.ShouldBe(true);
                var prescriptionId = prescriptionResult.CreatedEntityId;

                var prescriptionRepo = scope.Resolve<interfaces.repositories.IMedicationPrescriptionRepository>();
                var prescription = await prescriptionRepo.GetAsync(patientId, prescriptionId);
                prescription.AdmissionId.ShouldBe(admissionId);
                prescription.AssignedTo.ShouldBeNull();
                prescription.Dose.ShouldBe(new domain.Dose(prescriptionCmd.DoseAmount,
                                                            new domain.Unit(prescriptionCmd.DoseUnitName)));
                prescription.EndAt.ShouldBe(endTime);
                prescription.Frequency.ShouldBe(new domain.Frequency(prescriptionCmd.FrequencySpecification));
                prescription.Id.ShouldBe(prescriptionId);
                prescription.Medication.ShouldBe(new domain.Medication(prescriptionCmd.MedicationName));
                prescription.OrderedAt.ShouldBe(DateTime.UtcNow, new TimeSpan(0, 1, 0));
                prescription.OrderedBy.Id.ShouldBe(ApplicationInitialize.TestUserId);
                prescription.PatientIdentity.Id.ShouldBe(patientId);
                prescription.PatientIdentity.Mrn.ShouldBe(cmd.Mrn);
                prescription.StartAt.ShouldBe(startTime);
                prescription.State.ShouldBe(domain.OrderState.Created);

                var orderRepo = scope.Resolve<interfaces.repositories.IOrderRepository>();
                var allOrders = await orderRepo.GetOfAdmissionAsync(
                    patientId, admissionId, domain.OrderStateExtensions.GetActiveStates());
                allOrders.Count.ShouldBe(1);
                allOrders.First().Id.ShouldBe(prescriptionId);
            }
        }

        [TestMethod]
        public async Task AdministerMedicationCommand_WhenReceived_MedicationAdministered()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_AdministerMedicationCommand_WhenReceived_MedicationAdministered_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.UtcNow
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);
                var patientId = result.CreatedEntityId;

                var startTime = DateTime.UtcNow;
                var admissionCmd = new AdmitPatientCommand(patientId, startTime);
                var admissionResult = await mediator.Send(admissionCmd);
                admissionResult.Successful.ShouldBe(true);
                var admissionId = admissionResult.CreatedEntityId;

                var endTime = startTime.AddDays(5);
                var prescriptionCmd = new PrescribeMedicationCommand(
                    patientId, admissionId,
                    "Diazepam", 5, "mg", "Q24H", startTime, endTime, ApplicationInitialize.TestUserId);
                var prescriptionResult = await mediator.Send(prescriptionCmd);
                prescriptionResult.Successful.ShouldBe(true);
                var prescriptionId = prescriptionResult.CreatedEntityId;

                var prescriptionRepo = scope.Resolve<interfaces.repositories.IMedicationPrescriptionRepository>();
                var prescription = await prescriptionRepo.GetAsync(patientId, prescriptionId);
                prescription.Administrations.Count.ShouldBe(0);

                var administrationTime = DateTime.UtcNow;
                var administrationCmd = new AdministerMedicationCommand(
                    patientId, prescriptionId, ApplicationInitialize.TestUserId, administrationTime);
                var administrationResult = await mediator.Send(administrationCmd);
                prescriptionResult.Successful.ShouldBe(true);

                prescription = await prescriptionRepo.GetAsync(patientId, prescriptionId);
                prescription.State.ShouldBe(domain.OrderState.InProgress);
                prescription.Administrations.Count.ShouldBe(1);
                var administration = prescription.Administrations.First();
                administration.AdministeredAt.ShouldBe(administrationTime);
                administration.AdministeredBy.Id.ShouldBe(ApplicationInitialize.TestUserId);

                // Second administration:
                administrationTime = DateTime.UtcNow;
                administrationCmd = new AdministerMedicationCommand(
                    patientId, prescriptionId, ApplicationInitialize.TestUserId, administrationTime);
                administrationResult = await mediator.Send(administrationCmd);
                prescriptionResult.Successful.ShouldBe(true);

                prescription = await prescriptionRepo.GetAsync(patientId, prescriptionId);
                prescription.State.ShouldBe(domain.OrderState.InProgress);
                prescription.Administrations.Count.ShouldBe(2);
                administration = prescription.Administrations.Last();
                administration.AdministeredAt.ShouldBe(administrationTime);
                administration.AdministeredBy.Id.ShouldBe(ApplicationInitialize.TestUserId);
            }
        }

        [TestMethod]
        public async Task AssignOrderCommand_WhenReceived_OrderAssigned()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_AssignOrderCommand_WhenReceived_OrderAssigned_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.UtcNow
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);
                var patientId = result.CreatedEntityId;

                var startTime = DateTime.UtcNow;
                var admissionCmd = new AdmitPatientCommand(patientId, startTime);
                var admissionResult = await mediator.Send(admissionCmd);
                admissionResult.Successful.ShouldBe(true);
                var admissionId = admissionResult.CreatedEntityId;

                var endTime = startTime.AddDays(5);
                var prescriptionCmd = new PrescribeMedicationCommand(
                    patientId, admissionId,
                    "Diazepam", 5, "mg", "Q24H", startTime, endTime, ApplicationInitialize.TestUserId);
                var prescriptionResult = await mediator.Send(prescriptionCmd);
                prescriptionResult.Successful.ShouldBe(true);
                var prescriptionId = prescriptionResult.CreatedEntityId;

                var prescriptionRepo = scope.Resolve<interfaces.repositories.IMedicationPrescriptionRepository>();
                var prescription = await prescriptionRepo.GetAsync(patientId, prescriptionId);
                prescription.State.ShouldBe(domain.OrderState.Created);

                var assignCmd = new AssignOrderCommand(
                    patientId, prescriptionId, ApplicationInitialize.TestUserId, ApplicationInitialize.TestUserId);
                var assignResult = await mediator.Send(assignCmd);
                assignResult.Successful.ShouldBe(true);

                prescription = await prescriptionRepo.GetAsync(patientId, prescriptionId);
                prescription.State.ShouldBe(domain.OrderState.Ready);
                prescription.AssignedTo.Id.ShouldBe(ApplicationInitialize.TestUserId);
            }
        }

        [TestMethod]
        public async Task CancelMedicationCommand_WhenReceived_PrescriptionCancelled()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_CancelMedicationCommand_WhenReceived_PrescriptionCancelled_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.UtcNow
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);
                var patientId = result.CreatedEntityId;

                var startTime = DateTime.UtcNow;
                var admissionCmd = new AdmitPatientCommand(patientId, startTime);
                var admissionResult = await mediator.Send(admissionCmd);
                admissionResult.Successful.ShouldBe(true);
                var admissionId = admissionResult.CreatedEntityId;

                var endTime = startTime.AddDays(5);
                var prescriptionCmd = new PrescribeMedicationCommand(
                    patientId, admissionId,
                    "Diazepam", 5, "mg", "Q24H", startTime, endTime, ApplicationInitialize.TestUserId);
                var prescriptionResult = await mediator.Send(prescriptionCmd);
                prescriptionResult.Successful.ShouldBe(true);
                var prescriptionId = prescriptionResult.CreatedEntityId;

                var reason = $"Prescription {prescriptionId} cancelled by test";
                var cancelCmd = new CancelOrderCommand(
                    patientId, prescriptionId, reason, ApplicationInitialize.TestUserId);
                var cancelResult = await mediator.Send(cancelCmd);
                cancelResult.Successful.ShouldBe(true);

                var orderRepo = scope.Resolve<interfaces.repositories.IOrderRepository>();
                var cancelledOrders = await orderRepo.GetOfAdmissionAsync(
                    patientId, admissionId, new List<domain.OrderState>() { domain.OrderState.Cancelled });
                cancelledOrders.Count.ShouldBe(1);
                cancelledOrders[0].Id.ShouldBe(prescriptionId);
                cancelledOrders[0].CancellationReason.ShouldBe(reason);
            }
        }

        [TestMethod]
        public async Task Admission_WhenEnded_AllItsPrescriptionsCancelled()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_Admission_WhenEnded_AllItsPrescriptionsCancelledd_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.UtcNow
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);
                var patientId = result.CreatedEntityId;

                var startTime = DateTime.UtcNow;
                var admissionCmd = new AdmitPatientCommand(patientId, startTime);
                var admissionResult = await mediator.Send(admissionCmd);
                admissionResult.Successful.ShouldBe(true);
                var admissionId = admissionResult.CreatedEntityId;

                var endTime = startTime.AddDays(5);
                var prescriptionCmd = new PrescribeMedicationCommand(
                    patientId, admissionId,
                    "Diazepam", 5, "mg", "Q24H", startTime, endTime, ApplicationInitialize.TestUserId);
                var prescriptionResult = await mediator.Send(prescriptionCmd);
                prescriptionResult.Successful.ShouldBe(true);
                var prescriptionId = prescriptionResult.CreatedEntityId;

                var admissionEndTime = DateTime.UtcNow;
                var endAdmissionCmd = new EndAdmissionCommand(patientId, admissionId, admissionEndTime);
                var endAdmissionResult = await mediator.Send(endAdmissionCmd);
                endAdmissionResult.Successful.ShouldBe(true);


                var prescriptionRepo = scope.Resolve<interfaces.repositories.IMedicationPrescriptionRepository>();
                var prescription = await prescriptionRepo.GetAsync(patientId, prescriptionId);
                prescription.AdmissionId.ShouldBe(admissionId);
                prescription.AssignedTo.ShouldBeNull();
                prescription.State.ShouldBe(domain.OrderState.Cancelled);

                var orderRepo = scope.Resolve<interfaces.repositories.IOrderRepository>();
                var activeOrders = await orderRepo.GetOfAdmissionAsync(
                    patientId, admissionId, domain.OrderStateExtensions.GetActiveStates());
                activeOrders.Count.ShouldBe(0);

                var cancelledPrescriptions = await orderRepo.GetOfAdmissionAsync(
                    patientId, admissionId, new List<domain.OrderState>() { domain.OrderState.Cancelled });
                cancelledPrescriptions.Count.ShouldBe(1);
                cancelledPrescriptions.First().Id.ShouldBe(prescriptionId);
            }
        }
    }
}

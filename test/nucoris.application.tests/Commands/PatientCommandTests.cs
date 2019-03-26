using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nucoris.application.commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using nucoris.domain;

namespace nucoris.application.tests
{
    [TestClass]
    public class PatientCommandTests
    {
        [TestMethod]
        public async Task CreatePatientCommand_WhenReceived_PatientCreated()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_CreatePatientCommand_WhenReceived_PatientCreated_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.Now
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);

                var patientId = result.CreatedEntityId;
                var patientRepo = scope.Resolve<interfaces.repositories.IPatientRepository>();
                var patient = await patientRepo.GetAsync(patientId);
                patient.PatientIdentity.Mrn.ShouldBe(cmd.Mrn);
                patient.PatientIdentity.Id.ShouldBe(patientId);
                patient.GivenName.ShouldBe(cmd.GivenName);
                patient.FamilyName.ShouldBe(cmd.FamilyName);
                patient.HasSystemGeneratedMrn.ShouldBe(false);
                patient.DateOfBirth.ShouldBe(cmd.DateOfBirth);
                patient.Allergies.ShouldBeEmpty();

                var eventRepo = scope.Resolve<interfaces.repositories.IEventRepository>();
                var events = await eventRepo.GetPatientEventsAsync(patientId);
                events.Count.ShouldBe(1);
                var patientCreated = events.First();
                patientCreated.ShouldBeOfType<PatientCreatedEvent>();
            }
        }

        [TestMethod]
        public async Task CreatePatientCommand_WhenDuplicate_DoNothing()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_CreatePatientCommand_WhenDuplicate_DoNothing_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.Now
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);
                var patientId = result.CreatedEntityId;

                // resend same command
                result = await mediator.Send(cmd);
                result.Successful.ShouldBe(false);

                var patientRepo = scope.Resolve<interfaces.repositories.IPatientRepository>();
                var patient = await patientRepo.GetAsync(patientId);
                patient.PatientIdentity.Mrn.ShouldBe(cmd.Mrn);
                patient.PatientIdentity.Id.ShouldBe(patientId);
                patient.GivenName.ShouldBe(cmd.GivenName);
                patient.FamilyName.ShouldBe(cmd.FamilyName);
                patient.HasSystemGeneratedMrn.ShouldBe(false);
                patient.DateOfBirth.ShouldBe(cmd.DateOfBirth);
                patient.Allergies.ShouldBeEmpty();
            }
        }

        [TestMethod]
        public async Task AddAllergiesCommand_WhenReceived_AllergyAdded()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_AddAllergiesCommand_WhenReceived_AllergyAdded_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.Now
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);
                var patientId = result.CreatedEntityId;

                var allergyCmd = new AddAllergiesCommand(patientId, new string[] { "Nuts", "dust" });
                var allergyResult = await mediator.Send(allergyCmd);
                allergyResult.Successful.ShouldBe(true);

                var patientRepo = scope.Resolve<interfaces.repositories.IPatientRepository>();
                var patient = await patientRepo.GetAsync(patientId);
                patient.PatientIdentity.Mrn.ShouldBe(cmd.Mrn);
                patient.PatientIdentity.Id.ShouldBe(patientId);
                patient.GivenName.ShouldBe(cmd.GivenName);
                patient.FamilyName.ShouldBe(cmd.FamilyName);
                patient.HasSystemGeneratedMrn.ShouldBe(false);
                patient.DateOfBirth.ShouldBe(cmd.DateOfBirth);
                patient.Allergies.ShouldContain(new domain.Allergy("nuts"));
                patient.Allergies.ShouldContain(new domain.Allergy("dust"));
            }
        }

        [TestMethod]
        public async Task RemoveAllergiesCommand_WhenReceived_AllergyRemoved()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_RemoveAllergiesCommand_WhenReceived_AllergyRemoved_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.UtcNow
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);
                var patientId = result.CreatedEntityId;

                var allergyCmd = new AddAllergiesCommand(patientId, new string[] { "Nuts", "dust" });
                var allergyResult = await mediator.Send(allergyCmd);
                allergyResult.Successful.ShouldBe(true);

                var removeAllergyCmd = new RemoveAllergiesCommand(patientId, new string[] { "Ddddust", "Dust" });
                var removeAllergyResult = await mediator.Send(removeAllergyCmd);
                removeAllergyResult.Successful.ShouldBe(true);

                var patientRepo = scope.Resolve<interfaces.repositories.IPatientRepository>();
                var patient = await patientRepo.GetAsync(patientId);
                patient.PatientIdentity.Mrn.ShouldBe(cmd.Mrn);
                patient.PatientIdentity.Id.ShouldBe(patientId);
                patient.GivenName.ShouldBe(cmd.GivenName);
                patient.FamilyName.ShouldBe(cmd.FamilyName);
                patient.HasSystemGeneratedMrn.ShouldBe(false);
                patient.DateOfBirth.ShouldBe(cmd.DateOfBirth);
                patient.Allergies.Count.ShouldBe(1);
                patient.Allergies.ShouldContain(new domain.Allergy("nuts"));

                var eventRepo = scope.Resolve<interfaces.repositories.IEventRepository>();
                var events = await eventRepo.GetPatientEventsAsync(patient.PatientIdentity.Id);
                events.Count(e => e is AllergyAddedEvent).ShouldBe(2); // added nuts, dust
                events.Count(e => e is AllergyRemovedEvent).ShouldBe(1); // removed dust
                ((AllergyRemovedEvent)events.First(e => e is AllergyRemovedEvent)).Allergy.Name.ShouldBe("Dust");
            }
        }

        [TestMethod]
        public async Task Patient_CanGetFullPatientRecord()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_Patient_CanGetAllItsChildrenEntities_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.UtcNow
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);
                var patientId = result.CreatedEntityId;

                var allergyCmd = new AddAllergiesCommand(patientId, new string[] { "Nuts" });
                var allergyResult = await mediator.Send(allergyCmd);
                allergyResult.Successful.ShouldBe(true);

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

                var patientRepo = scope.Resolve<interfaces.repositories.IPatientRepository>();
                var allChildren = await patientRepo.GetFullPatientRecordAsync(patientId);
                allChildren.Count(i => i is Patient).ShouldBe(1);
                allChildren.Count(i => i is Admission).ShouldBe(1);
                allChildren.Count(i => i is MedicationPrescription).ShouldBe(1);
                allChildren.Count(i => i is DomainEvent).ShouldBe(5); // one per command, except AdmitPatientCommand that generates 2
            }
        }
    }
}

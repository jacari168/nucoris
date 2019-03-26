using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nucoris.application.commands;
using nucoris.application.queries;
using nucoris.application.interfaces;
using nucoris.queries.PatientStateView;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace nucoris.application.tests
{
    [TestClass]
    public class PatientStateQueryTests
    {
        [TestMethod]
        public async Task PatientStateQuery_WhenPatientStateChanges_QueryItemConsistentlyFollowsIt()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_PatientStateQuery_WhenPatientStateChanges_QueryItemConsistentlyFollowsIt_" + guid,
                    givenName: "GN_" + guid,
                    familyName: null,
                    dateOfBirth: DateTime.UtcNow
                    );

                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);

                var patientId = result.CreatedEntityId;
                var mrn = cmd.Mrn;
                var queryRepo = scope.Resolve<IMaterializedViewRepository<PatientStateViewItem,PatientStateViewSpecification>>();

                // Patient currently not admitted, so searching just by MRN should find it, but not if searching by Admitted too:
                var specification = new PatientStateViewSpecification( mrn);
                var query = new PatientStateQuery(specification);
                var items = await mediator.Send(query);
                items.ShouldNotBeNull();
                items.Count.ShouldBe(1);
                var item = items.First();
                item.Id.ShouldBe(patientId); // id of item is the same id as patient
                item.QueryPatient.Id.ShouldBe(patientId);
                item.QueryPatient.Mrn.ShouldBe(cmd.Mrn);
                item.QueryPatient.GivenName.ShouldBe(cmd.GivenName);
                item.QueryPatient.FamilyName.ShouldBe(cmd.FamilyName);
                item.QueryPatient.DateOfBirth.ShouldBe(cmd.DateOfBirth);
                item.QueryPatient.Admissions.ShouldBeEmpty();

                specification = new PatientStateViewSpecification(PatientAdmissionState.Admitted, mrn, givenName: null, familyName: null);
                query = new PatientStateQuery(specification);
                items = await mediator.Send(query);
                items.ShouldNotBeNull();
                items.Count.ShouldBe(0);

                // Now after admitting it we should find it:
                var startTime = DateTime.UtcNow;
                var admissionCmd = new AdmitPatientCommand(patientId, startTime);
                var admissionResult = await mediator.Send(admissionCmd);
                admissionResult.Successful.ShouldBe(true);
                var admissionId = admissionResult.CreatedEntityId;

                specification = new PatientStateViewSpecification(PatientAdmissionState.Admitted, mrn, givenName: null, familyName: null);
                query = new PatientStateQuery(specification);
                items = await mediator.Send(query);
                items.Count.ShouldBe(1);
                item = items.First();
                item.Id.ShouldBe(patientId); // id of item is the same id as patient
                item.QueryPatient.Id.ShouldBe(patientId);
                item.QueryPatient.Mrn.ShouldBe(cmd.Mrn);
                item.QueryPatient.GivenName.ShouldBe(cmd.GivenName);
                item.QueryPatient.FamilyName.ShouldBe(cmd.FamilyName);
                item.QueryPatient.DateOfBirth.ShouldBe(cmd.DateOfBirth);
                item.QueryPatient.Admissions.Count.ShouldBe(1);
                var admission = item.QueryPatient.Admissions.First();
                admission.Id.ShouldBe(admissionId);
                admission.Started.ShouldBe(startTime);
                admission.Ended.ShouldBeNull();
                admission.IsActive.ShouldBe(true);

                // If we discharge it, we'll no longer find it as admitted:
                var endTime = DateTime.UtcNow;
                var endAdmissionCmd = new EndAdmissionCommand(patientId, admissionId, endTime);
                var endAdmissionResult = await mediator.Send(endAdmissionCmd);
                endAdmissionResult.Successful.ShouldBe(true);

                specification = new PatientStateViewSpecification(PatientAdmissionState.Admitted, mrn, givenName: null, familyName: null);
                query = new PatientStateQuery(specification);
                items = await mediator.Send(query);
                items.Count.ShouldBe(0);

                // But we'll find it as ended:
                specification = new PatientStateViewSpecification(PatientAdmissionState.Discharged, mrn, givenName: null, familyName: null);
                query = new PatientStateQuery(specification);
                items = await mediator.Send(query);
                items.Count.ShouldBe(1);
                item = items.First();
                item.Id.ShouldBe(patientId); // id of item is the same id as patient
                item.QueryPatient.Id.ShouldBe(patientId);
                item.QueryPatient.Mrn.ShouldBe(cmd.Mrn);
                item.QueryPatient.GivenName.ShouldBe(cmd.GivenName);
                item.QueryPatient.FamilyName.ShouldBe(cmd.FamilyName);
                item.QueryPatient.DateOfBirth.ShouldBe(cmd.DateOfBirth);
                item.QueryPatient.Admissions.Count.ShouldBe(1);
                admission = item.QueryPatient.Admissions.First();
                admission.Id.ShouldBe(admissionId);
                admission.Started.ShouldBe(startTime);
                admission.Ended.ShouldBe(endTime);
                admission.IsActive.ShouldBe(false);
            }
        }

        [TestMethod]
        public async Task PatientStateQuery_WhenPatientHasMultipleAdmissions_QueryItemConsistentlyFollowsIt()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_PatientStateQuery_WhenPatientStateChanges_QueryItemConsistentlyFollowsIt_" + guid,
                    givenName: "GN_" + guid,
                    familyName: "FN_" + guid,
                    dateOfBirth: DateTime.UtcNow
                    );

                // Create patient
                var mediator = scope.Resolve<MediatR.IMediator>();
                var result = await mediator.Send(cmd);
                result.Successful.ShouldBe(true);
                var patientId = result.CreatedEntityId;
                var mrn = cmd.Mrn;

                // Admit it
                var startTime = DateTime.UtcNow;
                var admissionCmd = new AdmitPatientCommand(patientId, startTime);
                var admissionResult = await mediator.Send(admissionCmd);
                admissionResult.Successful.ShouldBe(true);
                var admissionId = admissionResult.CreatedEntityId;

                // Discharge it
                var endTime = DateTime.UtcNow;
                var endAdmissionCmd = new EndAdmissionCommand(patientId, admissionId, endTime);
                var endAdmissionResult = await mediator.Send(endAdmissionCmd);
                endAdmissionResult.Successful.ShouldBe(true);

                // Second admission:
                var startTime2 = DateTime.UtcNow;
                var admissionCmd2 = new AdmitPatientCommand(patientId, startTime2);
                var admissionResult2 = await mediator.Send(admissionCmd2);
                admissionResult2.Successful.ShouldBe(true);
                var admissionId2 = admissionResult2.CreatedEntityId;


                // Now query for it:
                var queryRepo = scope.Resolve<IMaterializedViewRepository<PatientStateViewItem, PatientStateViewSpecification>>();

                var specification = new PatientStateViewSpecification(PatientAdmissionState.Admitted, mrn, givenName: null, familyName: null);
                var query = new PatientStateQuery(specification);
                var items = await mediator.Send(query);
                items.ShouldNotBeNull();
                items.Count.ShouldBe(1);
                var item = items.First();
                item.Id.ShouldBe(patientId); // id of item is the same id as patient
                item.QueryPatient.Id.ShouldBe(patientId);
                item.QueryPatient.Mrn.ShouldBe(cmd.Mrn);
                item.QueryPatient.GivenName.ShouldBe(cmd.GivenName);
                item.QueryPatient.FamilyName.ShouldBe(cmd.FamilyName);
                item.QueryPatient.DateOfBirth.ShouldBe(cmd.DateOfBirth);
                item.QueryPatient.Admissions.Count.ShouldBe(2);

                var inactiveAdmission = item.QueryPatient.Admissions.First(i => !i.IsActive);
                inactiveAdmission.Id.ShouldBe(admissionId);
                inactiveAdmission.Started.ShouldBe(startTime);
                inactiveAdmission.Ended.ShouldBe(endTime);
                inactiveAdmission.IsActive.ShouldBe(false);

                var activeAdmission = item.QueryPatient.Admissions.First(i => i.IsActive);
                activeAdmission.Id.ShouldBe(admissionId2);
                activeAdmission.Started.ShouldBe(startTime2);
                activeAdmission.Ended.ShouldBeNull();
                activeAdmission.IsActive.ShouldBe(true);
            }
        }

    }
}

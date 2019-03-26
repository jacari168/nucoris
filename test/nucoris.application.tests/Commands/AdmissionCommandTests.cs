using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nucoris.application.commands;
using System;
using System.Threading.Tasks;
using Shouldly;

namespace nucoris.application.tests
{
    [TestClass]
    public class AdmissionCommandTests
    {
        [TestMethod]
        public async Task AdmitPatientCommand_WhenReceived_NewAdmissionCreated()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_AdmitPatientCommand_WhenReceived_NewAdmissionCreated_" + guid,
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

                var admissionRepo = scope.Resolve<interfaces.repositories.IAdmissionRepository>();
                var admission = await admissionRepo.GetAsync(patientId, admissionId);
                admission.Started.ShouldBe(startTime);
                admission.Ended.ShouldBeNull();
                admission.IsActive.ShouldBe(true);
            }
        }

        [TestMethod]
        public async Task EndAdmissionCommand_WhenReceived_AdmissionEnded()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_EndAdmissionCommand_WhenReceived_AdmissionEnded_" + guid,
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
                var endTime = DateTime.UtcNow;
                var endAdmissionCmd = new EndAdmissionCommand(patientId, admissionId, endTime);
                var endAdmissionResult = await mediator.Send(endAdmissionCmd);
                endAdmissionResult.Successful.ShouldBe(true);

                var admissionRepo = scope.Resolve<interfaces.repositories.IAdmissionRepository>();
                var admission = await admissionRepo.GetAsync(patientId, admissionId);
                admission.Started.ShouldBe(startTime);
                admission.Ended.ShouldBe(endTime);
            }
        }
    }
}

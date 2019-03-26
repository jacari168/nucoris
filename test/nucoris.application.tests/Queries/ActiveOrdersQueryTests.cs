using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nucoris.application.commands;
using nucoris.application.interfaces.repositories;
using nucoris.application.queries;
using nucoris.application.interfaces;
using nucoris.queries.ActiveOrdersView;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nucoris.application.tests
{
    [TestClass]
    public class ActiveOrdersQueryTests
    {
        // The ActiveOrdersQuery partition is expected to be maintained asynchronously
        //  via Azure Service Bus, so we can't simulate its full workflow in an automated test.
        // What we do here is only to verify that the CRUD operations on its repository work as expected.

        [TestMethod]
        public async Task ActiveOrdersQueryTests_CRUDOperations_WorkAsExpected()
        {
            using (var scope = ApplicationInitialize.AutofacContainer.BeginLifetimeScope())
            {
                var guid = Guid.NewGuid().ToString();

                CreatePatientCommand cmd = new CreatePatientCommand(
                    mrn: "MRN_ActiveOrdersQueryTests_CRUDOperations_WorkAsExpected_" + guid,
                    givenName: "GN_" + guid,
                    familyName: null,
                    dateOfBirth: DateTime.UtcNow
                    );

                // Create patient, admit it and prescribe medication:
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

                // After creating a medication a MedicationPrescribedEvent should go into the bus,
                //  to be picked by a listener. The listener will then update the ActiveOrdersQuery in the DB.
                //  Here this process is simulated and encapsulated by this method:
                var orderEvents = await UpdateQueryInDbAsync(scope, patientId);

                // We should be able to retrieve now the new item:
                var specification = new ActiveOrdersViewSpecification(patientId: patientId, assignedUserId: null, orderStates: null);
                var query = new ActiveOrdersQuery(specification);
                var items = await mediator.Send(query);
                items.ShouldNotBeNull();
                items.Count.ShouldBe(1);
                var retrievedItem = items.First();
                retrievedItem.Id.ShouldBe(prescriptionId);
                retrievedItem.QueryOrder.AdmissionId.ShouldBe(admissionId);
                retrievedItem.QueryOrder.AssignedUserId.ShouldBeNull();
                retrievedItem.QueryOrder.Description.ShouldBe(orderEvents.First().Order.Description);
                retrievedItem.QueryOrder.FamilyName.ShouldBe(cmd.FamilyName);
                retrievedItem.QueryOrder.GivenName.ShouldBe(cmd.GivenName);
                retrievedItem.QueryOrder.Id.ShouldBe(prescriptionId);
                retrievedItem.QueryOrder.Mrn.ShouldBe(cmd.Mrn);
                retrievedItem.QueryOrder.OrderState.ShouldBe(OrderState.Created);
                retrievedItem.QueryOrder.PatientId.ShouldBe(patientId);

                // Let's create and assign a new prescription now:
                prescriptionCmd = new PrescribeMedicationCommand(
                    patientId, admissionId,
                    "Diazepam", 10, "mg", "Q24H", startTime, endTime, ApplicationInitialize.TestUserId);
                prescriptionResult = await mediator.Send(prescriptionCmd);
                prescriptionResult.Successful.ShouldBe(true);
                var prescriptionId2 = prescriptionResult.CreatedEntityId;
                orderEvents = await UpdateQueryInDbAsync(scope, patientId);

                var assignCmd = new AssignOrderCommand(
                    patientId, prescriptionId2, ApplicationInitialize.TestUserId, ApplicationInitialize.TestUserId);
                var assignResult = await mediator.Send(assignCmd);
                assignResult.Successful.ShouldBe(true);
                orderEvents = await UpdateQueryInDbAsync(scope, patientId);

                // If we query by patientId we should find the two items:
                specification = new ActiveOrdersViewSpecification(patientId: patientId, assignedUserId: null, orderStates: null);
                query = new ActiveOrdersQuery(specification);
                items = await mediator.Send(query);
                items.Count.ShouldBe(2);

                // But only one of them it's been assigned, so we should find only the latest one if querying by userid:
                specification = new ActiveOrdersViewSpecification(
                    patientId: patientId, assignedUserId: ApplicationInitialize.TestUserId, orderStates: null);
                query = new ActiveOrdersQuery(specification);
                items = await mediator.Send(query);
                items.ShouldNotBeNull();
                items.Count.ShouldBe(1);
                retrievedItem = items.First();
                retrievedItem.Id.ShouldBe(prescriptionId2);
                retrievedItem.QueryOrder.AdmissionId.ShouldBe(admissionId);
                retrievedItem.QueryOrder.AssignedUserId.ShouldBe(ApplicationInitialize.TestUserId);
                retrievedItem.QueryOrder.Description.ShouldBe(orderEvents.First().Order.Description);
                retrievedItem.QueryOrder.FamilyName.ShouldBe(cmd.FamilyName);
                retrievedItem.QueryOrder.GivenName.ShouldBe(cmd.GivenName);
                retrievedItem.QueryOrder.Id.ShouldBe(prescriptionId2);
                retrievedItem.QueryOrder.Mrn.ShouldBe(cmd.Mrn);
                retrievedItem.QueryOrder.OrderState.ShouldBe(OrderState.Ready); // state changes to Ready after assignment
                retrievedItem.QueryOrder.PatientId.ShouldBe(patientId);

                // If we discharge it, all active orders (here, 2) should be cancelled and removed from the query,
                //  which only deals with active orders:
                var endAdmissionTime = DateTime.UtcNow;
                var endAdmissionCmd = new EndAdmissionCommand(patientId, admissionId, endAdmissionTime);
                var endAdmissionResult = await mediator.Send(endAdmissionCmd);
                endAdmissionResult.Successful.ShouldBe(true);
                orderEvents = await UpdateQueryInDbAsync(scope, patientId, eventsToRead: 2);

                specification = new ActiveOrdersViewSpecification(
                    patientId: patientId, assignedUserId: null, orderStates: null);
                query = new ActiveOrdersQuery(specification);
                items = await mediator.Send(query);
                items.ShouldNotBeNull();
                items.Count.ShouldBe(0);
            }
        }

        private static async Task<IEnumerable<domain.OrderEvent>> UpdateQueryInDbAsync(ILifetimeScope scope, Guid patientId,
            int eventsToRead = 1)
        {
            var queryRepo = scope.Resolve<IMaterializedViewRepository<ActiveOrdersViewItem, ActiveOrdersViewSpecification>>();

            // The orders event listener will need to load the patient (to get its name)
            //  and insert/update the query item in the active orders query:
            var patientRepo = scope.Resolve<IPatientRepository>();
            var patient = await patientRepo.GetAsync(patientId);

            var eventRepo = scope.Resolve<IEventRepository>();
            var events = await eventRepo.GetPatientEventsAsync(patientId);
            var filteredEvents = events.Where(i => i is domain.OrderEvent).
                                Cast<domain.OrderEvent>().TakeLast(eventsToRead);

            foreach (var orderEvent in filteredEvents)
            {
                var queryItem = await queryRepo.GetAsync(orderEvent.Order.Id);
                if (queryItem == null)
                {
                    queryItem = ActiveOrdersViewItemFactory.FromDomain(patient, orderEvent.Order);
                }
                else
                {
                    queryItem = ActiveOrdersViewItemFactory.FromExisting(queryItem, orderEvent.Order);
                }

                if (queryItem.QueryOrder.OrderState == OrderState.Inactive)
                {
                    // By definition, 
                    queryRepo.Remove(queryItem);
                }
                else
                {
                    queryRepo.Store(queryItem);
                }
            }

            // We have to force writing to the DB:
            var cmdSession = scope.Resolve<interfaces.ICommandSession>();
            await cmdSession.CommitAsync();

            return filteredEvents;
        }
    }
}

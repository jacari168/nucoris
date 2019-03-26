using Ardalis.GuardClauses;
using nucoris.domain;
using nucoris.application.interfaces;
using nucoris.queries.PatientStateView;
using System.Threading;
using System.Threading.Tasks;

namespace nucoris.application.eventHandlers
{
    /// <summary>
    /// This class handles events that affect patient status change (new patient, patient admitted or patient discharged),
    /// with the goal to update the query repository that feeds the patient worklist
    /// </summary>
    public class PatientStatusChangeEventHandler :  MediatR.INotificationHandler<PatientCreatedEvent>
                                                 ,  MediatR.INotificationHandler<AdmissionStartedEvent>
                                                 ,  MediatR.INotificationHandler<AdmissionEndedEvent>
    {
        private readonly IMaterializedViewRepository<PatientStateViewItem, PatientStateViewSpecification> _queryRepository;
        private domain.Patient _patientJustCreated;

        public PatientStatusChangeEventHandler(
            IMaterializedViewRepository<PatientStateViewItem, PatientStateViewSpecification> queryRepository)
        {
            _queryRepository = queryRepository;
            _patientJustCreated = null;
        }

        public async Task Handle(PatientCreatedEvent notification, CancellationToken cancellationToken)
        {
            StoreNewPatientStateViewItem(notification.Patient);
        }

        public async Task Handle(AdmissionStartedEvent notification, CancellationToken cancellationToken)
        {
            await UpdateAdmissionOfExistingPatientStateViewItem(notification);
        }

        public async Task Handle(AdmissionEndedEvent notification, CancellationToken cancellationToken)
        {
            await UpdateAdmissionOfExistingPatientStateViewItem(notification);
        }

        private void StoreNewPatientStateViewItem(domain.Patient patient)
        {
            var queryItem = PatientStateViewItemFactory.FromDomain(patient);
            _queryRepository.Store(queryItem);
            _patientJustCreated = patient;
        }

        private async Task UpdateAdmissionOfExistingPatientStateViewItem(AdmissionEvent notification)
        {
            var queryItem = await _queryRepository.GetAsync(notification.PatientIdentity.Id);

            // We expect that admission events will take place on patients already created and therefore existing in the database.
            // However, in case we decide in the future to handle multiple commands in the same session,
            //  we provide a workaround in case the patient has not yet been persisted.
            if( queryItem == null && _patientJustCreated != null && _patientJustCreated.PatientIdentity.Id == notification.PatientIdentity.Id)
            {
                queryItem = PatientStateViewItemFactory.FromDomain(_patientJustCreated);
            }

            Guard.Against.Null(queryItem, $"No {nameof(PatientStateViewItem)} found for patient {notification.PatientIdentity.Id.ToString()}");

            queryItem = PatientStateViewItemFactory.FromExisting(queryItem, notification.Admission);
            _queryRepository.Store(queryItem);
        }
    }
}

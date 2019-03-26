using System;
using nucoris.queries.ActiveOrdersView;

namespace nucoris.webapp.Models
{
    public class OrderVM
    {
        public OrderVM() {}

        public OrderVM(ActiveOrdersViewItem queryItem)
        {
            if (queryItem != null)
            {
                var order = queryItem.QueryOrder;
                this.Id = order.Id;
                this.Description = order.Description;
                this.AssignedUserId = order.AssignedUserId;
                this.IsActive = !(order.OrderState == OrderState.Inactive);
                this.AdmissionId = order.AdmissionId;

                this.PatientId = order.PatientId;
                this.Mrn = order.Mrn;
                this.GivenName = order.GivenName;
                this.FamilyName = order.FamilyName;
                this.PatientDisplayName = domain.NameUtilities.BuildDisplayName(this.GivenName, this.FamilyName);
            }
        }

        public OrderVM(domain.Order order)
        {
            if( order != null)
            {
                this.Id = order.Id;
                this.Description = order.Description;
                this.AssignedUserId = order.AssignedTo?.Id;
                this.AssignedUserDisplayName = order.AssignedTo?.DisplayName;
                this.IsActive = domain.OrderStateExtensions.IsActive(order.State);
                this.AdmissionId = order.AdmissionId;

                this.PatientId = order.PatientIdentity.Id;
                this.Mrn = order.PatientIdentity.Mrn;
            }
        }

        public OrderVM(queries.PatientDetails.Order order, 
            queries.PatientDetails.Admission admission,
            queries.PatientDetails.Patient patient)
        {
            this.Id = order.Id;
            this.Description = order.Description;
            this.AssignedUserDisplayName = order.AssignedUser;
            this.IsActive = order.IsActive;
            this.AdmissionId = admission.Id;

            this.PatientId = patient.Id;
            this.Mrn = patient.Mrn;
            this.GivenName = patient.GivenName;
            this.FamilyName = patient.FamilyName;
            this.PatientDisplayName = patient.DisplayName;
        }

        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid? AssignedUserId { get; set; }
        public string AssignedUserDisplayName { get; set; }
        public bool IsActive { get; set; }
        public Guid PatientId { get; set; }
        public string Mrn { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
        public string PatientDisplayName { get; set; }
        public Guid AdmissionId { get; set; }
    }
}

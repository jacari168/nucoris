using System;

namespace nucoris.queries.ActiveOrdersView
{
    // We have a slightly different set of states to show they can be adapted to application query needs,
    //  but these can change as the final application is being developed.
    public enum OrderState
    {
        Created,
        Ready,
        InProgress,
        Inactive
    }

    public class Order
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public Guid? AssignedUserId { get; set; }
        public OrderState OrderState { get; set; }

        public Guid AdmissionId { get; set; }
        public Guid PatientId { get; set; }
        public string Mrn { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
    }
}
using nucoris.application.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nucoris.queries.ActiveOrdersView
{
    public class ActiveOrdersViewSpecification : IMaterializedViewSpecification<ActiveOrdersViewItem>
    {
        public Guid? PatientId { get; }
        public Guid? AssignedUserId { get; }
        public List<OrderState> OrderStates { get; }

        public static ActiveOrdersViewSpecification ForUser(Guid? assignedUserId)
        {
            return new ActiveOrdersViewSpecification(patientId: null, assignedUserId: assignedUserId, orderStates: null);
        }

        public ActiveOrdersViewSpecification(
            Guid? patientId, Guid? assignedUserId, IEnumerable<OrderState> orderStates)
        {
            this.PatientId = patientId;
            this.AssignedUserId = assignedUserId;
            this.OrderStates = orderStates?.ToList();
        }

        public IEnumerable<DbDocumentCondition<ActiveOrdersViewItem>> AsLinqExpressions()
        {
            var conditions = new List<DbDocumentCondition<ActiveOrdersViewItem>>();

            if (this.PatientId != null)
            {
                conditions.Add(new DbDocumentCondition<ActiveOrdersViewItem>(doc =>
                            doc.docContents.QueryOrder.PatientId == this.PatientId.Value));
            }

            if ( this.AssignedUserId != null)
            {
                conditions.Add(new DbDocumentCondition<ActiveOrdersViewItem>(doc => 
                            doc.docContents.QueryOrder.AssignedUserId == this.AssignedUserId.Value));
            }

            if( this.OrderStates != null && this.OrderStates.Count > 0)
            {
                conditions.Add(new DbDocumentCondition<ActiveOrdersViewItem>(doc =>
                           this.OrderStates.Contains(doc.docContents.QueryOrder.OrderState)));
            }

            return conditions;
        }
    }
}
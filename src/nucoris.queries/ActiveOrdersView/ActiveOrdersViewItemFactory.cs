namespace nucoris.queries.ActiveOrdersView
{
    public static class ActiveOrdersViewItemFactory
    {
        public static ActiveOrdersViewItem FromDomain(domain.Patient patient, domain.Order order)
        {
            return new ActiveOrdersViewItem()
            {
                Id = order.Id,
                QueryOrder = new Order()
                {
                    AdmissionId = order.AdmissionId,
                    AssignedUserId = order.AssignedTo?.Id,
                    Description = order.Description,
                    FamilyName = patient.FamilyName,
                    GivenName = patient.GivenName,
                    Id = order.Id,
                    Mrn = patient.PatientIdentity.Mrn,
                    OrderState = TranslateToQuery(order.State),
                    PatientId = patient.PatientIdentity.Id
                }
            };
        }


        public static ActiveOrdersViewItem FromExisting(ActiveOrdersViewItem item, domain.Order order)
        {
            // We update only fields that might change
            item.QueryOrder.AssignedUserId = order.AssignedTo?.Id;
            item.QueryOrder.Description = order.Description;
            item.QueryOrder.OrderState = TranslateToQuery(order.State);

            return item;
        }

        private static OrderState TranslateToQuery(domain.OrderState domainState)
        {
            OrderState state;

            switch (domainState)
            {
                case domain.OrderState.Created:
                    state = OrderState.Created;
                    break;
                case domain.OrderState.Ready:
                    state = OrderState.Ready;
                    break;
                case domain.OrderState.InProgress:
                    state = OrderState.InProgress;
                    break;
                case domain.OrderState.Completed:
                case domain.OrderState.Cancelled:
                    state = OrderState.Inactive;
                    break;
                default:
                    state = OrderState.Created;
                    break;
            }

            return state;
        }
    }
}

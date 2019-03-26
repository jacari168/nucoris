using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using nucoris.application.interfaces;
using nucoris.domain;

namespace nucoris.persistence
{
    /// <summary>
    /// Repository you can use to work with classes derived from Order for actions that apply directly to Order base class,
    ///     such as assign, cancel, etc.
    /// </summary>
    public class OrderRepository : PatientDescendentRepository<Order>, 
        application.interfaces.repositories.IOrderRepository
    {
        private string _orderDocType;

        public OrderRepository(DbSession dbSession, IDbSessionConfiguration dbSessionConfiguration) : base(dbSession)
        {
            if( dbSessionConfiguration == null) throw new ArgumentNullException(nameof(dbSessionConfiguration));            
            _orderDocType = dbSessionConfiguration.GetDBDocType<Order>();
        }

        public override async Task<Order> GetAsync(Guid patientId, Guid itemId)
        {
            // Order is an abstract class.
            // We can't use the base class GetAsync<Order> because the default deserialization would fail.
            // We have instead to use the generic LoadManyAsync that reads documents as dynamic
            //  and "manually" deserializes each of them to the right type by inspecting its docSubType
            //  which contains the type's full name.
            var orders = await _dbSession.LoadManyDerivedAsync(patientId,
                new List<DbDocumentCondition<Order>>()
                {
                    new DbDocumentCondition<Order>((doc) => doc.id == itemId.ToString())
                });

            if( orders.Any())
            {
                return orders.First();
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Order>> GetOfAdmissionAsync(Guid patientId, Guid admissionId,
            IEnumerable<OrderState> withTheseStates)
        {
            var conditions = new List<DbDocumentCondition<Order>>();

            // Filter by docSubType for accuracy
            conditions.Add(new DbDocumentCondition<Order>((entity) => entity.docType == _orderDocType
                        && entity.docContents.AdmissionId == admissionId));

            if (withTheseStates != null && withTheseStates.Any())
            {
                // States are persisted as text:
                var statesAsText = withTheseStates.Select(i => i.ToString()).ToList();
                conditions.Add(new DbDocumentCondition<Order>((entity) => statesAsText.Contains(entity.docContents.State.ToString())));
            }

            return await _dbSession.LoadManyDerivedAsync(patientId, conditions);
        }

    }
}

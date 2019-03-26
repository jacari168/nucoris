using nucoris.domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nucoris.application.interfaces.repositories
{
    /// <summary>
    /// Interface to work with orders at the base class Order, when you don't need or don't know 
    /// the derived type.
    /// </summary>
    public interface IOrderRepository : IPatientDescendentRepository<Order>
    {
        Task<List<Order>> GetOfAdmissionAsync(Guid patientId, Guid admissionId, IEnumerable<OrderState> withTheseStates);
    }
}

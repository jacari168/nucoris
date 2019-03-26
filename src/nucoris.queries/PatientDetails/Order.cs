using System;
using System.Collections.Generic;

namespace nucoris.queries.PatientDetails
{
    public class Order
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string AssignedUser { get; set; }
        public bool IsActive { get; set; }

        public List<MedicationAdministration> Administrations { get; set; }
    }
}
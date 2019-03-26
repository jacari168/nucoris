using System;
using System.Collections.Generic;

namespace nucoris.queries.PatientDetails
{
    public class AdmissionDetails : Admission
    {
        public List<Order> Orders { get; set; }
    }
}
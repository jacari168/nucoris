using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nucoris.webapp.Models
{
    public class AdmissionVM
    {
        public Guid Id { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }
        public List<OrderVM> Orders { get; set; }
    }
}

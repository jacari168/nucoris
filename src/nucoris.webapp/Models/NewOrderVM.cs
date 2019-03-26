using System;
using System.ComponentModel.DataAnnotations;

namespace nucoris.webapp.Models
{
    public class NewOrderVM
    {
        public Guid PatientId { get; set; }
        public Guid AdmissionId { get; set; }

        [Required]
        [Display(Name = "Medication")]
        public Guid MedicationId { get; set; }

        [Required]
        [Display(Name = "Administration Frequency")]
        public Guid FrequencyId { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }
    }
}

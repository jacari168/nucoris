using System;
using System.ComponentModel.DataAnnotations;

namespace nucoris.webapp.Models
{
    public class NewPatientVM
    {
        [Required]
        [MinLength(8)]
        public string Mrn { get; set; }

        [Required]
        [Display(Name ="Given Name")]
        public string GivenName { get; set; }

        [Required]
        [Display(Name = "Family Name")]
        public string FamilyName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
    }
}

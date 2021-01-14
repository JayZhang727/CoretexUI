using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoretexUI.Models
{
    public class Vehicle : IValidatableObject
    {
        private readonly int currentYear = DateTime.Now.Year;
        [Key]
        public int Id { get; set; }
        [Required]
        public string Make { get; set; }
        [Required]
        public string Model { get; set; }
        [Required]
        public int Year { get; set; }
        [Required]
        public string VIN { get; set; }

        public Vehicle()
        {
            Year = 1950;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if(Year <= 1950)
            {
                var msg = "Year has to be greater than or equal to 1950";
                yield return new ValidationResult(msg, new[] { nameof(Year) });
            }

            if (Year > currentYear)
            {
                var msg = "Year can not be greater than the current year (" + currentYear.ToString() + ")";
                yield return new ValidationResult(msg, new[] { nameof(Year) });
            }
        }
    }
}

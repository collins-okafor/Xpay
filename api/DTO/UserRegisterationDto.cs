using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTO
{
    public class UserRegisterationDto
    {
        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [DisplayFormat(DataFormatString = "{DD/MM/YYYY}")]
        [DefaultValue("DD/MM/YYYY")]
        public string? DOB { get; set; }

        public string? PhoneNumber { get; set; }
        
        public string? Email { get; set; }

        public string? Address { get; set; }

        public string? BVN { get; set; }

        public string? NIN { get; set; }
    }
}
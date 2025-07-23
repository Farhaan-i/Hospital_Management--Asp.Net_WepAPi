using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Dto
{
    public class DoctorDto
    {
        public int DoctorId { get; set; }

        [Required]
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string? DoctorEmail { get; set; }

        public string? DoctorContactNumber { get; set; }
    }
}

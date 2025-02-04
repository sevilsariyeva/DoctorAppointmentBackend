using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Models
{
    public class User
    {
        public String Id { get; set; }
        [Required]
        public String Name { get; set; }

        [Required]
        public String Email { get; set; }

        [Required]
        public String Password { get; set; }
        public String ImageUrl { get; set; }
        public Object Address { get; set; }
        public String Gender { get; set; }
        public DateTime Dob { get; set; }
        public String Phone { get; set; }
    }
}

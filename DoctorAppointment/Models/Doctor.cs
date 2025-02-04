using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Models
{
    public class Doctor
    {
        public String Id { get; set; }
        [Required]
        public String Name { get; set; }

        [Required]
        public String Email { get; set; }

        [Required]
        public String Password { get; set; }
        [Required]
        public String ImageUrl { get; set; }
        [Required]
        public String Speciality { get; set; }
        [Required]
        public String Degree { get; set; }
        [Required]
        public String Experience { get; set; }
        [Required]
        public String About { get; set; }
        [Required]
        public Boolean Available { get; set; }
        [Required]
        public Double Fees { get; set; }
        [Required]
        public Object Address { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public Object SlotsBooked { get; set; }
    }
}

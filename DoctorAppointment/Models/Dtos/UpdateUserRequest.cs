using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Models.Dtos
{
    public class UpdateUserRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public IFormFile? Image { get; set; }
        public Address? Address { get; set; }
        public string? Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string? Phone { get; set; }
    }

}

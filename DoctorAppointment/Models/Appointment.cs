using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Models
{
    public class Appointment
    {
        [Required]
        public string? UserId { get; set; }
        [Required]
        public string? DocId { get; set; }
        [Required]
        public string? SlotDate { get; set; }
        [Required]
        public string? SlotTime { get; set; }
        [Required]
        public User? UserData { get; set; }
        [Required]
        public Doctor? DocData { get; set; }
        [Required]
        public decimal? Amount { get; set; }
        [Required]
        public DateTime? Date { get; set; }
        [Required]
        public bool? Cancelled { get; set; }
        [Required]
        public decimal? Payment { get; set; }
        [Required]
        public bool? IsCompleted { get; set; }
    }
}

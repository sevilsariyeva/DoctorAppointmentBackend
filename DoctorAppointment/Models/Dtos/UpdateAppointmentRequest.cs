namespace DoctorAppointment.Models.Dtos
{
    public class UpdateAppointmentRequest
    {
        public string? AppointmentId { get; set; }
        public decimal? Amount { get; set; }
        public bool? Cancelled { get; set; }
        public bool? IsCompleted { get; set; }
        public decimal? Payment { get; set; }
    }
}

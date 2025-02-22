namespace DoctorAppointment.Models.Dtos
{
    public class BookAppointmentRequest
    {
        public string UserId { get; set; }
        public string DocId { get; set; }
        public string SlotDate { get; set; }
        public string SlotTime { get; set; }
        public double? Amount { get; set; }
    }
}

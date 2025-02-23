namespace DoctorAppointment.Models.Dtos
{
    public class GetUserAppointmentsResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<Appointment> Appointments { get; set; }
    }

}

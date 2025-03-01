namespace DoctorAppointment.Models.Dtos
{
    public class DoctorDashboardDto
    {
        public long? Appointments { get; set; }
        public decimal Earnings { get; set; }
        public long Patients { get; set; }
        public List<Appointment> LatestAppointments { get; set; }
    }
}

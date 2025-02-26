namespace DoctorAppointment.Models.Dtos
{
    public class AdminDashboardDto
    {
        public long Doctors { get; set; }
        public long Appointments { get; set; }
        public long Patients { get; set; }
        public List<Appointment> LatestAppointments { get; set; }
    }
}

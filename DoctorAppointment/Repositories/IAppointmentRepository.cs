using DoctorAppointment.Models;

namespace DoctorAppointment.Repositories
{
    public interface IAppointmentRepository
    {
        Task AddAppointmentAsync(Appointment appointment);
        Task<List<Appointment>> GetUserAppointmentsAsync(string userId);
        Task<Appointment> GetAppointmentByUserIdAsync(string appointmentId);
        Task<bool> CancelAppointmentAsync(string userId, string appointmentId);
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<bool> UpdateAppointmentAsync(Appointment appointment);
        Task<Appointment?> GetAppointmentByIdAsync(string appointmentId);
        Task<Doctor?> GetDoctorByAppointmentIdAsync(string appointmentId);
    }
}

using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;

namespace DoctorAppointment.Services
{
    public interface IAppointmentService
    {
        Task<Appointment> BookAppointmentAsync(string userId, string docId, string slotDate, string slotTime);
        Task<List<Appointment>> GetUserAppointmentsAsync(string userId);
        Task<bool> CancelAppointmentAsync(CancelAppointmentRequest request, bool isAdmin);
        Task<List<Appointment>> GetUserAppointmentsAsync();
        Task<string?> GetUserIdByAppointmentIdAsync(string appointmentId);
        Task<Appointment> UpdateAppointmentAsync(UpdateAppointmentRequest request);
        Task<Appointment?> GetAppointmentByIdAsync(string appointmentId);
        Task<Doctor?> GetDoctorByAppointmentIdAsync(string appointmentId);
    }
}

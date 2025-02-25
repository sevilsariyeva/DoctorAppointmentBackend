using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;

namespace DoctorAppointment.Services
{
    public interface IAppointmentService
    {
        Task<BookAppointmentResponse> BookAppointmentAsync(string userId, string docId, string slotDate, string slotTime);
        Task<GetUserAppointmentsResponse> GetUserAppointmentsAsync(string userId);
        Task<CancelAppointmentResponse> CancelAppointmentAsync(string userId, string appointmentId);
        Task<ServiceResponse<List<Appointment>>> GetUserAppointmentsAsync();
        Task<string?> GetUserIdByAppointmentIdAsync(string appointmentId);
        Task<bool> CancelAppointmentAdminAsync(string userId, string appointmentId);
        Task<bool> UpdateAppointmentAsync(Appointment appointment);
        Task<Appointment?> GetAppointmentByIdAsync(string appointmentId);
        Task<Doctor?> GetDoctorByAppointmentIdAsync(string appointmentId);
    }
}

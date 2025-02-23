using DoctorAppointment.Models;

namespace DoctorAppointment.Repositories
{
    public interface IUserRepository
    {
        Task<bool> AddUserAsync(User user);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserAsync(User user);
        Task AddAppointmentAsync(Appointment appointment);
        Task<List<Appointment>> GetUserAppointmentsAsync(string userId);
        Task<Appointment> GetAppointmentByIdAsync(string appointmentId);
        Task<bool> CancelAppointmentAsync(string userId, string appointmentId);
    }
}

using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;
using MongoDB.Driver;

namespace DoctorAppointment.Repositories
{
    public interface IAppointmentRepository
    {
        Task AddAppointmentAsync(Appointment appointment);
        Task<List<Appointment>> GetUserAppointmentsAsync(string userId);
        Task<Appointment> GetAppointmentByUserIdAsync(string appointmentId);
        Task<bool> CancelAppointmentAsync(string appointmentId, string userId, bool isAdmin, IClientSessionHandle session);
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<bool> UpdateAppointmentAsync(Appointment appointment);
        Task<Appointment?> GetAppointmentByIdAsync(string appointmentId);
        Task<Doctor?> GetDoctorByAppointmentIdAsync(string appointmentId);
        Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(string doctorId);
        Task<decimal> GetDoctorTotalEarningsAsync(string doctorId);
        Task<long> GetDoctorAppointmentsCountAsync(string doctorId);
        Task<List<Appointment>> GetLatestAppointmentsByDoctorIdAsync(string doctorId, int count);
        Task<long> GetDoctorPatientsCountAsync(string doctorId);
    }
}

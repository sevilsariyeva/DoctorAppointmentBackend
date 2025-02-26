using DoctorAppointment.Models;

namespace DoctorAppointment.Repositories
{
    public interface IAdminRepository
    {
        Task<Admin> GetAdminByEmailAsync(string email);
        Task<Doctor> AddDoctorAsync(Doctor doctor);
        Task<Doctor> GetDoctorByEmailAsync(string email);
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<long> GetDoctorsCountAsync();
        Task<long> GetAppointmentsCountAsync();
        Task<long> GetPatientsCountAsync();
        Task<List<Appointment>> GetLatestAppointmentsAsync(int count);
    }
}

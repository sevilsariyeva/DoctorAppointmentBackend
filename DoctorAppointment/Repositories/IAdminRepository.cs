using DoctorAppointment.Models;

namespace DoctorAppointment.Repositories
{
    public interface IAdminRepository
    {
        Task<Admin> GetAdminByEmailAsync(string email);
        Task<Doctor> GetDoctorByEmailAsync(string email);
        Task<List<Appointment>> GetAllAppointmentsAsync();
        Task<Doctor> AddDoctorAsync(Doctor doctor);
    }
}

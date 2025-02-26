using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;

namespace DoctorAppointment.Services
{
    public interface IAdminService
    {
        Task<string> LoginAdmin(string email, string password);
        Task<Doctor> AddDoctorAsync(DoctorDto doctorDto, IFormFile image);
        Task<int> GetDoctorsCountAsync();
        Task<int> GetAppointmentsCountAsync();
        Task<int> GetPatientsCountAsync();
        Task<List<Appointment>> GetLatestAppointmentsAsync(int count);
    }
}

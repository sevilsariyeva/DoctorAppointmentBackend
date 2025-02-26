using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;

namespace DoctorAppointment.Repositories
{
    public interface IDoctorRepository
    {
        Task<List<DoctorDto>> GetAllDoctorsAsync();
        Task<Doctor> GetDoctorByIdAsync(string doctorId);
        Task<Doctor> GetDoctorByEmailAsync(string email);
        Task UpdateDoctorAsync(Doctor doctor);
    }
}

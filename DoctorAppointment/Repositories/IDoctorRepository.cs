using DoctorAppointment.Models;
using DoctorAppointment.Models.Dtos;

namespace DoctorAppointment.Repositories
{
    public interface IDoctorRepository
    {
        Task<Doctor> AddDoctorAsync(Doctor doctor);
        Task<List<DoctorDto>> GetAllDoctorsAsync();
        Task<Doctor> GetDoctorByIdAsync(string doctorId);
        Task UpdateDoctorAsync(Doctor doctor);
    }
}

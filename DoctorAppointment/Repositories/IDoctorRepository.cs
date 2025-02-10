using DoctorAppointment.Models;

namespace DoctorAppointment.Repositories
{
    public interface IDoctorRepository
    {
        Task<Doctor> AddDoctorAsync(Doctor doctor);
    }
}

using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Services
{
    public interface IDoctorService
    {
        Task<List<DoctorDto>> GetAllDoctorsAsync();
        Task<bool> ChangeAvailabilityAsync(string doctorId);
        Task<string> LoginDoctor(string email, string password);
    }
}

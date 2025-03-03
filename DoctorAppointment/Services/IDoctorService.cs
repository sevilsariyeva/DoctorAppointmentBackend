using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;

namespace DoctorAppointment.Services
{
    public interface IDoctorService
    {
        Task<List<DoctorDto>> GetAllDoctorsAsync();
        Task<bool> ChangeAvailabilityAsync(string doctorId);
        Task<JwtTokenResponse> LoginDoctor(LoginRequest request);
        Task<DoctorDto> GetDoctorProfileAsync(string doctorId);
        Task<Doctor> GetDoctorByIdAsync(string doctorId);
        Task<DoctorDto> UpdateDoctorAsync(string doctorId, DoctorDto request);
        Task<DoctorDashboardDto> GetDoctorDashboardStatisticsAsync(string doctorId, int latestAppointmentsCount);
    }
}

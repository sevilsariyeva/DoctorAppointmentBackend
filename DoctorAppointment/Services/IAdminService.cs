using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace DoctorAppointment.Services
{
    public interface IAdminService
    {
        Task<JwtTokenResponse> LoginAdmin(LoginRequest request);
        Task<Doctor> AddDoctorAsync(DoctorDto doctorDto, IFormFile image);
        Task<AdminDashboardDto> GetAdminDashboardStatisticsAsync(int latestAppointmentsCount);
    }
}

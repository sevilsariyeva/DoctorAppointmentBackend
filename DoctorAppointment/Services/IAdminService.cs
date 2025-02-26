using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace DoctorAppointment.Services
{
    public interface IAdminService
    {
        Task<string> LoginAdmin(LoginRequest request);
        Task<Doctor> AddDoctorAsync(DoctorDto doctorDto, IFormFile image);
        //Task<int> GetDoctorsCountAsync();
        //Task<int> GetAppointmentsCountAsync();
        //Task<int> GetPatientsCountAsync();
        Task<AdminDashboardDto> GetAdminDashboardStatisticsAsync(int latestAppointmentsCount);
    }
}

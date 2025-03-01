using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using DoctorAppointment.Repositories;
using DoctorAppointment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using DoctorAppointment.Utilities;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IPasswordHasher<Doctor> _passwordHasher;
    private readonly IConfiguration _configuration;

    public DoctorService(IDoctorRepository doctorRepository, IWebHostEnvironment environment, IPasswordHasher<Doctor> passwordHasher, IConfiguration configuration, IAppointmentRepository appointmentRepository)
    {
        _doctorRepository = doctorRepository;
        _environment = environment;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _appointmentRepository = appointmentRepository;
    }


    public async Task<bool> ChangeAvailabilityAsync(string doctorId)
    {
        if (string.IsNullOrEmpty(doctorId))
        {
            return false;
        }

        var doctor = await _doctorRepository.GetDoctorByIdAsync(doctorId);
        if (doctor == null)
        {
            return false;
        }

        doctor.Available = !doctor.Available;
        await _doctorRepository.UpdateDoctorAsync(doctor);

        return true;
    }

    public async Task<JwtTokenResponse> LoginDoctor(LoginRequest request)
    {
        var doctor = await _doctorRepository.GetDoctorByEmailAsync(request.Email);
        var verificationResult = _passwordHasher.VerifyHashedPassword(doctor, doctor.Password, request.Password);
        if (doctor==null || verificationResult == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialException("Invalid email or password.");
        }

        return JwtTokenGenerator.GenerateToken(doctor.Id, doctor.Email, "Doctor", _configuration);
    }

    public async Task<List<DoctorDto>> GetAllDoctorsAsync()
    {
        var doctors = await _doctorRepository.GetAllDoctorsAsync();

        return doctors.Select(d=>new DoctorDto
        {
            Id = d.Id,
            Name=d.Name,
            Email = d.Email,
            Password = d.Password,
            Speciality = d.Speciality,
            Image = d.Image,
            Degree = d.Degree,
            Experience = d.Experience,
            Fees = d.Fees,
            About = d.About,
            Available=d.Available,
            Address1=d.Address1,
            Address2=d.Address2,
        }).ToList();
    }

    public async Task<DoctorDashboardDto> GetDoctorDashboardStatisticsAsync(string doctorId, int latestAppointmentsCount)
    {
        return new DoctorDashboardDto
        {
            Appointments = await _appointmentRepository.GetDoctorAppointmentsCountAsync(doctorId),
            Earnings = await _appointmentRepository.GetDoctorTotalEarningsAsync(doctorId),
            LatestAppointments = await _appointmentRepository.GetLatestAppointmentsByDoctorIdAsync(doctorId, latestAppointmentsCount),
            Patients = await _appointmentRepository.GetDoctorPatientsCountAsync(doctorId)
        };
    }


}

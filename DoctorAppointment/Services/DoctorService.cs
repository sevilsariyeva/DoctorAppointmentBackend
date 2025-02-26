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

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IPasswordHasher<Doctor> _passwordHasher;
    private readonly IConfiguration _configuration;

    public DoctorService(IDoctorRepository doctorRepository, IWebHostEnvironment environment, IPasswordHasher<Doctor> passwordHasher, IConfiguration configuration)
    {
        _doctorRepository = doctorRepository;
        _environment = environment;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
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

    public async Task<string> LoginDoctor(string email, string password)
    {
        var doctor = await _doctorRepository.GetDoctorByEmailAsync(email);
        var verificationResult = _passwordHasher.VerifyHashedPassword(doctor, doctor.Password, password);
        if (doctor==null || verificationResult == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialException("Invalid email or password.");
        }

        var token = GenerateJwtToken(doctor);
        return token;
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
    private string GenerateJwtToken(Doctor doctor)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = _configuration["Jwt:SecretKey"];
        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes");

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured.");
        }

        var key = Encoding.UTF8.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                    new Claim(ClaimTypes.NameIdentifier, doctor.Id),
                    new Claim(ClaimTypes.Email, doctor.Email),
                    new Claim(ClaimTypes.Role, "Doctor")
            }),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

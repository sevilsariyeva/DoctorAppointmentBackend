using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using DoctorAppointment.Repositories;
using DoctorAppointment.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

public class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly IPasswordHasher<Doctor> _passwordHasher;

    public DoctorService(IDoctorRepository doctorRepository, IWebHostEnvironment environment, IPasswordHasher<Doctor> passwordHasher)
    {
        _doctorRepository = doctorRepository;
        _environment = environment;
        _passwordHasher = passwordHasher;
    }


    public async Task<Doctor> AddDoctorAsync(DoctorDto doctorDto, IFormFile image)
    {
        string fileName = null;

        if (image != null && image.Length > 0)
        {
            var extension = Path.GetExtension(image.FileName).ToLower();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
                throw new ArgumentException("Şəkil formatı etibarlı deyil. Yalnız .jpg, .jpeg və .png formatlarını dəstəkləyirik.");

            fileName = Guid.NewGuid().ToString() + extension;

            var uploadsDirectory = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsDirectory))
            {
                Directory.CreateDirectory(uploadsDirectory);
            }

            var filePath = Path.Combine(uploadsDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
        }

        var doctor = new Doctor
        {
            Name = doctorDto.Name,
            Email = doctorDto.Email,
            Password = doctorDto.Password,
            Speciality = doctorDto.Speciality,
            Image = fileName != null ? "/uploads/" + fileName : null,
            Degree = doctorDto.Degree,
            Experience = doctorDto.Experience,
            Fees = doctorDto.Fees,
            About = doctorDto.About,
            Address = new Address
            {
                Line1 = doctorDto.Address1,
                Line2 = doctorDto.Address2
            }
        };
        var hashedPassword = _passwordHasher.HashPassword(doctor, doctorDto.Password);
        doctor.Password = hashedPassword;
        await _doctorRepository.AddDoctorAsync(doctor);

        return doctor;
    }

    public async Task<List<DoctorDto>> GetAllDoctorsAsync()
    {
        var doctors = await _doctorRepository.GetAllDoctorsAsync();

        return doctors.Select(d=>new DoctorDto
        {
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
}

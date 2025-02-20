﻿using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Services
{
    public interface IDoctorService
    {
        Task<Doctor> AddDoctorAsync(DoctorDto doctorDto, IFormFile image);
        Task<List<DoctorDto>> GetAllDoctorsAsync();
        Task<bool> ChangeAvailabilityAsync(string doctorId);
    }
}

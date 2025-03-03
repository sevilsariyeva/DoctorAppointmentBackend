using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using DoctorAppointment.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;
using DoctorAppointment.Exceptions;

namespace DoctorAppointment.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly MongoDbService _mongoDbService;
        private readonly MongoClient _mongoClient;
        public AppointmentService(IDoctorRepository doctorRepository, IUserRepository userRepository, IAdminRepository adminRepository,IAppointmentRepository appointmentRepository,MongoDbService mongoDbService,MongoClient mongoClient)
        {
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _mongoClient = mongoClient;
            _mongoDbService = mongoDbService;
        }

        //{----------User----------}
        public async Task<Appointment> BookAppointmentAsync(string userId, string docId, string slotDate, string slotTime)
        {
            var doctor = await _doctorRepository.GetDoctorByIdAsync(docId);
            if (doctor == null)
            {
                throw new NotFoundException("Doctor not found.");
            }

            if (!doctor.Available)
            {
                throw new DoctorUnavailableException("Doctor is not available.");
            }

            if (doctor.SlotsBooked == null)
            {
                doctor.SlotsBooked = new Dictionary<string, List<string>>();
            }

            if (doctor.SlotsBooked.ContainsKey(slotDate) && doctor.SlotsBooked[slotDate].Contains(slotTime))
            {
                throw new DoctorUnavailableException("Slot is already booked.");
            }

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            if (!doctor.SlotsBooked.ContainsKey(slotDate))
            {
                doctor.SlotsBooked[slotDate] = new List<string>();
            }

            doctor.SlotsBooked[slotDate].Add(slotTime);

            var appointment = new Appointment
            {
                UserId = userId,
                DocId = docId,
                UserData = user,
                DocData = doctor,
                Amount = doctor.Fees,
                SlotTime = slotTime,
                SlotDate = slotDate,
                Date = DateTime.UtcNow
            };

            await _userRepository.AddAppointmentAsync(appointment);
            await _doctorRepository.UpdateDoctorAsync(doctor);

            return appointment;
        }


        public async Task<List<Appointment>> GetUserAppointmentsAsync(string userId)
        {
            var appointments = await _userRepository.GetUserAppointmentsAsync(userId);

            if (appointments == null || !appointments.Any())
            {
                throw new KeyNotFoundException("No appointments found.");
            }

            return appointments;
        }

        public async Task<List<Appointment>> GetUserAppointmentsAsync()
        {
            var appointments = await _adminRepository.GetAllAppointmentsAsync();
            return appointments ?? new List<Appointment>();
        }

        public async Task<string?> GetUserIdByAppointmentIdAsync(string appointmentId)
        {
            return (await _userRepository.GetAppointmentByIdAsync(appointmentId))?.UserId;
        }

        public async Task<bool> CancelAppointmentAsync(CancelAppointmentRequest request, bool isAdmin = false)
        {
            var appointment = await _userRepository.GetAppointmentByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                throw new NotFoundException("Appointment Not Found");
            }

            if (!isAdmin && appointment.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Unauthorized.");
            }

            var doctor = await _doctorRepository.GetDoctorByIdAsync(appointment.DocId);

            if (doctor == null)
            {
                throw new NotFoundException("Doctor Not Found");
            }

            if (doctor.SlotsBooked.TryGetValue(appointment.SlotDate, out var slotTimes))
            {
                slotTimes.Remove(appointment.SlotTime);
            }

            if (isAdmin && appointment.Payment != 0)
            {
                appointment.Payment -= doctor.Fees;
            }

            return await HandleTransactionAsync(request, isAdmin, doctor, appointment);
        }

        private async Task<bool> HandleTransactionAsync(CancelAppointmentRequest request, bool isAdmin, Doctor doctor, Appointment appointment)
        {
            using var session = await _mongoDbService.StartSessionAsync();
            session.StartTransaction();
            try
            {
                var result = await _appointmentRepository.CancelAppointmentAsync(request.AppointmentId, request.UserId, isAdmin, session);
                if (!result)
                {
                    await session.AbortTransactionAsync();
                    return false;
                }

                await _doctorRepository.UpdateDoctorAsync(doctor);
                await _appointmentRepository.UpdateAppointmentAsync(appointment);

                await session.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await session.AbortTransactionAsync();
                throw;
            }
        }


        public async Task<Appointment> UpdateAppointmentAsync(UpdateAppointmentRequest request)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(request.AppointmentId);
            if (appointment == null)
            {
                throw new NotFoundException($"Appointment with ID {request.AppointmentId} not found.");
            }
            appointment.IsCompleted = request.IsCompleted;
            appointment.Cancelled = request.Cancelled;
            appointment.Payment = request.Payment;

            var updated = await _appointmentRepository.UpdateAppointmentAsync(appointment);

            if (!updated)
            {
                throw new DatabaseUpdateException($"Failed to update appointment {request.AppointmentId}.");
            }

            return appointment;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(string appointmentId)
        {
            return await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
        }
        public async Task<bool> UpdateAppointmentPaymentAsync(string appointmentId, decimal paymentAmount)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(appointmentId);
            if (appointment == null)
            {
                throw new NotFoundException($"Appointment with ID {appointmentId} not found.");
            }

            appointment.Amount = paymentAmount;
            var updated = await _appointmentRepository.UpdateAppointmentAsync(appointment);

            if (!updated)
            {
                throw new DatabaseUpdateException($"Failed to update payment for appointment {appointmentId}.");
            }

            return true;
        }
        public async Task<Doctor> GetDoctorByAppointmentIdAsync(string appointmentId)
        {
            var doctor = await _appointmentRepository.GetDoctorByAppointmentIdAsync(appointmentId);
            if (doctor == null)
            {
                throw new NotFoundException($"Doctor for appointment {appointmentId} not found.");
            }

            return doctor;
        }
        public async Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId))
            {
                return new List<Appointment>();
            }

            var appointments = await _appointmentRepository.GetAppointmentsByDoctorIdAsync(doctorId);

            return appointments.Select(a => new Appointment
            {
                Id = a.Id,
                UserId = a.UserId,
                DocId = a.DocId,
                SlotDate = a.SlotDate,
                SlotTime = a.SlotTime,
                UserData = a.UserData, 
                DocData = a.DocData,
                Amount = a.Amount,
                Date = a.Date,
                Cancelled = a.Cancelled,
                Payment = a.Payment,
                IsCompleted = a.IsCompleted
            }).ToList();
        }

    }
}

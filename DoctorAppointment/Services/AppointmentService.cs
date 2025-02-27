using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using DoctorAppointment.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using MongoDB.Driver;

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
        public async Task<BookAppointmentResponse> BookAppointmentAsync(string userId, string docId, string slotDate, string slotTime)
        {
            var doctor = await _doctorRepository.GetDoctorByIdAsync(docId);
            if (doctor == null || !doctor.Available || (doctor.SlotsBooked.ContainsKey(slotDate) && doctor.SlotsBooked[slotDate].Contains(slotTime)))
            {
                return new BookAppointmentResponse { Success = false, Message = "Doctor not available or slot is booked" };
            }

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new BookAppointmentResponse { Success = false, Message = "User not found." };
            }

            doctor.SlotsBooked.TryAdd(slotDate, new List<string>());
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

            return new BookAppointmentResponse { Success = true, Message = "Appointment booked successfully." };
        }

        public async Task<GetUserAppointmentsResponse> GetUserAppointmentsAsync(string userId)
        {
            var appointments = await _userRepository.GetUserAppointmentsAsync(userId);
            return appointments != null && appointments.Any()
                ? new GetUserAppointmentsResponse { Success = true, Appointments = appointments }
                : new GetUserAppointmentsResponse { Success = false, Message = "No appointments found." };
        }

        //public async Task<bool> CancelAppointmentAsync(CancelAppointmentRequest request)
        //{
        //    var appointment = await _userRepository.GetAppointmentByIdAsync(request.AppointmentId);
        //    if (appointment == null)
        //    {
        //        throw new Exception("Appointment Not Found");
        //    }

        //    if (appointment.UserId != request.UserId)
        //    {
        //        throw new UnauthorizedAccessException("Unauthorized.");
        //    }

        //    var doctor = await _doctorRepository.GetDoctorByIdAsync(appointment.DocId);
        //    doctor?.SlotsBooked[appointment.SlotDate]?.Remove(appointment.SlotTime);

        //    var result = await _userRepository.CancelAppointmentAsync(request.UserId, request.AppointmentId);
        //    if (!result)
        //    {
        //        throw new Exception("Failed to cancel the appointment.");
        //    }

        //    await _doctorRepository.UpdateDoctorAsync(doctor);
        //    return result;
        //}


        //{----------Admin----------}
        public async Task<ServiceResponse<List<Appointment>>> GetUserAppointmentsAsync()
        {
            var appointments = await _adminRepository.GetAllAppointmentsAsync();
            return appointments != null && appointments.Count > 0
                ? new ServiceResponse<List<Appointment>>(appointments, "Appointments retrieved successfully.", true)
                : new ServiceResponse<List<Appointment>>(null, "No appointments found.", false);
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
                throw new Exception("Appointment Not Found");
            }

            if (!isAdmin && appointment.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("Unauthorized.");
            }

            var doctor = await _doctorRepository.GetDoctorByIdAsync(appointment.DocId);
            doctor?.SlotsBooked[appointment.SlotDate]?.Remove(appointment.SlotTime);

            if (isAdmin && appointment.Payment != null && doctor != null)
            {
                appointment.Payment -= doctor.Fees;
            }

            if (isAdmin)
            {
                using (var session = await _mongoDbService.StartSessionAsync())
                {
                    session.StartTransaction();
                    try
                    {
                        var result = await _appointmentRepository.CancelAppointmentAsync(request.AppointmentId, request.UserId, true, session);
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
            }
            else
            {
                using var session = await _mongoClient.StartSessionAsync();
                var result = await _appointmentRepository.CancelAppointmentAsync(request.AppointmentId,request.UserId, false, session);

                if (!result)
                {
                    throw new Exception("Failed to cancel the appointment.");
                }

                await _doctorRepository.UpdateDoctorAsync(doctor);
                return result;
            }

        }


        public async Task<bool> UpdateAppointmentAsync(Appointment appointment)
        {
            return await _appointmentRepository.UpdateAppointmentAsync(appointment);
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
                return false;
            }

            appointment.Amount = paymentAmount; 
            return await _appointmentRepository.UpdateAppointmentAsync(appointment);
        }
        public async Task<Doctor?> GetDoctorByAppointmentIdAsync(string appointmentId)
        {
            return await _appointmentRepository.GetDoctorByAppointmentIdAsync(appointmentId);
        }

      
    }
}

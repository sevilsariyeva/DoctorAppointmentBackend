using DoctorAppointment.Models.Dtos;
using DoctorAppointment.Models;
using DoctorAppointment.Repositories;

namespace DoctorAppointment.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAdminRepository _adminRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IDoctorRepository doctorRepository, IUserRepository userRepository, IAdminRepository adminRepository,IAppointmentRepository appointmentRepository)
        {
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _adminRepository = adminRepository ?? throw new ArgumentNullException(nameof(adminRepository));
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
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

        public async Task<CancelAppointmentResponse> CancelAppointmentAsync(string userId, string appointmentId)
        {
            var appointment = await _userRepository.GetAppointmentByIdAsync(appointmentId);
            if (appointment == null || appointment.UserId != userId)
            {
                return new CancelAppointmentResponse { Success = false, Message = "Appointment not found or unauthorized." };
            }

            var doctor = await _doctorRepository.GetDoctorByIdAsync(appointment.DocId);
            doctor?.SlotsBooked[appointment.SlotDate]?.Remove(appointment.SlotTime);

            var result = await _userRepository.CancelAppointmentAsync(userId, appointmentId);
            if (!result)
            {
                return new CancelAppointmentResponse { Success = false, Message = "Failed to cancel the appointment." };
            }

            await _doctorRepository.UpdateDoctorAsync(doctor);
            return new CancelAppointmentResponse { Success = true, Message = "Appointment canceled successfully." };
        }

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

        public async Task<bool> CancelAppointmentAdminAsync(string userId, string appointmentId)
        {
            var appointment = await _userRepository.GetAppointmentByIdAsync(appointmentId);
            if (appointment == null || appointment.UserId != userId)
            {
                return false;
            }

            var doctor = await _doctorRepository.GetDoctorByIdAsync(appointment.DocId);
            doctor?.SlotsBooked[appointment.SlotDate]?.Remove(appointment.SlotTime);

            if (appointment != null && appointment.Payment != null && doctor != null)
            {
                appointment.Payment -= doctor.Fees;
            }
            var result = await _userRepository.CancelAppointmentAsync(userId, appointmentId);
            if (!result)
            {
                return false;
            }

            await _doctorRepository.UpdateDoctorAsync(doctor);
            await _appointmentRepository.UpdateAppointmentAsync(appointment);
            return true;
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

using DoctorAppointment.Models;
using DoctorAppointment.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace DoctorAppointment.Repositories
{
    public class AppointmentRepository:IAppointmentRepository
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IMongoCollection<Appointment> _appointmentsCollection;
        private readonly IDoctorRepository _doctorRepository;
        public AppointmentRepository(MongoDbService mongoDbService, IDoctorRepository doctorRepository)
        {
            _usersCollection = mongoDbService.GetUsersCollection();
            _appointmentsCollection = mongoDbService.GetAppointmentsCollection();
            _doctorRepository = doctorRepository;
        }
        public async Task AddAppointmentAsync(Appointment appointment)
        {
            await _appointmentsCollection.InsertOneAsync(appointment);
        }
        public async Task<List<Appointment>> GetUserAppointmentsAsync(string userId)
        {
            return await _appointmentsCollection
     .Find(a => a.UserId == userId)
     .SortByDescending(a => a.SlotDate)
     .ThenByDescending(a => a.SlotTime)
     .ToListAsync();

        }
        public async Task<Appointment> GetAppointmentByUserIdAsync(string appointmentId)
        {
            return await _appointmentsCollection
                .Find(a => a.Id == appointmentId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> CancelAppointmentAsync(string userId, string appointmentId)
        {
            var appointment = await _appointmentsCollection
                .Find(a => a.Id == appointmentId && a.UserId == userId)
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                return false;
            }

            var doctor = await _doctorRepository.GetDoctorByIdAsync(appointment.DocId);
            if (doctor != null)
            {
                appointment.Payment -= doctor.Fees;
            }

            var update = Builders<Appointment>.Update
                .Set(a => a.Cancelled, true)
                .Set(a => a.Payment, appointment.Payment); 

            var result = await _appointmentsCollection.UpdateOneAsync(
                a => a.Id == appointmentId && a.UserId == userId,
                update);

            return result.ModifiedCount > 0;
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            try
            {
                var appointments = await _appointmentsCollection.Find(_ => true).ToListAsync();

                return appointments;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching appointments: {ex.Message}");
            }
        }
        public async Task<bool> UpdateAppointmentAsync(Appointment appointment)
        {
            var filter = Builders<Appointment>.Filter.Eq(a => a.Id, appointment.Id);
            var result = await _appointmentsCollection.ReplaceOneAsync(filter, appointment);
            return result.ModifiedCount > 0;
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(string appointmentId)
        {
            var filter = Builders<Appointment>.Filter.Eq(a => a.Id, appointmentId);
            return await _appointmentsCollection.Find(filter).FirstOrDefaultAsync();
        }
        public async Task<Doctor?> GetDoctorByAppointmentIdAsync(string appointmentId)
        {
            var appointment = await _appointmentsCollection
                .Find(a => a.Id == appointmentId)
                .FirstOrDefaultAsync();

            if (appointment == null || string.IsNullOrEmpty(appointment.DocId))
            {
                return null;
            }

            var doctor = await _doctorRepository.GetDoctorByIdAsync(appointment.DocId);
            return doctor;
        }


    }
}

using DoctorAppointment.Models;
using DoctorAppointment.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace DoctorAppointment.Repositories
{
    public class UserRepository:IUserRepository
    {
        private readonly IMongoCollection<User> _usersCollection;
        public UserRepository(MongoDbService mongoDbService)
        {
            _usersCollection = mongoDbService.GetUsersCollection();
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                await _usersCollection.InsertOneAsync(user);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _usersCollection
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync();
        }
        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _usersCollection.Find(u=>u.Id==userId).FirstOrDefaultAsync();
        }

    }
}

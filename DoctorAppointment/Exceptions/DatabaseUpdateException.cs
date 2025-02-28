namespace DoctorAppointment.Exceptions
{
    [Serializable]
    internal class DatabaseUpdateException : Exception
    {
        public DatabaseUpdateException()
        {
        }

        public DatabaseUpdateException(string? message) : base(message)
        {
        }

        public DatabaseUpdateException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
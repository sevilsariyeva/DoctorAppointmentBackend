namespace DoctorAppointment.Exceptions
{
    [Serializable]
    internal class DoctorUnavailableException : Exception
    {
        public DoctorUnavailableException()
        {
        }

        public DoctorUnavailableException(string? message) : base(message)
        {
        }

        public DoctorUnavailableException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
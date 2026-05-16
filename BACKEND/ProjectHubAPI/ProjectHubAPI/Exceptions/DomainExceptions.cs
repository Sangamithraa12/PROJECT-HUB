using System;

namespace ProjectHubAPI.Exceptions
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
    }

    public class NotFoundException : DomainException
    {
        public NotFoundException(string name, object key) 
            : base($"Entity \"{name}\" ({key}) was not found.") { }
    }
}
 

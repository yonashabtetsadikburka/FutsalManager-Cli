namespace TournamentManager.Core.Exceptions;
// Thrown when an operation violates a business rule of the domani.

public class DomainException : Exception
{
    public DomainException(string message) : base(message){}
}
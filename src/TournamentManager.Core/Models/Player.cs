using TournamentManager.Core.Exceptions;
namespace TournamentManager.Core.Models;

public class Player
{
    public const int MinShirtNumber = 1;
    public  const int MaxShirtNumber = 99;
    public Guid Id { get; }
    public string Name { get; }
    public int? ShirtNumber { get; }
    public Player(string name, int? shirtNumber = null) : this(Guid.NewGuid(), name, shirtNumber){}
    public Player(Guid id, string name, int? shirtNumber = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Player name is required.");
        }
        if (shirtNumber is < MinShirtNumber or > MaxShirtNumber)
        {
            throw new DomainException($"Shirt number should be between {MinShirtNumber} and {MaxShirtNumber}.");
        }
        Id = id;
        Name = name.Trim();
        ShirtNumber = shirtNumber;
    }
}
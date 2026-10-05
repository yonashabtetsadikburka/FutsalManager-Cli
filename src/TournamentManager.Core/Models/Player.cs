using TournamentManager.Core.Exceptions;
namespace TournamentManager.Core.Models;

public class Player
{
    public Guid Id { get; }
    public string Name { get; }
    public Player(string name) : this(Guid.NewGuid(), name){}
    public Player(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Player name is required.");
        }
        Id = id;
        Name = name.Trim();
    }
}
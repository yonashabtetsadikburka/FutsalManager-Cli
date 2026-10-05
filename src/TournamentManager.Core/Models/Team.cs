using TournamentManager.Core.Exceptions;
namespace TournamentManager.Core.Models;
public class Team
{
    public const int MaxPlayers = 12;
    private readonly List<Player> _players = new();

    public Guid Id { get; }
    public string Name { get; }
    public IReadOnlyList<Player> Players => _players;

    public Team(string name) : this(Guid.NewGuid(), name, Enumerable.Empty<Player>()){}

    public Team(Guid id, string name, IEnumerable<Player> players)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Team name is required.");
        }

        Id = id;
        Name = name.Trim();

        foreach(var player in players)
        {
            AddPlayer(player);
        }
    }

    public Player AddPlayer(string playerName)
    {
        var player = new Player(playerName);
        AddPlayer(player);
        return player;
    }

    public void AddPlayer(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        if(_players.Count >= MaxPlayers)
        {
            throw new DomainException($"A team cannot have more than {MaxPlayers} players.");        
        }

        var alreadyExists = _players.Any(p => string.Equals(p.Name, player.Name, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            throw new DomainException($"Player '{player.Name}' already exists in team '{Name}'.");
        }

        _players.Add(player);
    }
}
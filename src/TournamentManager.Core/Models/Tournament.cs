using TournamentManager.Core.Exceptions;
using TournamentManager.Core.Scheduling;
using TournamentManager.Core.Standings;

namespace TournamentManager.Core.Models;

public class Tournament
{
    public const int MinTeams = 2;

    private readonly List<Team> _teams;
    private readonly List<Match> _matches;

    public Guid Id { get; }
    public string Name { get; }
    public IReadOnlyList<Team> Teams => _teams;
    public IReadOnlyList<Match> Matches => _matches;

    public bool IsStarted => _matches.Count > 0;

    public Tournament(string name) : this(Guid.NewGuid(), name, new List<Team>(), new List<Match>()){}

    public Tournament(Guid id, string name, IEnumerable<Team> teams, IEnumerable<Match> matches)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Tournament name is required.");
        }

        Id = id;
        Name = name.Trim();
        _teams = teams.ToList();
        _matches = matches.ToList();

    }

    public Team AddTeam(string teamName)
    {
        if (IsStarted)
        {
            throw new DomainException("Teams cannot be added after the tournament has started.");
        }
        var team = new Team(teamName);
        var alreadyExists = _teams.Any(t => string.Equals(t.Name, team.Name, StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
        {
            throw new DomainException("Team '{team.Name}' already exists");
        }
        _teams.Add(team);
        return team;
    }

    public Team GetTeam(Guid teamId)
    {
        return _teams.FirstOrDefault(t => t.Id == teamId) ?? throw new DomainException("Team not found.");
    }

    public void Start(IScheduleGenerator generator)
    {
        ArgumentNullException.ThrowIfNull(generator);

        if (IsStarted)
        {
            throw new DomainException("The tournament has already started.");
        }

        if(_teams.Count < MinTeams)
        {
            throw new DomainException($"At least {MinTeams} teams are required to start.");
        }

        _matches.AddRange(generator.Generate(_teams));
    }

    public void RecordResult(Guid matchId, MatchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        var match = _matches.FirstOrDefault(m => m.Id == matchId) ?? throw new DomainException("Match not found.");

        foreach(var goal in result.Goals)
        {
            var team = GetTeam(goal.TeamId);
            if(team.Players.All(p => p.Id != goal.PlayerId))
            {
                throw new DomainException("A scorer does not belong to the indicated team.");
            }
            
        }
        match.SetResult(result);
    }

    public IReadOnlyList<StandingRow> GetStandingRows(IComparer<StandingRow>? comparer = null)
    {
        return new StandingsCalculator(comparer).Calculate(_teams, _matches);
    }

    public IReadOnlyList<ScorerStat> GetTopScorers()
    {
        var goalsByPlayer = _matches.Where(m => m.IsPlayed).SelectMany(m => m.Result!.Goals).GroupBy(g => g.PlayerId).ToDictionary(group => group.Key, group => group.Count());
        var stats = new List<ScorerStat>();

        foreach(var team in _teams)
        {
            foreach(var player in team.Players)
            {
                if(goalsByPlayer.TryGetValue(player.Id, out var goals))
                {
                    stats.Add(new ScorerStat(player, team, goals));
                }
            }
        }

        return stats.OrderByDescending(s => s.Goals).ThenBy(s => s.Player.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }
}
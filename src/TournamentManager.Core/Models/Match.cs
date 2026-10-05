using TournamentManager.Core.Exceptions;
namespace TournamentManager.Core.Models;

public class Match
{
    public Guid Id { get; }
    public int Round { get; }
    public Guid HomeTeamId { get; }
    public Guid AwayTeamId { get; }
    public MatchResult? Result { get; private set; }

    public bool IsPlayed => Result is not null;

    public Match(int round, Guid homeTeamId, Guid awayTeamId) : this(Guid.NewGuid(), round, homeTeamId, awayTeamId, null){}

    public Match(Guid id, int round, Guid homeTeamId, Guid awayTeamId, MatchResult? result)
    {
        if(round < 1)
        {
            throw new DomainException("Round must be at least 1.");
        }
        if(homeTeamId == awayTeamId)
        {
            throw new DomainException("A team cannot play against itseld.");
        }

        Id = id;
        Round = round;
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;

        if(result is not null)
        {
            SetResult(result);
        }
    }

    public void SetResult(MatchResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        foreach(var goal in result.Goals)
        {
            if(goal.TeamId != HomeTeamId && goal.TeamId != AwayTeamId)
            {
                throw new DomainException("A scorer's team is not playing in this match.");
            }
        }

        var homeScorers = result.Goals.Count(g => g.TeamId == HomeTeamId);
        var awayScorers = result.Goals.Count(g => g.TeamId == AwayTeamId);

        if(homeScorers > result.HomeGoals || awayScorers > result.AwayGoals)
        {
            throw new DomainException("There are more registered scorers than golas scored.");
        }

        Result = result;
    }
}
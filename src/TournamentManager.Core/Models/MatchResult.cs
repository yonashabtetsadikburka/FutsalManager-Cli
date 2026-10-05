using TournamentManager.Core.Exceptions;
namespace TournamentManager.Core.Models;

public class MatchResult
{
    public int HomeGoals { get; }
    public int AwayGoals { get; }
    public IReadOnlyList<Goal> Goals { get; }

    public MatchResult(int homeGoals, int awayGoals, IEnumerable<Goal>? goals = null)
    {
        if(homeGoals < 0 || awayGoals < 0)
        {
            throw new DomainException("Goals cannot be negative.");
        }

        HomeGoals = homeGoals;
        AwayGoals = awayGoals;
        Goals = (goals ?? Enumerable.Empty<Goal>()).ToList();
    }
}
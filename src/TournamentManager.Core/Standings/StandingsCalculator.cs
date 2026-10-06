using TournamentManager.Core.Models;
namespace TournamentManager.Core.Standings;

public class StandingsCalculator{
    private readonly IComparer<StandingRow> _comparer;

    public StandingsCalculator(IComparer<StandingRow>? comparer = null)
    {
        _comparer = comparer ?? new DefaultStandingsComparer();
    }

    public IReadOnlyList<StandingRow> Calculate(IEnumerable<Team> teams, IEnumerable<Match> matches)
    {
        var rows = teams.ToDictionary(t => t.Id, t => new StandingRow(t));

        foreach(var match in matches.Where(m => m.IsPlayed))
        {
            var result = match.Result!;
            rows[match.HomeTeamId].AddResult(result.HomeGoals, result.AwayGoals);
            rows[match.AwayTeamId].AddResult(result.AwayGoals, result.HomeGoals);
        }

        var table = rows.Values.ToList();
        table.Sort(_comparer);
        return table;
    }
}
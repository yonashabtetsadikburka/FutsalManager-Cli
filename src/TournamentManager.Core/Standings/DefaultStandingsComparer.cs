namespace TournamentManager.Core.Standings;

//Orders by point, then goal diff, then goals scored, then team name.

public class DefaultStandingsComparer : IComparer<StandingRow>
{
    public int Compare(StandingRow? x, StandingRow? y)
    {
        if(ReferenceEquals(x, y)) return 0;
        if(x is null) return 1;
        if(y is null) return -1;

        //Descending: the one with more points comes first, so y is compared to x.

        var result = y.Points.CompareTo(x.Points);
        
        if(result != 0) return result;

        result = y.GoalDifference.CompareTo(x.GoalDifference);
        if(result != 0) return result;

        result = y.GoalsFor.CompareTo(x.GoalsFor);
        if(result != 0) return result;

        return string.Compare(x.Team.Name, y.Team.Name, StringComparison.OrdinalIgnoreCase);
    }
}

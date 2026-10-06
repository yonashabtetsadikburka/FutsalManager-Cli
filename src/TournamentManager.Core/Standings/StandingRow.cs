using TournamentManager.Core.Models;
namespace TournamentManager.Core.Standings;

public class StandingRow
{
    public const int PointsForWin = 3;
    public const int PointsForDraw = 1;

    public StandingRow(Team team)
    {
        Team = team;
    }

    public Team Team { get; }
    public int Played { get; private set; }
    public int Won { get; private set; }
    public int Drawn { get; private set; }
    public int Lost { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }

    public int GoalDifference => GoalsFor - GoalsAgainst;
    public int Points => Won * PointsForWin + Drawn * PointsForDraw;

    internal void AddResult(int goalsFor, int goalsAgainst)
    {
        Played++;
        GoalsFor += goalsFor;
        GoalsAgainst += goalsAgainst;

        if(goalsFor > goalsAgainst)
        {
            Won++;
        }
        else if(goalsFor == goalsAgainst)
        {
            Drawn++;
        }
        else
        {
            Lost++;
        }
    }
}
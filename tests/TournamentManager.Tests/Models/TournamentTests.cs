using TournamentManager.Core.Exceptions;
using TournamentManager.Core.Models;
using TournamentManager.Core.Scheduling;

namespace TournamentManager.Tests.Models;

public class TournamentTests
{
    private static Tournament CreateStartedTournament(params string[] teamNames)
    {
        var tournament = new Tournament("Test Cup");
        foreach (var name in teamNames)
        {
            tournament.AddTeam(name);
        }

        tournament.Start(new RoundRobinScheduleGenerator());
        return tournament;
    }

    [Fact]
    public void AddTeam_DuplicateName_Throws()
    {
        var tournament = new Tournament("Test Cup");
        tournament.AddTeam("Alpha");

        Assert.Throws<DomainException>(() => tournament.AddTeam("ALPHA"));
    }

    [Fact]
    public void AddTeam_AfterStart_Throws()
    {
        var tournament = CreateStartedTournament("Alpha", "Beta");

        Assert.Throws<DomainException>(() => tournament.AddTeam("Gamma"));
    }

    [Fact]
    public void Start_WithOneTeam_Throws()
    {
        var tournament = new Tournament("Test Cup");
        tournament.AddTeam("Alpha");

        Assert.Throws<DomainException>(() => tournament.Start(new RoundRobinScheduleGenerator()));
    }

    [Fact]
    public void GetStandings_NoResults_OrdersByNameWithZeroPoints()
    {
        var tournament = CreateStartedTournament("Charlie", "Alpha", "Bravo");

        var table = tournament.GetStandingRows();

        Assert.Equal(new[] { "Alpha", "Bravo", "Charlie" }, table.Select(r => r.Team.Name));
        Assert.All(table, row => Assert.Equal(0, row.Points));
    }

    [Fact]
    public void GetStandings_HomeWin_GivesThreePointsToWinner()
    {
        var tournament = CreateStartedTournament("Alpha", "Beta");
        var match = tournament.Matches[0];

        tournament.RecordResult(match.Id, new MatchResult(3, 1));
        var table = tournament.GetStandingRows();

        Assert.Equal(match.HomeTeamId, table[0].Team.Id);
        Assert.Equal(3, table[0].Points);
        Assert.Equal(2, table[0].GoalDifference);
        Assert.Equal(0, table[1].Points);
    }

    [Fact]
    public void GetStandings_Draw_GivesOnePointToBoth()
    {
        var tournament = CreateStartedTournament("Alpha", "Beta");

        tournament.RecordResult(tournament.Matches[0].Id, new MatchResult(2, 2));

        Assert.All(tournament.GetStandingRows(), row =>
        {
            Assert.Equal(1, row.Points);
            Assert.Equal(1, row.Drawn);
        });
    }

    [Fact]
    public void GetTopScorers_CountsGoalsPerPlayer()
    {
        var tournament = new Tournament("Test Cup");
        var alpha = tournament.AddTeam("Alpha");
        var mario = alpha.AddPlayer("Mario");
        tournament.AddTeam("Beta");
        tournament.Start(new RoundRobinScheduleGenerator());

        var match = tournament.Matches[0];
        var scorers = new[]
        {
            new Goal(alpha.Id, mario.Id),
            new Goal(alpha.Id, mario.Id)
        };

        // Alpha is the home team in the only match of a two-team tournament.
        tournament.RecordResult(match.Id, new MatchResult(2, 0, scorers));
        var topScorers = tournament.GetTopScorers();

        Assert.Single(topScorers);
        Assert.Equal("Mario", topScorers[0].Player.Name);
        Assert.Equal(2, topScorers[0].Goals);
    }

    [Fact]
    public void RecordResult_ScorerNotInTeam_Throws()
    {
        var tournament = CreateStartedTournament("Alpha", "Beta");
        var match = tournament.Matches[0];
        var scorers = new[] { new Goal(match.HomeTeamId, Guid.NewGuid()) };

        Assert.Throws<DomainException>(() =>
            tournament.RecordResult(match.Id, new MatchResult(1, 0, scorers)));
    }
}
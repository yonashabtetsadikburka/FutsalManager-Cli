using TournamentManager.Core.Exceptions;
using TournamentManager.Core.Models;
using TournamentManager.Core.Scheduling;

namespace TournamentManager.Tests.Scheduling;
public class RoundRobinScheduleGeneratorTests
{
    private readonly RoundRobinScheduleGenerator _generator = new();

    private static List<Team> CreateTeams(int count)
    {
        return Enumerable.Range(1, count).Select(i => new Team($"Team {i}")).ToList();
    }

    [Fact] 
    public void Generate_FewerThanTwoTeams_Throws()
    {
        Assert.Throws<DomainException>(() => _generator.Generate(CreateTeams(1)));
    }

    [Theory]
    [InlineData(2, 1)]
    [InlineData(3, 3)]
    [InlineData(4, 6)]
    [InlineData(5, 10)]
    [InlineData(8, 28)]

    public void Generate_ReturnsExpectedNumberOfMatches(int teamCount, int expectedMatches)
    {
        var matches = _generator.Generate(CreateTeams(teamCount));
        Assert.Equal(expectedMatches, matches.Count);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(8)]

    public void Generate_NoTeamPlaysTwiceInTheSameRound(int teamCount)
    {
        var matches = _generator.Generate(CreateTeams(teamCount));

        foreach(var round in matches.GroupBy(m=> m.Round))
        {
            var teamsInRound = round.SelectMany(m => new[] {m.HomeTeamId, m.AwayTeamId}).ToList();
            Assert.Equal(teamsInRound.Count, teamsInRound.Distinct().Count());
        }
    }

    [Fact]
    public void Generate_EvenNumberOfTeams_HasNMinusOneRounds()
    {
        var matches = _generator.Generate(CreateTeams(6));
        Assert.Equal(5, matches.Max(m => m.Round));
    }
}
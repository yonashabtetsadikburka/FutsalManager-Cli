using TournamentManager.Core.Exceptions;
using TournamentManager.Core.Models;

namespace TournamentManager.Tests.Models;

public class TeamTests
{
    [Fact]
    public void Constructor_BlankName_Throws()
    {
        Assert.Throws<DomainException>(() => new Team(" "));
    }

    [Fact]
    public void AddPlayer_ValidName_AddsPlayer()
    {
        var team = new Team("Red Lions");

        team.AddPlayer("Mario");
        Assert.Single(team.Players);
        Assert.Equal("Mario", team.Players[0].Name);
    }

    [Fact]
    public void AddPlayer_DuplicateIgnoringCase_Throws()
    {
        var team = new Team("Red Lions");
        team.AddPlayer("Mario");

        Assert.Throws<DomainException>(() => team.AddPlayer("mario"));
    }

    [Fact]
    public void AddPlayer_MoreThanMaxPlayers_Throws()
    {
        var team = new Team("Red Lions");

        for (var i = 1; i <= Team.MaxPlayers; i++)
        {
            team.AddPlayer($"Player {i}");
        }

        Assert.Throws<DomainException>(() => team.AddPlayer("One too many"));
    }
}
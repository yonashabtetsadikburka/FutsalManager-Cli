using TournamentManager.Core.Exceptions;
using TournamentManager.Core.Models;

namespace TournamentManager.Tests.Models;

public class MatchTests
{
    [Fact]
    public void Constructor_SameTeamOnBothSides_Throws()
    {
        var teamId = Guid.NewGuid();
        Assert.Throws<DomainException>(() => new Match(1, teamId, teamId));
    }

    [Fact]
    public void MatchResult_NegativeGoals_Throws()
    {
        var teamId = Guid.NewGuid();Assert.Throws<DomainException>(() => new MatchResult(-1, 0));
    }

    [Fact]
    public void SetResult_ValidResult_MarksMatchAsPlayed()
    {
        var match = new Match(1, Guid.NewGuid(), Guid.NewGuid());

        match.SetResult(new MatchResult(2, 1));
        Assert.True(match.IsPlayed);
        Assert.Equal(2, match.Result!.HomeGoals);
    }

    [Fact]
    public void SetResult_MoreScorersThanGoals_Throws()
    {
        var homeId = Guid.NewGuid();
        var match = new Match(1, homeId, Guid.NewGuid());
        var scorers = new[]
        {
            new Goal(homeId, Guid.NewGuid()),
            new Goal(homeId, Guid.NewGuid())
        };
        Assert.Throws<DomainException>(() => match.SetResult(new MatchResult(1, 0, scorers)));
    }

    [Fact]
    public void SetResult_ScorerFromAnotherTeam_Throws()
    {
        var match = new Match(1, Guid.NewGuid(), Guid.NewGuid());
        var scorers = new[] {new Goal(Guid.NewGuid(), Guid.NewGuid())};

        Assert.Throws<DomainException>(() => match.SetResult(new MatchResult(1, 0, scorers)));
    }
}
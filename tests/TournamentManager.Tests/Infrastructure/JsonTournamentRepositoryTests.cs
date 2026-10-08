using TournamentManager.Core.Models;
using TournamentManager.Core.Scheduling;
using TournamentManager.Infrastructure.Json;

namespace TournamentManager.Tests.Infrastructure;
public class JsonTournamentRepositoryTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "tournament-tests", Guid.NewGuid().ToString());

    private readonly JsonTournamentRepository _repository;

    public JsonTournamentRepositoryTests()
    {
        _repository = new JsonTournamentRepository(_folder);
    }
    public void Dispose()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, recursive: true);
        }
    }

    [Fact]
    public void Load_UnknownId_ReturnsNull()
    {
        Assert.Null(_repository.Load(Guid.NewGuid()));
    }
    [Fact]
    public void SaveThenLoad_PreservesTeamMatchesAndResults()
    {
        var tournament = new Tournament("Summer Cup");
        var alpha = tournament.AddTeam("Alpha");
        var mario = alpha.AddPlayer("Mario");
        tournament.AddTeam("Beta");
        tournament.Start(new RoundRobinScheduleGenerator());

        var match = tournament.Matches[0];
        tournament.RecordResult(match.Id, new MatchResult(1, 0, new[] {new Goal(alpha.Id, mario.Id)}));

        _repository.Save(tournament);
        var loaded = _repository.Load(tournament.Id);

        Assert.NotNull(loaded);
        Assert.Equal("Summer Cup", loaded.Name);
        Assert.Equal(2, loaded.Teams.Count);
        Assert.Single(loaded.Matches);
        Assert.True(loaded.Matches[0].IsPlayed);
        Assert.Equal(1, loaded.GetTopScorers()[0].Goals);
    }

    [Fact]
    public void List_ReturnsSavedTournamentsOrderedByName()
    {
        _repository.Save(new Tournament("Zeta"));
        _repository.Save(new Tournament("Alpha"));

        var list = _repository.List();

        Assert.Equal(new[] { "Alpha", "Zeta" }, list.Select(i => i.Name));
    }
}
using TournamentManager.Core.Models;

namespace TournamentManager.Core.Persistence;

public interface ITournamentRepository
{
    void Save(Tournament tournament);
    Tournament? Load(Guid id);

    IReadOnlyList<TournamentInfo> List();
}
namespace TournamentManager.Core.Models;
// A goal scored by a player of the given team.

public record Goal(Guid TeamId, Guid PlayerId);
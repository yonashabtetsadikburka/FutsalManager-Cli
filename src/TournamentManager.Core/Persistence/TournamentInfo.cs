namespace TournamentManager.Core.Persistence;
// Minimal description of a saved tournament, used to biuld lists.

public record TournamentInfo(Guid Id, string Name);
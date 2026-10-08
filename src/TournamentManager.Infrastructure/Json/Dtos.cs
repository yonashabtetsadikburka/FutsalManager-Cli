namespace TournamentManager.Infrastructure.Json;

public record PlayerDto(Guid Id, string Name);
public record TeamDto(Guid Id, string Name, List<PlayerDto> Players);
public record GoalDto(Guid TeamId, Guid PlayerId);

public record MatchResultDto(int HomeGoals, int AwayGoals, List<GoalDto> Goals);

public record MatchDto(Guid Id, int Round, Guid HomeTeamId, Guid AwayTeamId, MatchResultDto? Result);

public record TournamentDto(Guid Id, string Name, List<TeamDto> Teams, List<MatchDto> Matches);
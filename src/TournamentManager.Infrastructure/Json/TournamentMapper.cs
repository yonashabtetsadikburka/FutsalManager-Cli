using System.Diagnostics;

using TournamentManager.Core.Models;

namespace TournamentManager.Infrastructure.Json;

public static class TournamentMapper
{
    public static TournamentDto ToDto(Tournament tournament)
    {
        return new TournamentDto(
            tournament.Id,
            tournament.Name,
            tournament.Teams.Select(ToTeamDto).ToList(),
            tournament.Matches.Select(ToMatchDto).ToList());
    }

    public static Tournament ToDomain(TournamentDto dto)
    {
        var teams = dto.Teams.Select(ToTeam).ToList();
        var matches = dto.Matches.Select(ToMatch).ToList();

        return new Tournament(dto.Id, dto.Name, teams, matches);
    }

    private static TeamDto ToTeamDto(Team team)
    {
        return new TeamDto(
            team.Id,
            team.Name,
            team.Players.Select(p => new PlayerDto(p.Id, p.Name)).ToList());
        
    }


    private static MatchDto ToMatchDto(Match match)
    {
        MatchResultDto? resultDto = null;

        if(match.Result is not null)
        {
            resultDto = new MatchResultDto(
                match.Result.HomeGoals,
                match.Result.AwayGoals,
                match.Result.Goals.Select(g => new GoalDto(g.TeamId, g.PlayerId)).ToList());
            
        }

        return new MatchDto(match.Id, match.Round, match.HomeTeamId, match.AwayTeamId, resultDto);
    }

    private static Team ToTeam(TeamDto dto)
    {
        var players = dto.Players.Select(p => new Player(p.Id, p.Name));
        return new Team(dto.Id, dto.Name, players);
    }

    private static Match ToMatch(MatchDto dto)
    {
        MatchResult? result = null;

        if(dto.Result is not null)
        {
            var goals = dto.Result.Goals.Select(g => new Goal(g.TeamId, g.PlayerId));
            result = new MatchResult(dto.Result.HomeGoals, dto.Result.AwayGoals, goals);
        }

        return new Match(dto.Id, dto.Round, dto.HomeTeamId, dto.AwayTeamId, result);
    }
}
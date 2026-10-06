using TournamentManager.Core.Models;
namespace TournamentManager.Core.Scheduling;

public interface IScheduleGenerator
{
    IReadOnlyList<Match> Generate(IReadOnlyList<Team> teams);
}
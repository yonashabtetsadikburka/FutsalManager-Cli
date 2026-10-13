using TournamentManager.Cli;
using TournamentManager.Core.Scheduling;
using TournamentManager.Infrastructure.Json;

var dataFolder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "TournamentManager",
    "data"
);

var app = new ConsoleApp(
    new JsonTournamentRepository(dataFolder),
    new RoundRobinScheduleGenerator()
);

app.Run();
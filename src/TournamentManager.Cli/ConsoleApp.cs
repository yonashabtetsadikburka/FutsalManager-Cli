using System.Reflection;

using Microsoft.Win32.SafeHandles;

using Spectre.Console;
using TournamentManager.Core.Exceptions;
using TournamentManager.Core.Models;
using TournamentManager.Core.Persistence;
using TournamentManager.Core.Scheduling;

namespace TournamentManager.Cli;

public class ConsoleApp
{
    private const string NewTournamentOption = "New tournament";
    private const string OpenTournamentOption = "Open tournament";
    private const string ExitOption = "Exit";

    private const string AddTeamOption = "Add team";
    private const string AddPlayerOption = "Add player to a team";
    private const string StartOption = "Start tournament (generate schedule)";
    private const string EnterResultOption = "Enter a match result";
    private const string StandingsOption = "Show standings";
    private const string ScheduleOption = "Show schedule";
    private const string ScorersOption = "Show top scorers";
    private const string BackOption = "Back";

    private readonly ITournamentRepository _repository;
    private readonly IScheduleGenerator _scheduleGenerator;

    public ConsoleApp(ITournamentRepository repository, IScheduleGenerator scheduleGenerator)
    {
        _repository = repository;
        _scheduleGenerator = scheduleGenerator;
    }

    public void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Tournament Manager").Color(Color.Green));

            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("[bold]Main menu[/]").AddChoices(NewTournamentOption, OpenTournamentOption, ExitOption));

            switch (choice)
            {
                case NewTournamentOption:
                    Safe(CreateTournament);
                    break;
                case OpenTournamentOption:
                    Safe(OpenTournament);
                    break;
                default:
                    return;
            }
        }
    }

    //Runs an action and turns bussiness-rule errors into a friendly message.
    private static void Safe(Action action)
    {
        try
        {
            action();
        }
        catch(DomainException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/]{Markup.Escape(ex.Message)}");
            Pause();
        }
    }

    private static void Pause()
    {
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(intercept: true);
    }

    //To create and open a tournament

    private void CreateTournament()
    {
        var name = AnsiConsole.Ask<string>("Tournament [green]name[/]:");
        var tournament = new Tournament(name);

        _repository.Save(tournament);
        ShowTournamentMenu(tournament);
    }

    private void OpenTournament()
    {
        var tournaments = _repository.List();

        if(tournaments.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No saved tournaments yet.[/]");
            Pause();
            return;
        }

        var info = AnsiConsole.Prompt(
            new SelectionPrompt<TournamentInfo>()
            .Title("Choose a tournament")
            .UseConverter(i => Markup.Escape(i.Name))
            .AddChoices(tournaments)
        );

        var tournament = _repository.Load(info.Id);

        if(tournament is null)
        {
            AnsiConsole.MarkupLine("[red]Tournament file not found.[/]");
            Pause();
            return;
        }

        ShowTournamentMenu(tournament);
    }

    //Menu of the tournament

    private void ShowTournamentMenu(Tournament tournament)
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[green]{Markup.Escape(tournament.Name)}[/]").LeftJustified());
            AnsiConsole.MarkupLine(tournament.IsStarted
            ? "[grey]Status: in progress[/]"
            : "[grey]Status: not started (you can still add teams)[/]");

            var options = new List<string>();

            if (tournament.IsStarted)
            {
                options.Add(EnterResultOption);
            }
            else
            {
                options.AddRange(new[] { AddTeamOption, AddPlayerOption, StartOption });
            }

            options.AddRange(new[] { StandingsOption, ScorersOption, BackOption });

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                .Title("What do you want to do?")
                .AddChoices(options)
            );

            if(choice == BackOption)
            {
                return;
            }
            Safe(() => HandleTournamentChoice(tournament, choice));
        }
    }

    private void HandleTournamentChoice(Tournament tournament, string choice)
    {
        switch (choice)
        {
            case AddTeamOption:
                AddTeam(tournament);
                break;
            case AddPlayerOption:
                AddPlayer(tournament);
                break;
            case StartOption:
                StartTournament(tournament);
                break;
            case EnterResultOption:
                EnterResult(tournament);
                break;
            case StandingsOption:
                ShowStandings(tournament);
                break;
            case ScheduleOption:
                ShowSchedule(tournament);
                break;
            case ScorersOption:
                ShowTopScorers(tournament);
                break;
        }
    }

    //Composition of the tournament

    private void AddTeam(Tournament tournament)
    {
        var name = AnsiConsole.Ask<string>("Team [green]name[/]:");
        tournament.AddTeam(name);
        _repository.Save(tournament);
    }

    private void AddPlayer(Tournament tournament)
    {
        if(tournament.Teams.Count == 0)
        {
            throw new DomainException("Add at least one first.");
        }

        var team = AnsiConsole.Prompt(
            new SelectionPrompt<Team>()
            .Title("Which team?")
            .UseConverter(t => Markup.Escape(t.Name))
            .AddChoices(tournament.Teams)
        );

        var playerName = AnsiConsole.Ask<string>("Player [green]name[/]:");
        team.AddPlayer(playerName);
        _repository.Save(tournament);
    }

    private void StartTournament(Tournament tournament)
    {
        if(!AnsiConsole.Confirm("Once started you cannot add teams. Start now?"))
        {
            return;
        }

        tournament.Start(_scheduleGenerator);
        _repository.Save(tournament);

        AnsiConsole.MarkupLine($"[green]Schedule generated:{tournament.Matches.Count} matches.[/]");
        Pause();
    }

    //Inserting results and scorers

    private void EnterResult(Tournament tournament)
    {
        var match = AnsiConsole.Prompt(
            new SelectionPrompt<Match>()
            .Title("Which match?")
            .PageSize(12)
            .UseConverter(m => Markup.Escape(Describe(tournament, m)))
            .AddChoices(tournament.Matches)
        );

        var home = tournament.GetTeam(match.HomeTeamId);
        var away = tournament.GetTeam(match.AwayTeamId);

        var homeGoals = AskGoals($"Goals scored by [green]{Markup.Escape(home.Name)}[/]:");
        var awayGoals = AskGoals($"Goals scored by [green]{Markup.Escape(away.Name)}[/]:");

        var scorers = new List<Goal>();
        scorers.AddRange(AskScorers(home, homeGoals));
        scorers.AddRange(AskScorers(away, awayGoals));

        tournament.RecordResult(match.Id, new MatchResult(homeGoals, awayGoals, scorers));
        _repository.Save(tournament);

    }

    private static int AskGoals(string label)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<int>(label)
            .Validate(goals => goals >= 0
            ? ValidationResult.Success()
            : ValidationResult.Error("[red]Goals cannot be negative[/]"))
        );
    }

    private static List<Goal> AskScorers(Team team, int goals)
    {
        var result = new List<Goal>();

        if (goals == 0 || team.Players.Count == 0)
        {
            return result;
        }

        for (var i = 1; i <= goals; i++)
        {
            var prompt = $"Register the scorer of goal {i}/{goals} for {Markup.Escape(team.Name)}?";
            if (!AnsiConsole.Confirm(prompt))
            {
                break;
            }

            var player = AnsiConsole.Prompt(
                new SelectionPrompt<Player>()
                    .Title("Scorer")
                    .UseConverter(p => Markup.Escape(p.Name))
                    .AddChoices(team.Players));

            result.Add(new Goal(team.Id, player.Id));
        }

        return result;
    }

    private static string Describe(Tournament tournament, Match match)
    {
        var home = tournament.GetTeam(match.HomeTeamId).Name;
        var away = tournament.GetTeam(match.AwayTeamId).Name;

        var score = match.Result is null
        ? "-"
        : $"{match.Result.HomeGoals}-{match.Result.AwayGoals}";

        return $"Round {match.Round}: {home} vs {away} ({score})";
    }

    //Standings, schedules and scorers

    private static void ShowStandings(Tournament tournament)
    {
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("#");
        table.AddColumn("Team");

        foreach (var header in new[] { "P", "W", "D", "L", "GF", "GA", "GD", "Pts" })
        {
            table.AddColumn(new TableColumn(header).RightAligned());
        }

        var rows = tournament.GetStandingRows();

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            table.AddRow(
                (i + 1).ToString(),
                Markup.Escape(row.Team.Name),
                row.Played.ToString(),
                row.Won.ToString(),
                row.Drawn.ToString(),
                row.Lost.ToString(),
                row.GoalsFor.ToString(),
                row.GoalsAgainst.ToString(),
                row.GoalDifference.ToString(),
                $"[bold]{row.Points}[/]");
        }

        AnsiConsole.Write(table);
        Pause();
    }

    private static void ShowSchedule(Tournament tournament)
    {
        if (!tournament.IsStarted)
        {
            throw new DomainException("Start the tournament to generate the schedule.");
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Round");
        table.AddColumn("Home");
        table.AddColumn(new TableColumn("Result").Centered());
        table.AddColumn("Away");

        foreach (var match in tournament.Matches.OrderBy(m => m.Round))
        {
            var result = match.Result is null
                ? "[grey]-[/]"
                : $"{match.Result.HomeGoals} - {match.Result.AwayGoals}";

            table.AddRow(
                match.Round.ToString(),
                Markup.Escape(tournament.GetTeam(match.HomeTeamId).Name),
                result,
                Markup.Escape(tournament.GetTeam(match.AwayTeamId).Name));
        }

        AnsiConsole.Write(table);
        Pause();
    }

    private static void ShowTopScorers(Tournament tournament)
    {
        var scorers = tournament.GetTopScorers();

        if (scorers.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No goals registered yet.[/]");
            Pause();
            return;
        }

        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("Player");
        table.AddColumn("Team");
        table.AddColumn(new TableColumn("Goals").RightAligned());

        foreach (var scorer in scorers)
        {
            table.AddRow(
                Markup.Escape(scorer.Player.Name),
                Markup.Escape(scorer.Team.Name),
                scorer.Goals.ToString());
        }

        AnsiConsole.Write(table);
        Pause();
    }
}
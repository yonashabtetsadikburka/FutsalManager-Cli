using System.Text.Json;

using Microsoft.VisualBasic;

using TournamentManager.Core.Models;
using TournamentManager.Core.Persistence;

namespace TournamentManager.Infrastructure.Json;

//Stores each tournament in its own JSON file inside a folder.

public class JsonTournamentRepository : ITournamentRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _folder;

    public JsonTournamentRepository(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            throw new ArgumentException("Folder path is required.", nameof(folder));
        }

        _folder = folder;
        Directory.CreateDirectory(_folder);
    }

    public void Save(Tournament tournament)
    {
        ArgumentNullException.ThrowIfNull(tournament);
        var dto = TournamentMapper.ToDto(tournament);
        var json = JsonSerializer.Serialize(dto, SerializerOptions);
        File.WriteAllText(GetPath(tournament.Id), json);
    }

    public Tournament? Load(Guid id)
    {
        var path = GetPath(id);
        if (!File.Exists(path))
        {
            return null;
        }
        var dto = ReadDto(path);
        return dto is null ? null : TournamentMapper.ToDomain(dto);
    }

    public IReadOnlyList<TournamentInfo> List()
    {
        return Directory.EnumerateFiles(_folder, "*.json")
        .Select(ReadDto)
        .Where(dto => dto is not null)
        .Select(dto => new TournamentInfo(dto!.Id, dto.Name))
        .OrderBy(info => info.Name, StringComparer.OrdinalIgnoreCase)
        .ToList();
    }

    private string GetPath(Guid id) => Path.Combine(_folder, $"{id}.json");

    private static TournamentDto? ReadDto(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<TournamentDto>(json, SerializerOptions);
    }
}
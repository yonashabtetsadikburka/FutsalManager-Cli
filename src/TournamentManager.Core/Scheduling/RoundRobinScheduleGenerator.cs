using System.Runtime.CompilerServices;

using TournamentManager.Core.Exceptions;
using TournamentManager.Core.Models;

namespace TournamentManager.Core.Scheduling;

//Single round-robin: every team plays every other team once.
//Uses the "circle method".

public class RoundRobinScheduleGenerator : IScheduleGenerator
{
    public IReadOnlyList<Match> Generate(IReadOnlyList<Team> teams)
    {
        ArgumentNullException.ThrowIfNull(teams);
        if(teams.Count < 2)
        {
            throw new DomainException("At least 2 teams are required to generate a schedule.");
        }

        //One slot per team. A null slot is a rest when the number of teams is odd.

        var slots = teams.Select(t => (Guid?)t.Id).ToList();
        if(slots.Count % 2 != 0)
        {
            slots.Add(null);
        }

        var slotCount = slots.Count;
        var roundCount = slotCount - 1;
        var matches = new List<Match>();

        for(var round = 0; round < roundCount; round++)
        {
            for(var i = 0; i < slotCount / 2; i++)
            {
                var home = slots[i];
                var away = slots[slotCount - 1 - i];

                if(home is null || away is null)
                {
                    continue; //one team rests this round
                }

                matches.Add(new Match(round + 1, home.Value, away.Value));
            }
            Rotate(slots);

        }
        return matches;

    }

    //Keeps the first slot fixed and move the last one to the second position.

    private static void Rotate(List<Guid?> slots)
    {
        var last = slots[^1];
        slots.RemoveAt(slots.Count - 1);
        slots.Insert(1, last);
    }
}

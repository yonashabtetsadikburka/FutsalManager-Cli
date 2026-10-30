using TournamentManager.Core.Exceptions;
using TournamentManager.Core.Models;

namespace TournamentManager.Tests.Models;

public class PlayerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(-5)]
    public void Constructor_ShirtNumberOutOfRange_Throws(int number)
    {
        Assert.Throws<DomainException>(() => new Player("Mario", number));
    }

    [Fact]
    public void Constructor_NoShirtNumber_IsAllowed()
    {
        var player = new Player("Mario");

        Assert.Null(player.ShirtNumber);
    }

    [Fact]
    public void Constructor_ValidShirtNumber_IsStored()
    {
        var player = new Player("Mario", 9);

        Assert.Equal(9, player.ShirtNumber);
    }
}
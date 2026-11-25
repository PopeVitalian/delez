using Delez.Api.Domain;
using Xunit;

namespace Delez.Tests;

public class DelitevStroskaTests
{
    [Fact]
    public void EnakomernaDelitev_90MedTriClane_VsakDobi30()
    {
        var delezi = DelitevStroska.Enakomerno(90.00m, [1, 2, 3]);
        Assert.Equal(30.00m, delezi[1]);
        Assert.Equal(30.00m, delezi[2]);
        Assert.Equal(30.00m, delezi[3]);
    }

    [Fact]
    public void DelitevZOstankom_10MedTriClane_VsotaJeNatanchnoEnakaZnesku()
    {
        var delezi = DelitevStroska.Enakomerno(10.00m, [1, 2, 3]);
        Assert.Equal(10.00m, delezi.Values.Sum());
        Assert.Equal(3.34m, delezi[1]);
        Assert.Equal(3.33m, delezi[2]);
        Assert.Equal(3.33m, delezi[3]);
    }

    [Fact]
    public void Delitev_PrazenSeznamClanov_VrzeneIzjemo()
    {
        Assert.Throws<ArgumentException>(() => DelitevStroska.Enakomerno(10.00m, []));
    }

    [Fact]
    public void Delitev_100MedSestClanov_VsotaJeEnakaZnesku()
    {
        var delezi = DelitevStroska.Enakomerno(100.00m, [1, 2, 3, 4, 5, 6]);
        Assert.Equal(100.00m, delezi.Values.Sum());
        Assert.Equal(6, delezi.Count);
    }

    [Fact]
    public void Delitev_ZaEnegaClana_ClanPrejmeVesZnesek()
    {
        var delezi = DelitevStroska.Enakomerno(45.50m, [7]);
        Assert.Single(delezi);
        Assert.Equal(45.50m, delezi[7]);
    }
}

using Delez.Api.Domain;
using Delez.Api.Models;
using Xunit;

namespace Delez.Tests;

public class IzracunStanjTests
{
    private static Strosek Strosek(int placnikId, decimal znesek, params (int clanstvoId, decimal znesek)[] delezi) =>
        new()
        {
            PlacnikId = placnikId,
            Znesek = znesek,
            Delezi = delezi
                .Select(d => new DelezStroska { ClanstvoId = d.clanstvoId, Znesek = d.znesek })
                .ToList()
        };

    private static Nakazilo Nakazilo(int placnikId, int prejemnikId, decimal znesek) =>
        new() { PlacnikId = placnikId, PrejemnikId = prejemnikId, Znesek = znesek };

    [Fact]
    public void BrezStroskovInNakazil_SoVsaStanjaNicla()
    {
        var stanja = IzracunStanj.Izracunaj([1, 2, 3], [], []);
        Assert.Equal(0m, stanja[1]);
        Assert.Equal(0m, stanja[2]);
        Assert.Equal(0m, stanja[3]);
    }

    [Fact]
    public void Strosek_PlacnikuPrisezeRazlika_DeleznikiIzgubijo()
    {
        var strosek = Strosek(1, 90.00m, (1, 30.00m), (2, 30.00m), (3, 30.00m));
        var stanja = IzracunStanj.Izracunaj([1, 2, 3], [strosek], []);
        Assert.Equal(60.00m, stanja[1]);
        Assert.Equal(-30.00m, stanja[2]);
        Assert.Equal(-30.00m, stanja[3]);
    }

    [Fact]
    public void VsotaStanjVsehClanovJeVednoNicla()
    {
        var s1 = Strosek(1, 45.00m, (1, 22.50m), (2, 22.50m));
        var s2 = Strosek(3, 60.00m, (2, 30.00m), (3, 30.00m));
        var stanja = IzracunStanj.Izracunaj([1, 2, 3], [s1, s2], []);
        Assert.Equal(0m, stanja.Values.Sum());
    }

    [Fact]
    public void Nakazilo_PlacnikuStanjeRaste_PrejemnikuPada()
    {
        var stanja = IzracunStanj.Izracunaj([1, 2], [], [Nakazilo(2, 1, 30.00m)]);
        Assert.Equal(-30.00m, stanja[1]);
        Assert.Equal(30.00m, stanja[2]);
    }

    [Fact]
    public void StrosekInNakaziloSkupaj_DajoPravilnoKoncnoStanje()
    {
        var strosek = Strosek(1, 90.00m, (1, 30.00m), (2, 30.00m), (3, 30.00m));
        var stanja =
            IzracunStanj.Izracunaj([1, 2, 3], [strosek], [Nakazilo(2, 1, 30.00m)]);
        Assert.Equal(30.00m, stanja[1]);
        Assert.Equal(0.00m, stanja[2]);
        Assert.Equal(-30.00m, stanja[3]);
    }
}

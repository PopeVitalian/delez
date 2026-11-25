using Delez.Api.Domain;
using Xunit;

namespace Delez.Tests;

public class PredlogNakazilTests
{
    [Fact]
    public void VsaStanjaPoravnana_PredlogJePrazen()
    {
        var predlog =
            PredlogNakazil.Izracunaj(new Dictionary<int, decimal> { { 1, 0m }, { 2, 0m } });
        Assert.Empty(predlog);
    }

    [Fact]
    public void EnDolznikEnUpnik_EnoSamoNakazilo()
    {
        var predlog =
            PredlogNakazil.Izracunaj(new Dictionary<int, decimal> { { 1, 30.00m }, { 2, -30.00m } });
        var nakazilo = Assert.Single(predlog);
        Assert.Equal(2, nakazilo.PlacnikId);
        Assert.Equal(1, nakazilo.PrejemnikId);
        Assert.Equal(30.00m, nakazilo.Znesek);
    }

    [Fact]
    public void SteviloPredlogovNePresezaNClenovMinusEna()
    {
        var stanja = new Dictionary<int, decimal> { { 1, 40.00m }, { 2, -30.00m }, { 3, -10.00m }, { 4, 0m } };
        var predlog = PredlogNakazil.Izracunaj(stanja);
        Assert.True(predlog.Count <= stanja.Count(s => s.Value != 0) - 1);
        Assert.Equal(2, predlog.Count);
    }

    [Fact]
    public void PredlogiSoUrejeniPoZneskuPadajoce()
    {
        var stanja = new Dictionary<int, decimal> { { 1, 10.00m }, { 2, -40.00m }, { 3, 30.00m } };
        var predlog = PredlogNakazil.Izracunaj(stanja);
        Assert.Equal(30.00m, predlog[0].Znesek);
        Assert.True(predlog[0].Znesek >= predlog[^1].Znesek);
    }

    [Fact]
    public void IzvedbaPredlogovPopolnomaPoravnaStanja()
    {
        var stanja = new Dictionary<int, decimal> { { 1, 25.00m }, { 2, -15.00m }, { 3, -10.00m } };
        var predlog = PredlogNakazil.Izracunaj(stanja);
        foreach (var p in predlog)
        {
            stanja[p.PlacnikId] += p.Znesek;
            stanja[p.PrejemnikId] -= p.Znesek;
        }
        Assert.All(stanja.Values, v => Assert.Equal(0m, v));
    }
}

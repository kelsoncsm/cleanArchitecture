using Loja.Domain.Common;

namespace Loja.Tests.Domain;

/// <summary>Testes do Value Object: garantem que "Dinheiro inválido" simplesmente não existe.</summary>
public class DinheiroTests
{
    [Fact]
    public void De_ComValorNegativo_LancaDominioException()
    {
        Assert.Throws<DominioException>(() => Dinheiro.De(-1m));
    }

    [Fact]
    public void De_ArredondaParaDuasCasasDecimais()
    {
        Assert.Equal(10.13m, Dinheiro.De(10.125m).Valor);
    }

    [Fact]
    public void IgualdadePorValor_DoisDinheirosComMesmoValor_SaoIguais()
    {
        Assert.Equal(Dinheiro.De(10m), Dinheiro.De(10m));
    }

    [Fact]
    public void Percentual_DezPorCentoDeDuzentos_EhVinte()
    {
        Assert.Equal(20m, Dinheiro.De(200m).Percentual(10m).Valor);
    }
}

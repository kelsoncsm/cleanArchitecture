using Loja.Domain.Common;
using Loja.Domain.Descontos;
using Loja.Domain.Pedidos;

namespace Loja.Tests.Domain;

/// <summary>
/// Testes do Strategy. Note que a calculadora é criada com uma lista de políticas: nos testes escolhemos
/// exatamente quais políticas participam (isolamento graças à Injeção de Dependência).
/// </summary>
public class DescontoTests
{
    private static readonly IPoliticaDesconto[] Politicas = { new DescontoClienteVip(), new DescontoPorQuantidade() };

    private static Pedido PedidoCom(bool vip, int quantidade, decimal preco)
    {
        var pedido = Pedido.Criar("Maria", vip, DateTime.UtcNow);
        pedido.AdicionarItem("Produto", Dinheiro.De(preco), quantidade);
        return pedido;
    }

    [Fact]
    public void ClienteVip_Recebe10PorCento()
    {
        var pedido = PedidoCom(vip: true, quantidade: 1, preco: 200m);

        Assert.Equal(20m, new DescontoClienteVip().Calcular(pedido).Valor);
    }

    [Fact]
    public void ClienteComum_NaoRecebeDescontoVip()
    {
        var pedido = PedidoCom(vip: false, quantidade: 1, preco: 200m);

        Assert.Equal(0m, new DescontoClienteVip().Calcular(pedido).Valor);
    }

    [Fact]
    public void Quantidade_DezOuMais_Recebe5PorCento()
    {
        var pedido = PedidoCom(vip: false, quantidade: 10, preco: 10m);

        Assert.Equal(5m, new DescontoPorQuantidade().Calcular(pedido).Valor);
    }

    [Fact]
    public void Calculadora_EscolheOMaiorDesconto_SemAcumular()
    {
        // VIP: 10% de 1000 = 100. Volume: 5% de 1000 = 50. Vence o VIP (100), sem somar (150).
        var pedido = PedidoCom(vip: true, quantidade: 10, preco: 100m);

        var desconto = new CalculadoraDeDesconto(Politicas).CalcularMelhorDesconto(pedido);

        Assert.Equal(100m, desconto.Valor);
    }

    [Fact]
    public void Calculadora_SemPoliticas_NaoDaDesconto()
    {
        var pedido = PedidoCom(vip: true, quantidade: 10, preco: 100m);

        var desconto = new CalculadoraDeDesconto(Array.Empty<IPoliticaDesconto>()).CalcularMelhorDesconto(pedido);

        Assert.Equal(Dinheiro.Zero, desconto);
    }
}

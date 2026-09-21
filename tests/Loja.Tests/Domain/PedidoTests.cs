using Loja.Domain.Common;
using Loja.Domain.Pedidos;

namespace Loja.Tests.Domain;

/// <summary>
/// Testes de DOMÍNIO: rápidos (milissegundos), sem banco, sem HTTP, sem mocks.
/// Isso só é possível porque o Domain não depende de nada externo - um dos maiores benefícios da Clean Architecture.
///
/// Convenção de nome: Metodo_Cenario_ResultadoEsperado (o nome do teste já documenta a regra de negócio).
/// Estrutura: Arrange (preparar) / Act (agir) / Assert (verificar).
/// </summary>
public class PedidoTests
{
    private static Pedido NovoPedido(bool vip = false) =>
        Pedido.Criar("Maria", vip, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    [Fact]
    public void Criar_SemNomeDeCliente_LancaDominioException()
    {
        Assert.Throws<DominioException>(() => Pedido.Criar("  ", false, DateTime.UtcNow));
    }

    [Fact]
    public void Subtotal_ComVariosItens_SomaOsSubtotais()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem("Teclado", Dinheiro.De(150m), 2);
        pedido.AdicionarItem("Mouse", Dinheiro.De(80m), 1);

        Assert.Equal(380m, pedido.Subtotal.Valor);
    }

    [Fact]
    public void AdicionarItem_ComQuantidadeZero_LancaDominioException()
    {
        var pedido = NovoPedido();

        Assert.Throws<DominioException>(() => pedido.AdicionarItem("Cabo", Dinheiro.De(10m), 0));
    }

    [Fact]
    public void Confirmar_SemItens_LancaDominioException()
    {
        var pedido = NovoPedido();

        Assert.Throws<DominioException>(() => pedido.Confirmar(Dinheiro.Zero));
    }

    [Fact]
    public void Confirmar_ComDesconto_AtualizaStatusETotal()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem("Teclado", Dinheiro.De(100m), 1);

        pedido.Confirmar(Dinheiro.De(10m));

        Assert.Equal(StatusPedido.Confirmado, pedido.Status);
        Assert.Equal(90m, pedido.Total.Valor);
    }

    [Fact]
    public void Confirmar_ComDescontoMaiorQueSubtotal_LancaDominioException()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem("Teclado", Dinheiro.De(100m), 1);

        Assert.Throws<DominioException>(() => pedido.Confirmar(Dinheiro.De(150m)));
    }

    [Fact]
    public void AdicionarItem_EmPedidoConfirmado_LancaDominioException()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem("Teclado", Dinheiro.De(100m), 1);
        pedido.Confirmar(Dinheiro.Zero);

        Assert.Throws<DominioException>(() => pedido.AdicionarItem("Mouse", Dinheiro.De(50m), 1));
    }

    [Fact]
    public void Cancelar_PedidoConfirmado_LancaDominioException()
    {
        var pedido = NovoPedido();
        pedido.AdicionarItem("Teclado", Dinheiro.De(100m), 1);
        pedido.Confirmar(Dinheiro.Zero);

        Assert.Throws<DominioException>(pedido.Cancelar);
    }
}

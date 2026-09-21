using Loja.Domain.Common;
using Loja.Domain.Pedidos;

namespace Loja.Domain.Descontos;

/// <summary>Estratégia concreta: compras com 10 ou mais unidades ganham 5% sobre o subtotal.</summary>
public sealed class DescontoPorQuantidade : IPoliticaDesconto
{
    private const int QuantidadeMinima = 10;
    private const decimal Percentual = 5m;

    public string Nome => "Volume (10+ unidades)";

    public Dinheiro Calcular(Pedido pedido) =>
        pedido.QuantidadeTotalDeItens >= QuantidadeMinima
            ? pedido.Subtotal.Percentual(Percentual)
            : Dinheiro.Zero;
}

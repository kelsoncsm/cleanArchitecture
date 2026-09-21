using Loja.Domain.Pedidos;

namespace Loja.Application.Pedidos;

/// <summary>
/// Converte a entidade de domínio em DTO de resposta.
///
/// POR QUE um mapper manual e não AutoMapper?
/// - Para um sistema simples, 15 linhas explícitas são mais fáceis de entender e depurar do que "mágica" por
///   convenção, e o compilador avisa se um campo mudar. (Para dezenas de DTOs, uma biblioteca pode compensar.)
/// - Concentrar a conversão aqui evita duplicá-la em cada caso de uso (DRY).
/// </summary>
internal static class PedidoMapper
{
    public static PedidoResponse ParaResponse(this Pedido pedido) =>
        new(
            pedido.Id,
            pedido.Cliente,
            pedido.ClienteVip,
            pedido.Status.ToString(),
            pedido.CriadoEmUtc,
            pedido.Itens
                .Select(i => new ItemPedidoResponse(i.Produto, i.PrecoUnitario.Valor, i.Quantidade, i.Subtotal.Valor))
                .ToList(),
            pedido.Subtotal.Valor,
            pedido.Desconto.Valor,
            pedido.Total.Valor);
}

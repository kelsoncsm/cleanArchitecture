namespace Loja.Application.Pedidos;

// ---------------------------------------------------------------------------------------------------------
// DTOs (Data Transfer Objects): o "formato dos dados" que entra e sai da aplicação.
//
// POR QUE não expor a entidade <c>Pedido</c> diretamente na API?
// - A entidade tem regras e estado interno protegido; o contrato público (JSON) precisa ser ESTÁVEL. Se
//   expuséssemos a entidade, qualquer refatoração do Domain quebraria os clientes da API.
// - Evita vazar detalhes internos e o famoso "over-posting" (cliente enviando campos que não deveria controlar,
//   como Status ou Desconto).
//
// POR QUE <c>record</c>?
// - Imutáveis, com igualdade por valor e sintaxe curta: perfeitos para carregar dados sem comportamento.
// ---------------------------------------------------------------------------------------------------------

public sealed record ItemPedidoRequest(string Produto, decimal PrecoUnitario, int Quantidade);

public sealed record CriarPedidoRequest(string Cliente, bool ClienteVip, IReadOnlyList<ItemPedidoRequest> Itens);

public sealed record ConfirmarPedidoRequest(Guid PedidoId);

public sealed record ObterPedidoRequest(Guid PedidoId);

/// <summary>Request vazio: "listar" não recebe parâmetros, mas a interface genérica exige um tipo.</summary>
public sealed record ListarPedidosRequest;

public sealed record ItemPedidoResponse(string Produto, decimal PrecoUnitario, int Quantidade, decimal Subtotal);

public sealed record PedidoResponse(
    Guid Id,
    string Cliente,
    bool ClienteVip,
    string Status,
    DateTime CriadoEmUtc,
    IReadOnlyList<ItemPedidoResponse> Itens,
    decimal Subtotal,
    decimal Desconto,
    decimal Total);

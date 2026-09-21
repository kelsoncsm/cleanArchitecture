namespace Loja.Application.Common;

/// <summary>
/// Contrato de um CASO DE USO (uma ação que o sistema oferece: "criar pedido", "confirmar pedido"...).
///
/// POR QUE um caso de uso por classe (e não um "PedidoService" com 20 métodos)?
/// - SRP: cada classe tem UM motivo para mudar. Mexer em "confirmar" não arrisca quebrar "criar".
/// - Classes pequenas são fáceis de ler, testar e dar nome (o nome já documenta o que o sistema FAZ).
///
/// POR QUE uma interface genérica única?
/// - É ela que permite o padrão DECORATOR (log, tratamento de exceção...) ser escrito UMA vez e aplicado
///   a TODOS os casos de uso, sem alterar nenhum deles (OCP).
/// - O endpoint da API depende desta interface (DIP), nunca da classe concreta.
///
/// Nota: este é um "CQRS leve" feito à mão. Em sistemas grandes costuma-se usar a biblioteca MediatR, mas para
/// uma arquitetura simples a interface acima entrega o mesmo benefício sem dependência extra.
/// </summary>
public interface ICasoDeUso<in TRequest, TResponse>
{
    Task<Result<TResponse>> ExecutarAsync(TRequest request, CancellationToken cancellationToken);
}

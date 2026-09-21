namespace Loja.Domain.Pedidos;

/// <summary>
/// PADRÃO REPOSITORY: uma "coleção de pedidos em memória" do ponto de vista do negócio.
///
/// POR QUE a INTERFACE fica no Domain e a IMPLEMENTAÇÃO fica na Infrastructure?
/// - É a Inversão de Dependência (o "D" do SOLID) na prática. O negócio declara "eu preciso de algo que guarde
///   pedidos" e não se importa se é SQL Server, MongoDB ou um dicionário em memória.
/// - A seta de dependência aponta para o Domain: Infrastructure -> Domain (e não Domain -> Infrastructure).
/// - Nos testes, substituímos por uma implementação em memória sem tocar em nenhuma regra de negócio.
///
/// POR QUE é uma interface pequena e focada? (ISP - Interface Segregation, o "I" do SOLID)
/// - Só tem o que os casos de uso realmente precisam. Não existe um "IRepository&lt;T&gt;" genérico com 15 métodos
///   que ninguém usa; quem implementa não é obrigado a suportar o que não precisa.
///
/// POR QUE CancellationToken e Task?
/// - Acesso a dados é I/O. Async libera threads do servidor e permite cancelar a operação quando o cliente desiste.
/// </summary>
public interface IPedidoRepository
{
    Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken);

    Task<Pedido?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Pedido>> ListarAsync(CancellationToken cancellationToken);

    Task AtualizarAsync(Pedido pedido, CancellationToken cancellationToken);
}

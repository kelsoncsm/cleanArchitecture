using Loja.Domain.Common;

namespace Loja.Domain.Pedidos;

/// <summary>
/// AGGREGATE ROOT: o Pedido é o "chefe" do seu agregado (ele + seus itens).
/// Toda alteração passa por métodos que validam as regras; ninguém de fora consegue deixar o pedido em estado inválido.
///
/// PRINCÍPIOS aplicados aqui
/// - Encapsulamento / "Tell, Don't Ask": em vez de o use case ler campos e decidir (anêmico), ele PEDE ao pedido
///   que faça: <c>pedido.Confirmar(...)</c>. A regra fica junto dos dados que ela protege (alta coesão).
/// - SRP (Single Responsibility): esta classe só sabe do que é próprio de um pedido. Persistência, JSON, HTTP e
///   log NÃO estão aqui.
/// - Factory Method (<see cref="Criar"/>): o construtor é privado; a criação passa por um método com nome
///   que expressa a intenção e garante que o objeto nasce válido.
/// </summary>
public sealed class Pedido : Entidade
{
    // Lista privada e mutável por dentro; o mundo externo só enxerga uma versão somente-leitura (Itens).
    private readonly List<ItemPedido> _itens = new();

    public string Cliente { get; }
    public bool ClienteVip { get; }
    public DateTime CriadoEmUtc { get; }
    public StatusPedido Status { get; private set; }
    public Dinheiro Desconto { get; private set; } = Dinheiro.Zero;

    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

    public int QuantidadeTotalDeItens => _itens.Sum(i => i.Quantidade);

    public Dinheiro Subtotal => _itens.Aggregate(Dinheiro.Zero, (acumulado, item) => acumulado + item.Subtotal);

    public Dinheiro Total => Subtotal - Desconto;

    private Pedido(Guid id, string cliente, bool clienteVip, DateTime criadoEmUtc) : base(id)
    {
        Cliente = cliente;
        ClienteVip = clienteVip;
        CriadoEmUtc = criadoEmUtc;
        Status = StatusPedido.Aberto;
    }

    /// <summary>
    /// Recebe a data como parâmetro (em vez de chamar <c>DateTime.UtcNow</c> aqui dentro) para o Domain ficar
    /// PURO e determinístico: fácil de testar e sem dependência escondida do relógio do sistema.
    /// </summary>
    public static Pedido Criar(string cliente, bool clienteVip, DateTime criadoEmUtc)
    {
        if (string.IsNullOrWhiteSpace(cliente))
            throw new DominioException("O nome do cliente é obrigatório.");

        return new Pedido(Guid.NewGuid(), cliente.Trim(), clienteVip, criadoEmUtc);
    }

    public void AdicionarItem(string produto, Dinheiro precoUnitario, int quantidade)
    {
        GarantirQueEstaAberto();
        _itens.Add(new ItemPedido(produto, precoUnitario, quantidade));
    }

    /// <summary>
    /// Confirma o pedido aplicando o desconto já calculado.
    /// Quem CALCULA o desconto é a <c>CalculadoraDeDesconto</c> (Strategy); o pedido só valida e aplica.
    /// Isso mantém o SRP e deixa o pedido sem conhecer as políticas comerciais (que mudam com frequência).
    /// </summary>
    public void Confirmar(Dinheiro desconto)
    {
        GarantirQueEstaAberto();

        if (_itens.Count == 0)
            throw new DominioException("Não é possível confirmar um pedido sem itens.");

        if (desconto > Subtotal)
            throw new DominioException("O desconto não pode ser maior que o subtotal do pedido.");

        Desconto = desconto;
        Status = StatusPedido.Confirmado;
    }

    public void Cancelar()
    {
        if (Status == StatusPedido.Cancelado)
            throw new DominioException("O pedido já está cancelado.");

        if (Status == StatusPedido.Confirmado)
            throw new DominioException("Um pedido confirmado não pode ser cancelado.");

        Status = StatusPedido.Cancelado;
    }

    private void GarantirQueEstaAberto()
    {
        if (Status != StatusPedido.Aberto)
            throw new DominioException($"O pedido está {Status} e não pode mais ser alterado.");
    }
}

using Loja.Domain.Common;
using Loja.Domain.Pedidos;

namespace Loja.Domain.Descontos;

/// <summary>
/// PADRÃO STRATEGY: cada política de desconto é uma "estratégia" intercambiável atrás desta interface.
///
/// O PROBLEMA que resolve
/// Sem o Strategy, o cálculo viraria um método com vários <c>if</c> (if VIP..., else if quantidade...).
/// A cada nova promoção (Black Friday, cupom...) alteraríamos esse método gigante, com risco de quebrar as regras antigas.
///
/// O QUE GANHAMOS (SOLID)
/// - OCP (Open/Closed, o "O"): para criar uma nova promoção, ADICIONAMOS uma classe nova. Nenhum código existente muda.
/// - SRP (o "S"): cada classe conhece UMA regra de desconto.
/// - LSP (Liskov, o "L"): qualquer implementação pode ser usada no lugar de outra sem surpresas
///   (sempre devolve um Dinheiro >= 0, nunca lança exceção para pedidos válidos).
/// </summary>
public interface IPoliticaDesconto
{
    /// <summary>Nome exibível da política (útil para logs e para explicar o desconto ao cliente).</summary>
    string Nome { get; }

    /// <summary>Devolve o desconto que ESTA política concederia ao pedido (Zero se não se aplica).</summary>
    Dinheiro Calcular(Pedido pedido);
}

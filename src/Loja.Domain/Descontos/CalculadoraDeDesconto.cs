using Loja.Domain.Common;
using Loja.Domain.Pedidos;

namespace Loja.Domain.Descontos;

/// <summary>
/// DOMAIN SERVICE que escolhe, entre todas as políticas registradas, a que dá o MAIOR desconto
/// (as promoções não se acumulam - regra de negócio de exemplo).
///
/// POR QUE existe (e não colocamos isso dentro do Pedido)?
/// - A regra "qual promoção vale" envolve várias políticas, não apenas um pedido. Não pertence naturalmente a
///   nenhuma entidade, então vira um serviço de domínio.
///
/// POR QUE recebe <c>IEnumerable&lt;IPoliticaDesconto&gt;</c> no construtor?
/// - INJEÇÃO DE DEPENDÊNCIA + DIP: a calculadora depende da ABSTRAÇÃO, nunca de "new DescontoClienteVip()".
/// - O contêiner de DI entrega todas as políticas registradas. Nova promoção = registrar uma classe nova (OCP);
///   esta calculadora não precisa ser editada.
/// </summary>
public sealed class CalculadoraDeDesconto
{
    private readonly IReadOnlyCollection<IPoliticaDesconto> _politicas;

    public CalculadoraDeDesconto(IEnumerable<IPoliticaDesconto> politicas)
    {
        _politicas = politicas.ToList();
    }

    public Dinheiro CalcularMelhorDesconto(Pedido pedido)
    {
        var melhor = Dinheiro.Zero;

        foreach (var politica in _politicas)
        {
            var desconto = politica.Calcular(pedido);
            if (desconto > melhor)
                melhor = desconto;
        }

        return melhor;
    }
}

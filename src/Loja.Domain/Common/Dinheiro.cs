namespace Loja.Domain.Common;

/// <summary>
/// VALUE OBJECT que representa uma quantia em dinheiro.
///
/// POR QUE não usar simplesmente <c>decimal</c>? (combate ao code smell "Primitive Obsession")
/// - Um <c>decimal</c> aceita valor negativo, aceita 10.123456 centavos, e não diz nada sobre o que representa.
///   Aqui as regras (não negativo, 2 casas decimais, arredondamento comercial) vivem em UM só lugar.
/// - É IMUTÁVEL: toda operação devolve um novo objeto. Objetos imutáveis não têm efeitos colaterais
///   e são seguros para usar em qualquer lugar (inclusive com várias threads).
/// - Igualdade por valor: <c>Dinheiro.De(10) == Dinheiro.De(10)</c> é verdadeiro (o record cuida disso).
///
/// POR QUE construtor privado + método de fábrica <see cref="De"/>?
/// - Garante que NÃO existe um Dinheiro inválido em memória: ou é criado válido, ou lança exceção.
/// </summary>
public sealed record Dinheiro
{
    public decimal Valor { get; }

    private Dinheiro(decimal valor) =>
        // Arredondamento comercial ("meio para cima"), o esperado em valores monetários no Brasil.
        Valor = Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    public static Dinheiro Zero { get; } = new(0m);

    public static Dinheiro De(decimal valor)
    {
        if (valor < 0)
            throw new DominioException("Um valor monetário não pode ser negativo.");

        return new Dinheiro(valor);
    }

    public Dinheiro Multiplicar(int quantidade) => De(Valor * quantidade);

    /// <summary>Calcula um percentual deste valor. Ex.: <c>De(200).Percentual(10)</c> = 20.</summary>
    public Dinheiro Percentual(decimal percentual) => De(Valor * percentual / 100m);

    public static Dinheiro operator +(Dinheiro a, Dinheiro b) => De(a.Valor + b.Valor);

    public static Dinheiro operator -(Dinheiro a, Dinheiro b) => De(a.Valor - b.Valor);

    public static bool operator >(Dinheiro a, Dinheiro b) => a.Valor > b.Valor;

    public static bool operator <(Dinheiro a, Dinheiro b) => a.Valor < b.Valor;

    public override string ToString() => Valor.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
}

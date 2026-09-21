namespace Loja.Domain.Common;

/// <summary>
/// Classe base das entidades: objetos com IDENTIDADE (dois pedidos com os mesmos itens continuam sendo pedidos diferentes).
///
/// POR QUE existe?
/// - Centraliza a regra "igualdade por Id" em um único lugar (DRY), em vez de repeti-la em cada entidade.
/// - Contrasta com Value Objects (ex.: <c>Dinheiro</c>), que são iguais quando seus VALORES são iguais.
///
/// POR QUE o setter de Id é protegido?
/// - Encapsulamento: ninguém de fora troca a identidade de uma entidade depois de criada.
/// </summary>
public abstract class Entidade
{
    public Guid Id { get; protected set; }

    protected Entidade(Guid id)
    {
        if (id == Guid.Empty)
            throw new DominioException("O identificador da entidade não pode ser vazio.");

        Id = id;
    }

    public override bool Equals(object? obj) =>
        obj is Entidade outra && GetType() == outra.GetType() && Id == outra.Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}

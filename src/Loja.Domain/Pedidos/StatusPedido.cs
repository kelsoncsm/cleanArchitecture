namespace Loja.Domain.Pedidos;

/// <summary>
/// Ciclo de vida de um pedido: Aberto -> Confirmado ou Aberto -> Cancelado.
///
/// POR QUE um enum e não uma string ("aberto")?
/// - O compilador impede valores inválidos (não existe "abrto" por erro de digitação).
/// - Facilita achar todos os usos e refatorar.
/// </summary>
public enum StatusPedido
{
    Aberto = 1,
    Confirmado = 2,
    Cancelado = 3
}

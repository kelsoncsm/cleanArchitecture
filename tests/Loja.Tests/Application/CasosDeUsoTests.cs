using Loja.Application;
using Loja.Application.Common;
using Loja.Application.Pedidos;
using Loja.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Loja.Tests.Application;

/// <summary>
/// Testes dos casos de uso "de ponta a ponta" dentro do processo: usamos o MESMO registro de DI da aplicação real
/// (AddApplication + AddInfrastructure), então testamos também a montagem dos decorators.
///
/// POR QUE não usar um framework de mock (Moq/NSubstitute)?
/// - Como o repositório em memória é uma implementação real, simples e rápida da interface, ela serve de "fake"
///   sem dependência extra. Menos mágica, testes mais legíveis.
/// </summary>
public class CasosDeUsoTests
{
    private readonly ServiceProvider _provider;

    public CasosDeUsoTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication().AddInfrastructure();
        _provider = services.BuildServiceProvider();
    }

    private ICasoDeUso<TRequest, TResponse> Caso<TRequest, TResponse>() =>
        _provider.GetRequiredService<ICasoDeUso<TRequest, TResponse>>();

    private static CriarPedidoRequest PedidoValido(bool vip = false, int quantidade = 1) =>
        new("Maria", vip, new[] { new ItemPedidoRequest("Teclado", 100m, quantidade) });

    [Fact]
    public async Task Criar_ComDadosValidos_RetornaPedidoAberto()
    {
        var resultado = await Caso<CriarPedidoRequest, PedidoResponse>().ExecutarAsync(PedidoValido(), default);

        Assert.True(resultado.EhSucesso);
        Assert.Equal("Aberto", resultado.Valor.Status);
        Assert.Equal(100m, resultado.Valor.Total);
    }

    [Fact]
    public async Task Criar_SemItens_RetornaErroDeValidacao()
    {
        var request = new CriarPedidoRequest("Maria", false, Array.Empty<ItemPedidoRequest>());

        var resultado = await Caso<CriarPedidoRequest, PedidoResponse>().ExecutarAsync(request, default);

        Assert.False(resultado.EhSucesso);
        Assert.Equal(TipoErro.Validacao, resultado.Erro.Tipo);
    }

    [Fact]
    public async Task Criar_ComQuantidadeZero_ConverteExcecaoDeDominioEmRegraDeNegocio()
    {
        // A DominioException lançada no Domain é convertida pelo decorator em Result de falha.
        var request = new CriarPedidoRequest("Maria", false, new[] { new ItemPedidoRequest("Cabo", 10m, 0) });

        var resultado = await Caso<CriarPedidoRequest, PedidoResponse>().ExecutarAsync(request, default);

        Assert.False(resultado.EhSucesso);
        Assert.Equal(TipoErro.RegraDeNegocio, resultado.Erro.Tipo);
    }

    [Fact]
    public async Task Confirmar_ClienteVip_AplicaDescontoDeDezPorCento()
    {
        var criado = await Caso<CriarPedidoRequest, PedidoResponse>().ExecutarAsync(PedidoValido(vip: true), default);

        var confirmado = await Caso<ConfirmarPedidoRequest, PedidoResponse>()
            .ExecutarAsync(new ConfirmarPedidoRequest(criado.Valor.Id), default);

        Assert.True(confirmado.EhSucesso);
        Assert.Equal("Confirmado", confirmado.Valor.Status);
        Assert.Equal(10m, confirmado.Valor.Desconto);
        Assert.Equal(90m, confirmado.Valor.Total);
    }

    [Fact]
    public async Task Confirmar_DuasVezes_RetornaRegraDeNegocio()
    {
        var criado = await Caso<CriarPedidoRequest, PedidoResponse>().ExecutarAsync(PedidoValido(), default);
        var confirmar = Caso<ConfirmarPedidoRequest, PedidoResponse>();

        await confirmar.ExecutarAsync(new ConfirmarPedidoRequest(criado.Valor.Id), default);
        var segunda = await confirmar.ExecutarAsync(new ConfirmarPedidoRequest(criado.Valor.Id), default);

        Assert.False(segunda.EhSucesso);
        Assert.Equal(TipoErro.RegraDeNegocio, segunda.Erro.Tipo);
    }

    [Fact]
    public async Task Obter_PedidoInexistente_RetornaNaoEncontrado()
    {
        var resultado = await Caso<ObterPedidoRequest, PedidoResponse>()
            .ExecutarAsync(new ObterPedidoRequest(Guid.NewGuid()), default);

        Assert.False(resultado.EhSucesso);
        Assert.Equal(TipoErro.NaoEncontrado, resultado.Erro.Tipo);
    }

    [Fact]
    public async Task Listar_RetornaPedidosCriados()
    {
        await Caso<CriarPedidoRequest, PedidoResponse>().ExecutarAsync(PedidoValido(), default);
        await Caso<CriarPedidoRequest, PedidoResponse>().ExecutarAsync(PedidoValido(), default);

        var resultado = await Caso<ListarPedidosRequest, IReadOnlyList<PedidoResponse>>()
            .ExecutarAsync(new ListarPedidosRequest(), default);

        Assert.Equal(2, resultado.Valor.Count);
    }
}

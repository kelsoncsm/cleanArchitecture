using Loja.Domain.Pedidos;
using Loja.Infrastructure.Persistencia;
using Microsoft.Extensions.DependencyInjection;

namespace Loja.Infrastructure;

/// <summary>
/// Registra as implementações concretas das abstrações. É aqui (e só aqui) que se decide "qual banco usar".
/// Para migrar para SQL Server, troque a linha do repositório por uma implementação com EF Core.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Singleton: o "banco" em memória precisa viver enquanto a aplicação vive.
        // Com EF Core seria AddScoped (um DbContext por requisição).
        services.AddSingleton<IPedidoRepository, PedidoRepositoryEmMemoria>();

        // Relógio do sistema como dependência injetável (permite trocar por um relógio falso nos testes).
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}

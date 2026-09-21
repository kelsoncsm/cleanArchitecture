# Loja — Arquitetura simples em C# / .NET 8 (Clean Architecture, SOLID e Design Patterns)

Projeto didático de um mini sistema de **pedidos** (criar, listar, obter e confirmar com desconto).
O domínio é pequeno de propósito: o foco é a **estrutura** e o **porquê de cada decisão**.

> Cada arquivo `.cs` e `.csproj` tem comentários explicando **por que** aquilo existe e qual princípio/padrão está sendo aplicado.
> Este README dá a visão geral; os detalhes estão em [`docs/`](docs/).

## Como executar

```bash
dotnet test                              # 24 testes (domínio + casos de uso)
dotnet run --project src/Loja.Api        # sobe a API em http://localhost:5000
```

Depois use o arquivo [`src/Loja.Api/requests.http`](src/Loja.Api/requests.http) (VS Code REST Client, Visual Studio ou Rider) ou o `curl`:

```bash
curl -X POST http://localhost:5000/api/pedidos -H "Content-Type: application/json" \
  -d '{"cliente":"Maria","clienteVip":true,"itens":[{"produto":"Teclado","precoUnitario":150,"quantidade":2}]}'
```

Requisitos: SDK do .NET 8 (o código também roda em .NET 6+ trocando o `TargetFramework` em `Directory.Build.props`, exceto o uso de `TimeProvider`, que é do .NET 8).

## Estrutura

```
Loja.sln
Directory.Build.props            <- regras de qualidade compartilhadas (nullable, warnings como erro)
src/
  Loja.Domain/                   <- NÚCLEO: regras de negócio. Não depende de NADA.
    Common/                        Entidade, Dinheiro (Value Object), DominioException
    Pedidos/                       Pedido (Aggregate Root), ItemPedido, StatusPedido, IPedidoRepository
    Descontos/                     IPoliticaDesconto (Strategy) + políticas + CalculadoraDeDesconto
  Loja.Application/              <- CASOS DE USO: orquestra o Domain. Depende só do Domain.
    Common/                        Result<T>, ICasoDeUso<TReq,TRes>
    Pedidos/                       Criar/Confirmar/Obter/Listar + DTOs + Mapper
    Decorators/                    Log e tratamento de exceção (Decorator)
    DependencyInjection.cs         registra tudo (AddApplication)
  Loja.Infrastructure/           <- DETALHES TÉCNICOS: banco, relógio... Implementa as interfaces.
    Persistencia/                  PedidoRepositoryEmMemoria (trocável por EF Core)
  Loja.Api/                      <- PORTA DE ENTRADA HTTP + Composition Root (Program.cs)
    Endpoints/                     PedidosEndpoints, ResultHttpExtensions
tests/
  Loja.Tests/                    <- testes de Domain e de casos de uso
docs/
  ARQUITETURA.md  SOLID.md  DESIGN-PATTERNS.md  COMO-EVOLUIR.md
```

## A regra que sustenta tudo: a Regra da Dependência

As dependências (referências entre projetos) apontam **sempre para dentro**, em direção ao Domain:

```
   Api ────────────► Application ────► Domain ◄──── Infrastructure
    │                                                     ▲
    └──────────── (Composition Root liga tudo) ───────────┘
```

- O **Domain** não conhece ninguém: é o ativo mais valioso e não pode quebrar por causa de um framework ou banco.
- A **Application** conhece o Domain e define *o que* o sistema faz (casos de uso).
- A **Infrastructure** e a **Api** são detalhes: podem ser trocadas sem tocar nas regras de negócio.
- Quando o Domain precisa de algo externo (guardar pedidos), ele declara uma **interface** (`IPedidoRepository`) e a Infrastructure a implementa. Isso é a **Inversão de Dependência**.

Essa regra é verificada pelo próprio compilador: se alguém tentar usar `Loja.Infrastructure` dentro do Domain, o projeto não compila (não há `ProjectReference`).

## O caminho de uma requisição

`POST /api/pedidos/{id}/confirmar`

1. **Api** (`PedidosEndpoints`) recebe o HTTP e chama `ICasoDeUso<ConfirmarPedidoRequest, PedidoResponse>`.
2. O objeto recebido é o caso de uso **embrulhado** por decorators: `Log( ExcecaoDeDominio( ConfirmarPedidoUseCase ) )`.
3. **Application** (`ConfirmarPedidoUseCase`) busca o pedido no `IPedidoRepository`, pede à `CalculadoraDeDesconto` o melhor desconto (Strategy) e chama `pedido.Confirmar(desconto)`.
4. **Domain** (`Pedido`) valida as regras (não vazio, não confirmado, desconto ≤ subtotal) e muda de estado.
5. O resultado volta como `Result<PedidoResponse>`; a **Api** traduz para HTTP (200, 404, 422...) em `ResultHttpExtensions`.

## Resumo: onde está cada princípio e padrão

| Conceito | Onde ver | Por quê |
|---|---|---|
| **S** — Responsabilidade única | Um caso de uso por classe; cada política de desconto separada | Um único motivo para mudar |
| **O** — Aberto/Fechado | `IPoliticaDesconto`, decorators | Novas promoções/comportamentos sem editar código existente |
| **L** — Substituição de Liskov | `PedidoRepositoryEmMemoria` ↔ futura versão EF | Qualquer implementação vale no lugar da outra |
| **I** — Segregação de interfaces | `IPedidoRepository` enxuto, `ICasoDeUso` com 1 método | Ninguém depende do que não usa |
| **D** — Inversão de dependência | Interfaces no Domain, implementações na Infrastructure, DI no `Program.cs` | O núcleo não depende de detalhes |
| Strategy | `Descontos/` | Regras de desconto intercambiáveis |
| Repository | `IPedidoRepository` | Domínio ignora o banco |
| Factory Method | `Pedido.Criar`, `Dinheiro.De` | Objetos nascem sempre válidos |
| Decorator | `Decorators/` | Log e tratamento de erro sem poluir os casos de uso |
| Result | `Result<T>` | Falhas esperadas explícitas, sem exceções para controle de fluxo |
| Value Object | `Dinheiro` | Evita "obsessão por primitivos" |
| Aggregate Root | `Pedido` | Um único ponto que protege as regras |
| Dependency Injection | `DependencyInjection.cs` + `Program.cs` | Baixo acoplamento e testabilidade |

Veja o detalhamento em [`docs/SOLID.md`](docs/SOLID.md) e [`docs/DESIGN-PATTERNS.md`](docs/DESIGN-PATTERNS.md).

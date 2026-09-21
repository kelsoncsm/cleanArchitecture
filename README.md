# Loja — Clean Architecture (.NET 8)

Projeto didático de um mini sistema de pedidos, feito pra mostrar a estrutura de uma Clean Architecture simples em C#/.NET. Detalhes e explicações mais a fundo em [`Detalhamento.md`](Detalhamento.md) e em [`docs/`](docs/).

## Servidor de desenvolvimento

Rode `dotnet run --project src/Loja.Api` para subir a API. Ela fica disponível em `http://localhost:5000`.

## Build

Rode `dotnet build` para compilar a solução inteira.

## Testes

Rode `dotnet test` para rodar os testes unitários (xUnit).

## Estrutura / camadas

O projeto é dividido em camadas, cada uma num projeto próprio:

- **Loja.Domain** — o núcleo: entidades e regras de negócio. Não depende de mais nada.
- **Loja.Application** — os casos de uso (criar pedido, confirmar, etc). Depende só do Domain.
- **Loja.Infrastructure** — detalhes técnicos, como o repositório. Implementa o que o Domain pede.
- **Loja.Api** — a porta de entrada HTTP (Minimal API), onde tudo é ligado (`Program.cs`).
- **tests/Loja.Tests** — testes de Domain e Application.

As dependências sempre apontam pra dentro, em direção ao Domain.

## Swagger

Com a API rodando (`http://localhost:5000`), a documentação interativa fica em `http://localhost:5000/swagger`.

using Loja.Api.Endpoints;
using Loja.Application;
using Loja.Infrastructure;

// ---------------------------------------------------------------------------------------------------------
// COMPOSITION ROOT: o único ponto do sistema que conhece TODAS as camadas e "monta o quebra-cabeça".
//
// POR QUE centralizar a montagem aqui?
// - Todas as outras classes só declaram o que PRECISAM (construtor) e nunca criam suas dependências com `new`.
//   Isso é Inversão de Dependência + Injeção de Dependência: quem usa não sabe (nem precisa saber) quem implementa.
// - Trocar uma peça (banco, relógio, política de desconto) = mudar UMA linha aqui.
// ---------------------------------------------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()      // casos de uso, políticas de desconto, decorators
    .AddInfrastructure();  // repositório, relógio

// Habilita o formato padrão ProblemDetails (RFC 7807) também para erros inesperados (HTTP 500),
// para NUNCA devolver stack trace ao cliente (segurança: não vazar detalhes internos).
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Handler global: exceções inesperadas (bugs, banco fora do ar) viram um 500 padronizado, em um único lugar.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPedidos();

app.Run();

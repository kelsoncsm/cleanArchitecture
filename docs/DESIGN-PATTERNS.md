# Design Patterns usados (e por quê)

Regra de bolso adotada: **só usar um padrão quando ele resolve um problema real.** Padrão sem problema é over-engineering.
Cada padrão abaixo responde a uma dor concreta do projeto.

## 1. Strategy — `Domain/Descontos/`
**Problema:** regras de desconto mudam o tempo todo e não devem virar um `if/else` gigante.
**Solução:** uma interface (`IPoliticaDesconto`) e uma classe por regra. A `CalculadoraDeDesconto` recebe todas por injeção e escolhe a melhor.
**Benefício:** nova promoção = classe nova + 1 linha de registro (Open/Closed). Cada regra é testada isoladamente.

```csharp
public interface IPoliticaDesconto { string Nome { get; } Dinheiro Calcular(Pedido pedido); }
```

## 2. Repository — `IPedidoRepository` (Domain) / `PedidoRepositoryEmMemoria` (Infrastructure)
**Problema:** o negócio não deve saber se os dados estão em SQL, Mongo ou memória.
**Solução:** uma interface que se comporta como "uma coleção de pedidos", declarada no Domain e implementada fora.
**Benefício:** trocar o banco não afeta as regras; testes sem infraestrutura.

## 3. Factory Method — `Pedido.Criar(...)`, `Dinheiro.De(...)`
**Problema:** um construtor público permite criar objetos inválidos e não expressa intenção.
**Solução:** construtor privado + método estático nomeado que valida. O objeto **nasce sempre válido**.
**Benefício:** não existe `Pedido` sem cliente nem `Dinheiro` negativo em memória.

## 4. Decorator — `Application/Decorators/`
**Problema:** log e tratamento de exceção são necessários em *todo* caso de uso, mas não fazem parte de nenhum deles.
**Solução:** classes que implementam a mesma interface (`ICasoDeUso`), recebem outro caso de uso e acrescentam comportamento antes/depois.
**Benefício:** código de negócio limpo; comportamento transversal escrito uma vez. A composição fica em `AddCasoDeUso`:

```
Log( ExcecaoDeDominio( CriarPedidoUseCase ) )
```

A ordem importa: o log fica por fora para registrar o `Result` final.

## 5. Result (Railway-Oriented / Either) — `Application/Common/Result.cs`
**Problema:** usar exceções para "não encontrado" ou "regra violada" esconde o fluxo e custa caro.
**Solução:** o retorno é `Sucesso(valor)` ou `Falha(erro)`, explícito na assinatura.
**Benefício:** quem chama é *obrigado* a considerar a falha; a API mapeia cada tipo de erro para o status HTTP correto.

## 6. Value Object — `Dinheiro`
**Problema:** *Primitive Obsession* (`decimal` representando dinheiro).
**Solução:** tipo imutável, com invariantes e igualdade por valor (`record`).

## 7. Aggregate Root + Entity — `Pedido` / `ItemPedido` / `Entidade`
**Problema:** garantir que o pedido e seus itens nunca fiquem inconsistentes (ex.: total errado, item alterado por fora).
**Solução:** apenas o `Pedido` expõe operações; `ItemPedido` tem construtor `internal`; a coleção é somente leitura.

## 8. Dependency Injection + Composition Root — `DependencyInjection.cs` / `Program.cs`
**Problema:** classes que criam suas dependências com `new` ficam acopladas e impossíveis de testar isoladamente.
**Solução:** dependências pelo construtor; o contêiner de DI do .NET as fornece; a montagem ocorre em um só lugar.

## 9. DTO + Mapper — `PedidoDtos.cs`, `PedidoMapper.cs`
**Problema:** expor a entidade acopla o contrato público ao modelo interno.
**Solução:** objetos de transferência estáveis e uma conversão explícita.

## 10. Domain Service — `CalculadoraDeDesconto`
**Problema:** uma regra que envolve várias políticas não pertence naturalmente a uma entidade.
**Solução:** um serviço de domínio sem estado, que coordena as estratégias.

---

## Padrões que **NÃO** usamos (e por quê)

| Padrão | Por que ficou de fora | Quando faria sentido |
|---|---|---|
| Unit of Work | Não há várias operações de escrita que precisem ser atômicas | Ao usar EF Core com transações entre agregados |
| Mediator (MediatR) | Nossa `ICasoDeUso` já entrega o benefício, sem dependência extra | Pipelines complexos, notificações, muitos handlers |
| Generic Repository | Vira interface "gorda" e vaza detalhes de consulta | Raramente; prefira repositórios específicos por agregado |
| Abstract Factory / Builder | Nenhum objeto tem criação complexa o bastante | Objetos com dezenas de parâmetros opcionais |
| Singleton "manual" | O contêiner de DI já gerencia tempo de vida | Nunca escreva o seu; use `AddSingleton` |

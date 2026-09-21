# Arquitetura: as decisões e os porquês

## 1. Por que camadas?

Um sistema costuma sofrer com **acoplamento**: a regra de negócio misturada com SQL, JSON e HTTP. Quando isso acontece,
trocar o banco exige mexer nas regras, testar uma regra exige subir o banco e o servidor, e o código vira um novelo.

A solução aqui é separar por **motivo de mudança**:

| Camada | Muda quando... | Conhece |
|---|---|---|
| Domain | a regra do negócio muda | ninguém |
| Application | um caso de uso muda ("agora confirmar também envia e-mail") | Domain |
| Infrastructure | trocamos tecnologia (SQL → Mongo) | Domain, Application |
| Api | mudamos o protocolo (REST → gRPC) | Application, Infrastructure (só no Composition Root) |

Quem muda com mais frequência (tecnologia) fica **fora**; quem muda menos e vale mais (regra de negócio) fica **dentro** e protegido.

## 2. Decisões e justificativas

### Domain rico, e não "anêmico"
`Pedido` tem comportamento (`Confirmar`, `Cancelar`, `AdicionarItem`) e protege os próprios dados (`private set`, lista somente leitura).
No modelo anêmico, a entidade é só um saco de propriedades e a lógica vaza para *services* — as regras se espalham e se duplicam.
Aqui vale o princípio **"Tell, Don't Ask"**: peça ao objeto que faça, não pergunte o estado para decidir por ele.

### Value Object `Dinheiro`
Usar `decimal` puro permite valor negativo, três casas decimais e mistura acidental com outros números.
`Dinheiro` é imutável, sempre válido e arredonda em um lugar só. É o antídoto para a *Primitive Obsession*.

### Interface do repositório no Domain
O negócio declara a **necessidade** ("preciso guardar pedidos"); a Infrastructure fornece o **como**.
Se a interface ficasse na Infrastructure, o Domain teria de referenciá-la e a dependência apontaria para fora, quebrando a arquitetura.

### Um caso de uso por classe (`ICasoDeUso<TReq,TRes>`)
- Nome autoexplicativo: a pasta `Pedidos/` lista o que o sistema faz.
- Classes pequenas, testáveis, com poucas dependências.
- A interface única permite aplicar **decorators** de forma genérica.

### `Result<T>` em vez de exceções para o fluxo normal
"Pedido não encontrado" não é excepcional — é um resultado previsível. Exceções ficam para o inesperado (banco caiu).
O `Result` torna a possibilidade de falha **visível na assinatura** do método.

Como as regras do Domain lançam `DominioException` (é a forma mais natural de proteger invariantes dentro de uma entidade),
o `ExcecaoDeDominioDecorator` converte essa exceção em `Result` na borda da Application, uma vez só.

### DTOs separados das entidades
O contrato da API (JSON) precisa ser estável; o modelo de domínio precisa ser livre para evoluir.
Também evita *over-posting* (o cliente enviar `status` ou `desconto` no JSON).

### A validação mora no Domain
`Pedido`, `ItemPedido` e `Dinheiro` validam suas próprias regras. A Application valida só o que é formato da requisição.
Assim há **uma fonte da verdade**: a regra vale para API, fila, job ou teste.

### Tradução Result → HTTP na Api
Códigos HTTP são um detalhe de transporte. A Application não deve saber que HTTP existe.
Mapeamento adotado: `Validacao → 400`, `NaoEncontrado → 404`, `RegraDeNegocio → 422`, exceção inesperada → `500` (sem stack trace, com `ProblemDetails`).

### Composition Root
Só o `Program.cs` conhece todas as camadas e monta os objetos. O resto recebe dependências pelo construtor.
Trocar qualquer peça é mudar **uma linha** no `AddInfrastructure`/`AddApplication`.

### Infraestrutura em memória
Para não exigir banco instalado. Não é uma limitação da arquitetura: é a demonstração de que ela funciona — veja
[`COMO-EVOLUIR.md`](COMO-EVOLUIR.md) para trocar por EF Core sem alterar Domain nem Application.

## 3. Testabilidade como consequência

| Tipo de teste | O que exercita | Precisa de banco/HTTP? |
|---|---|---|
| `Domain/*Tests` | regras de negócio puras | Não |
| `Application/CasosDeUsoTests` | casos de uso + decorators + DI reais | Não (repositório em memória) |

Testes rápidos (24 em ~0,4 s) são o sinal de que o acoplamento está sob controle.

## 4. O que ficou de fora de propósito (YAGNI)

Para manter simples, **não** incluímos: Unit of Work, MediatR, AutoMapper, FluentValidation, CQRS com bancos separados,
Event Sourcing, autenticação. Todos são úteis em cenários específicos; adicioná-los sem necessidade só aumenta a complexidade.
A estrutura aceita cada um deles quando o problema real aparecer.

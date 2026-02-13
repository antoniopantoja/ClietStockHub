# ClietStockHub

Aplicação web de **Catálogo e Pedidos** com frontend em Angular 17+ e backend em .NET 8, utilizando PostgreSQL.

## Objetivo

O sistema deve permitir:
- Gestão de produtos e clientes (CRUD)
- Criação de pedidos com itens
- Validação de estoque
- Idempotência na criação de pedidos

## Escopo Core

### Backend (.NET 8, ASP.NET Core Web API)

- Modelagem em PostgreSQL:
	- `products(id, name, sku, price, stock_qty, is_active, created_at)`
	- `customers(id, name, email, document, created_at)`
	- `orders(id, customer_id, total_amount, status, created_at)`
	- `order_items(id, order_id, product_id, unit_price, quantity, line_total)`
- CRUD de `products` e `customers` via EF Core
- Criação de pedido com transação atômica, validação de estoque e idempotência por `Idempotency-Key` no header
- Leituras otimizadas com Dapper para listagens
- Observabilidade mínima com logs estruturados
- Testes unitários para regras de negócio
- Aplicação de princípios SOLID e separação por Clean Architecture

### Frontend (Angular 17+)

- Listas com paginação, filtros e ordenação para `products` e `customers`
- Formulários reativos com validações sincrônicas e assíncronas quando aplicável
- Tela de criação de pedidos com busca de produtos (typeahead), cálculo de totais e validação de estoque
- Tratamento global de erros via interceptor, respeitando o envelope da API
- Acessibilidade básica: semântica, navegação por teclado e ARIA quando aplicável

### Infra local

- Dockerfiles para frontend e backend
- `docker-compose` para subir aplicação + PostgreSQL
- Seed mínimo de dados (ex.: 20 produtos, 10 clientes)

## Contrato de API (Envelope)

Todas as respostas da API devem seguir o formato:

```json
{
	"cod_retorno": 0,
	"mensagem": null,
	"data": {}
}
```

Regras do envelope:
- `cod_retorno`: `0` para sucesso, `1` para erro
- `mensagem`: opcional, descreve erros ou avisos
- `data`: payload da operação

Exemplo de erro:

```json
{
	"cod_retorno": 1,
	"mensagem": "Estoque insuficiente para o produto SKU-123",
	"data": null
}
```

Regra de domínio:
- `orders.status`: `CREATED`, `PAID`, `CANCELLED`

## Requisitos técnicos

- .NET 8 (ASP.NET Core Web API), EF Core e Dapper
- Angular 17+ (TypeScript) com RxJS idiomático
- PostgreSQL
- Logs estruturados (ex.: Serilog)
- Docker e Docker Compose para execução local

## Estrutura do repositório

```text
.
├─ backend/
│  ├─ src/ (Api, Application, Domain, Infrastructure)
│  ├─ tests/
│  ├─ Dockerfile
│  └─ README-backend.md (opcional)
├─ frontend/
│  ├─ src/
│  ├─ e2e/
│  ├─ Dockerfile
│  └─ README-frontend.md (opcional)
├─ docker-compose.yml
├─ .env.example
└─ README.md
```

## Decisões técnicas

- **Arquitetura backend:** Clean Architecture com separação em `Api`, `Application`, `Domain` e `Infrastructure`
- **Persistência:** EF Core para operações transacionais e Dapper para leituras otimizadas
- **Banco de dados:** PostgreSQL como banco relacional principal
- **Observabilidade:** logs estruturados para rastreabilidade de operações
- **Resiliência de escrita:** idempotência por header `Idempotency-Key` na criação de pedidos
- **Frontend:** Angular com Reactive Forms, RxJS e interceptor global de erros

## Execução local

### Pré-requisitos

- Docker
- Docker Compose

### Passos

1. Criar arquivo de ambiente:

```bash
cp .env.example .env
```

> No Windows PowerShell, use:

```powershell
Copy-Item .env.example .env
```

2. Subir os serviços:

```bash
docker compose up -d --build
```

3. Acompanhar logs (opcional):

```bash
docker compose logs -f
```

4. Derrubar ambiente:

```bash
docker compose down
```


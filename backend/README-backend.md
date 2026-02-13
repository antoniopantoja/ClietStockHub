# Backend — ClietStockHub

Este documento descreve a **estrutura base da API** e os **padrões adotados** no backend.

## Stack

- .NET 8
- ASP.NET Core Web API
- EF Core + Dapper (planejado para persistência)
- PostgreSQL (planejado)
- Serilog (logging estruturado)

## Estrutura base

```text
backend/
├─ src/
│  ├─ Api/
│  │  ├─ Common/ (contratos compartilhados da API, ex.: envelope)
│  │  ├─ Controllers/ (endpoints HTTP)
│  │  ├─ Extensions/ (composição da aplicação: DI + pipeline)
│  │  ├─ Middleware/ (tratamento global de exceções)
│  │  ├─ Program.cs (bootstrap mínimo)
│  │  └─ appsettings*.json (configuração de ambiente/logging)
│  ├─ Application/ (casos de uso, DTOs, contratos)
│  ├─ Domain/ (entidades e regras de negócio)
│  └─ Infrastructure/ (persistência, integrações externas)
└─ tests/ (testes unitários e integração)
```

## Arquitetura e organização

- **Estilo arquitetural:** Clean Architecture com separação por responsabilidades.
- **Direção de dependências:**
	- `Api -> Application, Infrastructure`
	- `Infrastructure -> Application, Domain`
	- `Application -> Domain`
	- `Domain` sem dependência de outras camadas.
- **Bootstrap enxuto:** `Program.cs` delega configuração para extensões da camada `Api`.

## Padrões adotados

### 1) Registro de dependências via extensões

- Configuração centralizada em métodos de extensão:
	- `AddStructuredLogging()`
	- `AddApiServices(configuration)`
	- `UseApiPipeline()`
- Objetivo: manter `Program.cs` simples, legível e padronizado.

### 2) Envelope padrão de respostas

Todas as respostas seguem o contrato:

```json
{
	"cod_retorno": 0,
	"mensagem": null,
	"data": {}
}
```

Regras:
- `cod_retorno`: `0` sucesso, `1` erro
- `mensagem`: detalhe opcional
- `data`: payload da operação

### 3) Middleware global de exceções

- Exceções não tratadas são capturadas em middleware único.
- Erros são logados com contexto (`TraceId`, `Path`, `StatusCode`).
- Resposta de erro é padronizada no envelope da API.

### 4) Logging estruturado (Serilog)

- Serilog configurado no host da API.
- Logging de requisições HTTP habilitado (`UseSerilogRequestLogging`).
- Saída estruturada no console com propriedades de contexto.

### 5) Swagger/OpenAPI

- Swagger habilitado em ambiente de desenvolvimento.
- Documento `v1` com título/descrição da API.
- Comentários XML habilitados para enriquecer a documentação dos endpoints.
- URL local (profile `http`): `http://localhost:5167/swagger`

## Endpoints iniciais

- `GET /health`
- `GET /api/health`
- `GET /` (status básico da aplicação)

## Convenções para próximas entregas

- Controllers devem apenas orquestrar request/response.
- Regras de negócio devem ficar em `Application`/`Domain`.
- Infraestrutura (EF/Dapper, repositórios, integrações) deve ficar em `Infrastructure`.
- Toda nova resposta pública deve respeitar o envelope padrão.
- Toda exceção não tratada deve passar pelo middleware global.


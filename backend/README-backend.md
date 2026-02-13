# Backend — ClietStockHub

Estrutura inicial baseada em Clean Architecture:

- `src/Api`: ASP.NET Core Web API (controllers/endpoints, DI, middleware, Serilog)
- `src/Application`: casos de uso, DTOs, validações, contratos
- `src/Domain`: entidades, value objects, regras de negócio
- `src/Infrastructure`: persistência (EF Core + Dapper), repositórios, integrações
- `tests`: testes unitários e integração

## Stack planejada
- .NET 8
- ASP.NET Core Web API
- EF Core + Dapper
- PostgreSQL
- Serilog

## Swagger e documentação inicial

- Swagger/OpenAPI habilitado na API (`Swashbuckle.AspNetCore`)
- Documento: `v1`
- URL local (profile `http`): `http://localhost:5167/swagger`

### Endpoints iniciais

- `GET /health`
- `GET /api/health`

### Envelope de resposta

Todos os endpoints retornam o envelope padrão:

```json
{
	"cod_retorno": 0,
	"mensagem": null,
	"data": {}
}
```


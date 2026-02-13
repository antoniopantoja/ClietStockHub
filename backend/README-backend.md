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


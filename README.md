# CustomerCreditClassifier

## Tecnologias Utilizadas

- .NET 8
- ASP.NET Core
- MassTransit (com suporte a Kafka e EntityFramework)
- Entity Framework Core (PostgreSQL)
- PostgreSQL
- Kafka
- Docker (para orquestração de ambientes)
- xUnit, Moq, FluentAssertions, Bogus (para testes)

## Estrutura do Projeto

```
.
├── .gitignore
├── CustomerCreditClassifier.sln
├── global.json
├── README.md
├── Files/
│   └── docker-compose.yaml
├── src/
│   ├── CustomerCreditClassifier.Api/
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.json
│   │   ├── CustomerCreditClassifier.Api.csproj
│   │   ├── CustomerCreditClassifier.Api.http
│   │   └── ...
│   ├── CustomerCreditClassifier.Application/
│   │   └── ...
│   ├── CustomerCreditClassifier.Domain/
│   │   ├── Events/
│   │   └── CustomerCreditClassifier.Domain.csproj
│   └── CustomerCreditClassifier.Infrastructure.MassTransit/
│       ├── CustomerCreditClassifier.Infrastructure.MassTransit.csproj
│       ├── Extensions/
│       ├── Mappings/
│       ├── Migrations/
│       ├── StateMachines/
│       └── Data/
├── tests/
│   ├── CustomerCreditClassifier.Application.Tests/
│   │   ├── CustomerCreditClassifier.Application.Tests.csproj
│   │   └── UnitTest1.cs
│   └── CustomerCreditClassifier.Infrastructure.MassTransit.Tests/
│       ├── CustomerCreditClassifier.Infrastructure.MassTransit.Tests.csproj
│       ├── StateMachines/
│       │   └── PreviousServiceStateMachineTests.cs
│       └── UnitTest1.cs
```

## Como executar

1. Configure o ambiente utilizando o Docker Compose:
   ```sh
   docker compose -f Files/docker-compose.yaml up -d
   ```

2. Ajuste as strings de conexão em `src/CustomerCreditClassifier.Api/appsettings.Development.json` se necessário.

3. Execute as migrations do Entity Framework para criar o banco de dados.

4. Rode a aplicação via Visual Studio, VS Code ou CLI:
   ```sh
   dotnet run --project src/CustomerCreditClassifier.Api/CustomerCreditClassifier.Api.csproj
   ```

5. Acesse a documentação Swagger em `http://localhost:5180/swagger`.

---
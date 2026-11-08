# JobBoard

![version](https://img.shields.io/badge/version-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)
![GraphQL](https://img.shields.io/badge/GraphQL-HotChocolate-E10098)
![SQL Server](https://img.shields.io/badge/database-SQL%20Server-CC2927)

A job board backend built with **ASP.NET Core**, **HotChocolate GraphQL**, **Entity Framework Core (SQL Server)**, and **JWT authentication**, organized with **Clean Architecture**.

## Features

- GraphQL API with **queries, mutations, and real-time subscriptions** (WebSockets)
- **Filtering, sorting, and projections** out of the box (clients fetch only the fields they ask for)
- **JWT authentication** and authorization on GraphQL operations
- **Repository pattern** over EF Core
- Custom **GraphQL error filter** for consistent, safe error responses
- Automatic **database seeding** with initial data
- **CORS** configured for a browser client

## Architecture

```
JobBoard.sln
├── JobBoard.API             # Presentation: GraphQL schema (Query / Mutation / Subscription), error filter, Program.cs
├── JobBoard.Application     # Use-case contracts: interfaces (e.g. IJobRepository)
├── JobBoard.Domain          # Entities: Company, Job, JobApplication, User
└── JobBoard.Infrastructure  # EF Core DbContext, migrations, repositories, seeder
```

Dependencies point inward:

```
API ──► Application ──► Domain
 │                        ▲
 └──► Infrastructure ─────┘
```

- **Domain** has no dependencies.
- **Application** defines interfaces; **Infrastructure** implements them.
- **API** wires everything together through dependency injection.

## Domain Model

| Entity | Description |
|--------|-------------|
| `Company` | An employer that posts jobs |
| `Job` | A job posting belonging to a company |
| `JobApplication` | A candidate's application to a job |
| `User` | An account (candidate or company member) |

## Tech Stack

- C# / .NET 8
- ASP.NET Core
- HotChocolate (GraphQL server)
- Entity Framework Core + SQL Server
- JWT Bearer authentication

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or Docker)
- EF Core tools: `dotnet tool install --global dotnet-ef`

### 1. Clone

```bash
git clone https://github.com/USERNAME/JobBoard.git
cd JobBoard
```

### 2. Configure secrets

Never commit real secrets. Use **user-secrets** for local development:

```bash
cd JobBoard.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=JobBoardDb;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "JwtSettings:SecretKey" "<a-long-random-string-at-least-32-characters>"
dotnet user-secrets set "JwtSettings:Issuer" "JobBoard"
dotnet user-secrets set "JwtSettings:Audience" "JobBoardClient"
```

### 3. Apply migrations

```bash
dotnet ef database update --project JobBoard.Infrastructure --startup-project JobBoard.API
```

### 4. Run

```bash
dotnet run --project JobBoard.API
```

Open the GraphQL IDE (Nitro / Banana Cake Pop) at `https://localhost:<port>/graphql`.
The database is seeded automatically on startup.

## GraphQL Examples

> Adjust field names to match your schema.

**Query with filtering, sorting, and projection**

```graphql
query {
  jobs(where: { title: { contains: "developer" } }, order: { title: ASC }) {
    id
    title
    company {
      name
    }
  }
}
```

**Mutation (requires `Authorization: Bearer <token>`)**

```graphql
mutation {
  applyToJob(jobId: 1) {
    id
    status
  }
}
```

**Subscription (real-time via WebSockets)**

```graphql
subscription {
  onJobPosted {
    id
    title
  }
}
```

## Authentication

Protected operations require a JWT in the request header:

```
Authorization: Bearer <your-token>
```

Token validation checks issuer, audience, lifetime, and signing key.

## System Design Notes

| Topic | Current (v1.0.0) | Production approach |
|-------|------------------|---------------------|
| Subscriptions | In-memory pub/sub (single instance) | Redis-backed subscriptions so events reach all API instances |
| Caching | None | Redis for hot queries; HotChocolate persisted queries |
| N+1 queries | EF projections | DataLoaders (batching) for nested relations |
| Query abuse | Defaults | Max depth / complexity limits, rate limiting |
| Secrets | user-secrets (local) | Azure Key Vault / environment variables |
| Observability | Console logs | OpenTelemetry, Serilog, health checks |
| Deployment | Local | Docker + CI/CD to Azure App Service / Container Apps |

## Roadmap

- [ ] Redis-backed subscriptions
- [ ] DataLoaders for nested queries
- [ ] Query depth and complexity limits
- [ ] Docker + Docker Compose (API + SQL Server)
- [ ] GitHub Actions CI
- [ ] Unit and integration tests

## License

MIT
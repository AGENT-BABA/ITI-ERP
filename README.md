# ITI ERP System

A full-stack ERP (Enterprise Resource Planning) system built with .NET 9 Web API backend and React frontend.

## Technology Stack

- **Backend**: .NET 9, Entity Framework Core, PostgreSQL
- **Frontend**: React 18, TypeScript, Material UI
- **Infrastructure**: Docker, Nginx

## Prerequisites

- .NET 9 SDK
- Node.js 20+
- Docker & Docker Compose
- PostgreSQL 16

## Getting Started

### Using Docker (Recommended)

```bash
cd docker
docker-compose up -d
```

- Frontend: http://localhost:3000
- API: http://localhost:5000
- Swagger: http://localhost:5000/swagger
- PostgreSQL: localhost:5432

### Local Development

**Backend:**
```bash
cd src/ITI.ERP.Api
dotnet run
```

**Frontend:**
```bash
cd frontend
npm install
npm start
```

## API Documentation

Once the API is running, access Swagger UI at:
http://localhost:5000/swagger

## Project Structure

```
ITI/
├── src/
│   ├── ITI.ERP.Api/           # Web API entry point
│   ├── ITI.ERP.Application/   # Business logic & DTOs
│   ├── ITI.ERP.Domain/        # Domain entities & interfaces
│   └── ITI.ERP.Infrastructure/ # Data access & external services
├── frontend/                  # React application
├── tests/                     # Unit & integration tests
├── docker/                    # Docker configuration
└── docs/                      # Documentation
```

## Development Guidelines

- Follow C# coding conventions
- Write unit tests for business logic
- Use Git conventional commits
- Update API documentation when adding endpoints

## License

MIT License

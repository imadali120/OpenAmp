# OpenAmp

OpenAmp je platforma za rezervaciju sala za muzičke probe, upravljanje bendovima, studijima, opremom i potrošnim artiklima.

Repozitorij trenutno sadrži:

- FAZU 1: SQL Server bazu, EF Core model, migracije i seed podatke
- FAZU 2: REST API, JWT autentikaciju, logiku rezervacija i Stripe plaćanje

## Implementirano

- .NET 8 slojevita arhitektura: \`Domain\`, \`Application\`, \`Infrastructure\` i \`Api\`
- Entity Framework Core 8 i SQL Server 2022
- Fluent API konfiguracije ključeva, relacija, indeksa i ograničenja
- seed podaci za uloge, statuse, žanrove, instrumente, studije, sale i inventar
- JWT registracija, prijava i rotacija refresh tokena
- PBKDF2-SHA512 hashiranje lozinki i hashirani refresh tokeni u bazi
- provjera slobodnih termina sale i opreme
- kalkulacija cijene sale, najma opreme i kupljenih artikala
- izmjena i otkazivanje rezervacije uz optimistic concurrency (\`RowVersion\`)
- Stripe Payment Intents, potpisani webhook i automatski refund
- Swagger/OpenAPI i health endpoint
- CQRS handleri, DTO modeli i servisni/repository ugovori
- Mermaid [ERD](docs/erd.md)

## Struktura

\`\`\`text
src/
  OpenAmp.Domain/          Entiteti i domenska pravila
  OpenAmp.Application/     DTO modeli, CQRS komande/upiti i ugovori
  OpenAmp.Infrastructure/  EF Core, autentikacija, rezervacije i Stripe
  OpenAmp.Api/             REST kontroleri, JWT, Swagger i middleware
tests/
  OpenAmp.Infrastructure.Tests/
docs/
  erd.md
  phase2-api.md
\`\`\`

## Pokretanje

Za razvoj su potrebni .NET 8 SDK i Docker Desktop.

1. Pokrenuti SQL Server:

   \`\`\`powershell
   docker compose up -d
   docker compose ps
   \`\`\`

2. Pokrenuti API:

   \`\`\`powershell
   dotnet restore
   dotnet run --project src/OpenAmp.Api --launch-profile https
   \`\`\`

3. Otvoriti:

   - Swagger: \`https://localhost:7149/swagger\`
   - health check: \`https://localhost:7149/health\`

U Development okruženju API automatski primjenjuje EF Core migracije. Razvojni SQL password i JWT ključ iz \`appsettings.Development.json\` služe samo za lokalni rad i moraju se zamijeniti prije deploymenta.

Detaljni endpointi i Stripe konfiguracija nalaze se u [uputama za FAZU 2](docs/phase2-api.md).

## Ručno izvršavanje migracija

\`\`\`powershell
$env:OPENAMP_CONNECTION_STRING='Server=localhost,1433;Database=OpenAmp;User Id=sa;Password=OpenAmp_Dev123!;TrustServerCertificate=True;Encrypt=False'
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/OpenAmp.Infrastructure
\`\`\`

## Build i testovi

\`\`\`powershell
dotnet restore
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
\`\`\`

## Zaštita rezervacija

\`RowVersion\` otkriva konkurentnu izmjenu postojeće rezervacije. Kreiranje i provjera preklapanja izvršavaju se u SQL Server \`SERIALIZABLE\` transakciji. Granice termina su poluotvorene (\`[od, do)\`), pa termin 10:00–12:00 ne blokira termin koji počinje tačno u 12:00.

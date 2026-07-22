# OpenAmp

OpenAmp je platforma za rezervaciju sala za muzičke probe, upravljanje bendovima, studijima, opremom i potrošnim artiklima. Ovaj repozitorij trenutno sadrži **Fazu 1: bazu podataka i arhitekturu projekta**.

## Šta je implementirano

- .NET 8 solution sa slojevima `Domain`, `Application` i `Infrastructure`
- kompletan Entity Framework Core 8 model za SQL Server
- Fluent API konfiguracije svih ključeva, relacija, indeksa i ograničenja
- šifarnici i seed podaci za uloge, statuse, žanrove, instrumente i kategorije
- testni studiji, sale, galerija, oprema i artikli
- inicijalna EF Core migracija
- optimistic concurrency preko `Rezervacija.RowVersion`
- atomska zaštita od preklapanja termina u `SERIALIZABLE` transakciji
- provjera zauzetosti odabrane opreme u istom terminu
- Mermaid [ERD](docs/erd.md)
- SQL Server 2022 Docker Compose konfiguracija

## Struktura

```text
src/
  OpenAmp.Domain/          Entiteti i domenska pravila
  OpenAmp.Application/     Use-case ugovori i aplikacijski izuzeci
  OpenAmp.Infrastructure/  EF Core, SQL Server, seed i servis rezervacija
tests/
  OpenAmp.Infrastructure.Tests/
docs/
  erd.md
```

## Pokretanje baze

1. Kopirati `.env.example` u `.env` i po potrebi promijeniti razvojnu lozinku.
2. Pokrenuti SQL Server:

   ```powershell
   docker compose up -d
   ```

3. Postaviti connection string i primijeniti migraciju:

   ```powershell
   $env:OPENAMP_CONNECTION_STRING='Server=localhost,1433;Database=OpenAmp;User Id=sa;Password=OpenAmp_Dev123!;TrustServerCertificate=True;Encrypt=False'
   dotnet ef database update --project src/OpenAmp.Infrastructure
   ```

## Build i testovi

```powershell
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

## Zaštita od dvostruke rezervacije

`RowVersion` rješava optimistic concurrency pri izmjeni postojeće rezervacije. Dva istovremena kreiranja su zaseban problem jer oba rade `INSERT`; zato `RezervacijaService` provjeru preklapanja i upis izvršava u SQL Server `SERIALIZABLE` transakciji. Indeks nad `(SalaId, TerminOdUtc, TerminDoUtc)` omogućava efikasno range zaključavanje. Granice termina su poluotvorene (`[od, do)`), pa termin 10:00–12:00 ne blokira termin koji počinje tačno u 12:00.

## Napomena o seed podacima

Seed podaci su demonstracijski. URL-ovi slika koriste lokalnu `example.openamp.local` domenu, a razvojni SQL password mora se promijeniti prije bilo kakvog javnog ili produkcijskog deploymenta.

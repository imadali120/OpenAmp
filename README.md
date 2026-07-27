# OpenAmp

OpenAmp je platforma za rezervaciju sala za muzičke probe, upravljanje bendovima, studijima, opremom i potrošnim artiklima.

Repozitorij sadrži:

- FAZU 1: SQL Server bazu, EF Core model, migracije i seed podatke
- FAZU 2: REST API, JWT autentikaciju, logiku rezervacija i Stripe plaćanje
- FAZU 3: Flutter mobilnu aplikaciju za Android i iOS
- FAZU 3.1: kompletiranje mobilnih rezervacija, profila, bendova, recenzija i Stripe lifecyclea
- FAZU 4: WPF desktop aplikaciju za administratore i zaposlenike studija
- FAZU 5: hibridni sistem preporuka, poslovne izvještaje i PDF izvoz

## Implementirano

- .NET 8 slojevita arhitektura: `Domain`, `Application`, `Infrastructure` i `Api`
- Entity Framework Core 8 i SQL Server 2022
- JWT registracija, prijava emailom ili usernameom i rotacija refresh tokena
- jedinstveni usernameovi, pozivnice za bend po usernameu i jedinstvena password politika
- provjera termina, kalkulacija cijene, izmjena i otkazivanje rezervacije
- Stripe Payment Intents, potpisani webhook i automatski refund
- Flutter aplikacija sa Riverpod state managementom i Dio API klijentom
- pretraga i detalji sala, satni slotovi, oprema, artikli i Stripe PaymentSheet
- upravljanje bendovima, članovima i pozivnicama, historija proba i profil
- upload profilnih, bend i studio/sala fotografija u SQL Server bazu
- tamni mobilni interfejs sa OpenAmp narandžastom akcent bojom
- WPF desktop dashboard, upravljanje salama, opremom, servisima, artiklima, rezervacijama, bendovima i korisnicima
- tamni desktop interfejs sa sedmičnim kalendarom i OpenAmp narandžastom akcent bojom
- izmjena/otkazivanje termina, refund pregled, recenzije i ponovno rezervisanje
- trajno sačuvane sale, navigacija do studija i korisničke postavke
- Stripe Customer Session za sačuvane kartice i oporavak napuštenog checkouta
- Swagger/OpenAPI, health endpoint i testovi
- preporuke sala, statistiku poslovanja i PDF izvještaje
- Mermaid [ERD](docs/erd.md)

## Struktura

```text
src/
  OpenAmp.Domain/          Entiteti i domenska pravila
  OpenAmp.Application/     DTO modeli, CQRS komande/upiti i ugovori
  OpenAmp.Infrastructure/  EF Core, autentikacija, rezervacije i Stripe
  OpenAmp.Api/             REST kontroleri, JWT, Swagger i middleware
  OpenAmp.Mobile/          Flutter aplikacija za muzičare
  OpenAmp.Desktop/         WPF aplikacija za studio
tests/
  OpenAmp.Infrastructure.Tests/
docs/
  erd.md
  phase2-api.md
  phase3-mobile.md
  phase4-desktop.md
  phase5-recommendations-reports.md
```

## Pokretanje baze i API-ja

Za razvoj su potrebni .NET 8 SDK i Docker Desktop.

```powershell
docker compose up -d
dotnet run --project src/OpenAmp.Api --launch-profile http
```

- Swagger: `http://localhost:5264/swagger`
- health check: `http://localhost:5264/health`

U Development okruženju API automatski primjenjuje EF Core migracije. Razvojni SQL password i JWT ključ služe samo za lokalni rad.

## Pokretanje Flutter aplikacije

```powershell
cd src/OpenAmp.Mobile
flutter pub get
flutter run --dart-define=OPENAMP_API_URL=http://10.0.2.2:5264 --dart-define=STRIPE_PUBLISHABLE_KEY=pk_test_...
```

`10.0.2.2` je adresa host računara iz Android emulatora. Za fizički uređaj koristi lokalnu IP adresu računara. Detaljne upute su u [dokumentaciji FAZE 3](docs/phase3-mobile.md).

## Pokretanje desktop aplikacije

Dok su baza i API pokrenuti:

```powershell
dotnet run --project src/OpenAmp.Desktop
```

Lokalni razvojni računi su `admin / OpenAmp1!` i `zaposlenik / OpenAmp1!`. Detaljne upute su u [dokumentaciji FAZE 4](docs/phase4-desktop.md).

## Build i testovi

```powershell
dotnet build OpenAmp.sln --configuration Release
dotnet test OpenAmp.sln --configuration Release --no-build

cd src/OpenAmp.Mobile
flutter analyze
flutter test
```

## Zaštita rezervacija

`RowVersion` otkriva konkurentnu izmjenu postojeće rezervacije. Kreiranje i provjera preklapanja izvršavaju se u SQL Server `SERIALIZABLE` transakciji. Granice termina su poluotvorene (`[od, do)`), pa termin 10:00–12:00 ne blokira termin koji počinje tačno u 12:00.

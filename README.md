# OpenAmp

OpenAmp je informacioni sistem za rezervaciju sala za muzičke probe, upravljanje bendovima, studijima, opremom i prodajnim artiklima.

Sistem pokriva svih pet faza seminarskog rada:

- SQL Server bazu, EF Core model, migracije i razvojne podatke
- .NET 8 REST API, JWT autentikaciju, rezervacije i Stripe plaćanje
- Flutter mobilnu aplikaciju za muzičare
- Flutter Windows aplikaciju za administratore i zaposlenike
- hibridne preporuke, poslovne izvještaje, PDF izvoz i štampu

## Tehnologije i arhitektura

- .NET 8: `Domain`, `Application`, `Infrastructure`, `Api` i `Worker`
- Entity Framework Core 8 i SQL Server 2022, baza `220336`
- CQRS handleri, servisi i repozitoriji
- RabbitMQ za asinhronu obradu obavijesti o rezervacijama
- Flutter za Android, iOS i Windows, uz Riverpod i Dio
- Stripe Payment Intents, 3D Secure, webhook i automatski refund
- JWT access token, rotirajući refresh token i role `ADMIN`, `ZAPOSLENIK`, `MUZICAR`
- Swagger/OpenAPI, health check, automatske migracije i razvojni seed

## Funkcionalnosti

- registracija, prijava emailom ili usernameom, profilna fotografija i stroga password politika
- jedinstveni usernameovi i pozivanje članova benda po usernameu
- pretraga sala, galerija, raspoloživi slotovi, oprema i artikli
- kalkulacija cijene, rezervacija, izmjena, otkazivanje i plaćanje
- bendovi, članovi, pozivnice, historija proba, favoriti, recenzije i notifikacije
- administracija studija, sala, opreme, servisa, artikala, rezervacija, bendova i korisnika
- CRUD svih sistemskih šifarnika uz inline validaciju
- Cosine Similarity, item-based collaborative filtering i dinamički hibridni scoring
- statistika prihoda po salama i rezervacija po žanru, PDF izvoz i direktna štampa

ER dijagram se nalazi u [docs/erd.md](docs/erd.md).

## Struktura

```text
src/
  OpenAmp.Domain/          Entiteti i domenska pravila
  OpenAmp.Application/     DTO modeli, CQRS komande/upiti i ugovori
  OpenAmp.Infrastructure/  EF Core, servisi, Stripe i RabbitMQ publisher
  OpenAmp.Api/             REST kontroleri, JWT, Swagger i middleware
  OpenAmp.Worker/          RabbitMQ background worker
  OpenAmp.Mobile/          Flutter Android/iOS/Windows aplikacija
tests/
  OpenAmp.Infrastructure.Tests/
docs/
```

## Najbrže pokretanje kompletnog sistema

Potrebni su Docker Desktop i Flutter SDK. Za standardni Windows build potreban je Visual Studio Build Tools workload **Desktop development with C++**, uključujući komponentu **C++ ATL for latest v143 build tools**.

```powershell
git clone https://github.com/imadali120/OpenAmp.git
cd OpenAmp
docker compose up --build -d
```

Compose podiže SQL Server, RabbitMQ, API i Worker. API automatski kreira bazu `220336`, primjenjuje migracije i dodaje testne podatke.

- Swagger: `http://localhost:5264/swagger`
- health check: `http://localhost:5264/health`
- RabbitMQ Management: `http://localhost:15672`

Stripe test ključevi su opcionalni i unose se samo lokalno kroz `.env`; primjer je u `.env.example`.

## Testni računi

Svi razvojni računi koriste lozinku `test`.

| Aplikacija | Username | Uloga |
|---|---|---|
| Flutter Windows | `admin` | Administrator |
| Flutter Windows | `zaposlenik` | Zaposlenik |
| Flutter Android/iOS | `muzicar` | Muzičar |
| Flutter Android/iOS | `jazz` | Muzičar |
| Flutter Android/iOS | `metal` | Muzičar |

## Flutter mobilna aplikacija

```powershell
cd src/OpenAmp.Mobile
flutter pub get
flutter run -d emulator-5554 `
  --dart-define=OPENAMP_API_URL=http://10.0.2.2:5264 `
  --dart-define=STRIPE_PUBLISHABLE_KEY=pk_test_VAS_KLJUC
```

`10.0.2.2` je adresa host računara iz Android emulatora. Za fizički uređaj koristi lokalnu IP adresu računara.

## Flutter Windows aplikacija

Na Windowsu prvo uključite **Settings → System → For developers → Developer Mode**, jer Flutter plugini koriste symlinkove.

```powershell
cd src/OpenAmp.Mobile
flutter pub get
flutter run -d windows --dart-define=OPENAMP_API_URL=http://127.0.0.1:5264
```

Nakon prijave `admin/test` ili `zaposlenik/test`, aplikacija automatski otvara desktop administratorski interfejs. Prijava muzičara otvara mobilni interfejs.

Ako Developer Mode ili ATL nisu dostupni, repozitorij sadrži fallback build skriptu koja koristi službene Microsoft pakete bez trajne sistemske izmjene:

```powershell
.\scripts\build-windows.ps1
```

## Lokalno pokretanje API-ja bez Dockera

SQL Server i RabbitMQ moraju biti dostupni, a lokalne tajne se postavljaju kroz .NET user-secrets.

```powershell
dotnet run --project .\src\OpenAmp.Api\OpenAmp.Api.csproj --launch-profile http
dotnet run --project .\src\OpenAmp.Worker\OpenAmp.Worker.csproj
```

Ako se komanda pokreće iz `C:\Users\imado`, prvo pređite u direktorij repozitorija ili koristite punu putanju do `.csproj` fajla.

## Build i testovi

```powershell
dotnet build OpenAmp.sln --configuration Release
dotnet test OpenAmp.sln --configuration Release --no-build

cd src/OpenAmp.Mobile
flutter analyze
flutter test
flutter build apk --release
flutter build windows --release
```

## Zaštita rezervacija

`RowVersion` osigurava optimistic locking pri izmjeni rezervacije. Kreiranje termina i provjera preklapanja izvršavaju se u SQL Server `SERIALIZABLE` transakciji. Granice termina su poluotvorene (`[od, do)`), pa termin 10:00–12:00 ne blokira termin koji počinje tačno u 12:00.

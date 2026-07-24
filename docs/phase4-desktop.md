# FAZA 4 — Desktop aplikacija

Desktop aplikacija je WPF klijent za administratore i zaposlenike studija. Koristi isti REST API i SQL Server bazu kao mobilna aplikacija; desktop klijent ne pristupa bazi direktno.

## Implementirano

- JWT prijava za uloge `ADMIN` i `ZAPOSLENIK`, uz automatsku obnovu access tokena
- dashboard sa današnjim probama, aktivnim salama, opremom na najmu, niskim zalihama i sedmičnom zauzetošću
- dodavanje, izmjena, deaktiviranje i galerija fotografija sala
- inventar opreme, dodjela sali, stanje od 1 do 5 i status
- prijava kvara, otvoreni servis, završetak servisa, trošak i kompletna servisna historija
- upravljanje potrošnim artiklima i upozorenja kada količina dođe do minimalne zalihe
- sedmični kalendar rezervacija po danima, kreiranje i izmjena termina/statusa
- pregled i izmjena osnovnih podataka bendova, članova i žanra
- pregled korisnika; administrator može promijeniti ulogu i aktivirati/deaktivirati račun
- tamni OpenAmp interfejs sa narandžastim akcentom

Izvještaji i sistem preporuka nisu dio ove faze. Implementiraju se zajedno u FAZI 5.

## Pokretanje

U prvom terminalu:

```powershell
docker compose up -d
dotnet run --project src/OpenAmp.Api --launch-profile http
```

U drugom terminalu:

```powershell
dotnet run --project src/OpenAmp.Desktop
```

Desktop podrazumijevano koristi `http://localhost:5264`. Druga adresa API-ja može se zadati varijablom:

```powershell
$env:OPENAMP_API_URL = "http://localhost:5264"
dotnet run --project src/OpenAmp.Desktop
```

Kada se API pokrene u `Development` okruženju, automatski se kreiraju lokalni računi ako već ne postoje:

- `admin` / `OpenAmp1!`
- `zaposlenik` / `OpenAmp1!`

Ovi računi i lozinke namijenjeni su isključivo lokalnom razvoju.

## Desktop API

Zaštićene rute nalaze se ispod `api/desktop`:

- `GET /dashboard`
- `GET|POST|PUT|DELETE /halls`
- `GET|POST|PUT /equipment`
- `POST|PUT /equipment/{id}/services`
- `GET|POST|PUT /articles`
- `GET|POST|PUT /reservations`
- `GET|PUT /bands`
- `GET|PUT /users`

Sve rute zahtijevaju `ADMIN` ili `ZAPOSLENIK` JWT ulogu. Promjena uloge ili statusa korisnika dodatno zahtijeva `ADMIN`.

## Baza

Migracija `Phase4DesktopOperations` dodaje:

- kolonu `Oprema.Stanje`
- tabelu `ServisiOpreme`
- relacije prema opremi i korisniku koji je prijavio kvar
- ograničenja za stanje opreme i trošak servisa

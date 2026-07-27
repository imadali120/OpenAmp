# FAZA 4 — Flutter desktop aplikacija

Desktop aplikacija je Flutter Windows klijent za administratore i zaposlenike studija. Koristi isti REST API kao mobilna aplikacija i ne pristupa bazi direktno.

## Implementirano

- JWT prijava za uloge `ADMIN` i `ZAPOSLENIK`, uz automatsku obnovu access tokena
- dashboard sa današnjim probama, aktivnim salama, opremom na najmu, niskim zalihama i sedmičnom zauzetošću
- CRUD studija, sala, opreme, artikala i svih sistemskih šifarnika
- inventar opreme, dodjela sali, stanje, status i servisna historija
- pregled i izmjena rezervacija uz optimistic locking
- pretraga i uređivanje bendova i korisničkih uloga
- izvještaji prihoda i rezervacija, filtriranje perioda, PDF izvoz i direktna štampa
- inline validacija svih administratorskih obrazaca
- jedinstveni tamni OpenAmp interfejs sa narandžastim akcentom

## Pokretanje

```powershell
docker compose up --build -d
cd src/OpenAmp.Mobile
flutter pub get
flutter run -d windows --dart-define=OPENAMP_API_URL=http://127.0.0.1:5264
```

Flutter plugini na Windowsu zahtijevaju uključen **Developer Mode**. Testni desktop računi su:

- `admin` / `test`
- `zaposlenik` / `test`

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
- `GET /reports` i `GET /reports/pdf`
- CRUD `/reference-data/{type}` i `/reference-data/studios/all`

Sve rute zahtijevaju `ADMIN` ili `ZAPOSLENIK` JWT ulogu. Promjena korisnika i uređivanje sistemskih šifarnika dodatno zahtijevaju `ADMIN`.

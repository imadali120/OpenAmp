# Provjera kriterija seminarskog rada

Ovaj dokument mapira zahtjeve iz uputa za seminarski rad na implementaciju u OpenAmp repozitoriju.

| Kriterij | Implementacija / dokaz |
|---|---|
| Javni GitHub repozitorij | `https://github.com/imadali120/OpenAmp` |
| Baza nazvana brojem indeksa | SQL Server baza `220336` u development konfiguraciji i Docker Composeu |
| Najmanje deset povezanih tabela | EF Core model sadrži 29 tabela; ERD je u `docs/erd.md` |
| Migracije i početni podaci | EF Core migracije i `DevelopmentDataSeeder` |
| Tri korisničke uloge | `ADMIN`, `ZAPOSLENIK`, `MUZICAR` |
| Testni računi s lozinkom `test` | `admin`, `zaposlenik`, `muzicar`, `jazz`, `metal` |
| Mobilna aplikacija | Flutter Android/iOS interfejs za muzičare |
| Desktop aplikacija | Flutter Windows administratorski interfejs |
| REST API i autentikacija | .NET 8 kontroleri, JWT access/refresh tokeni i autorizacija po ulozi |
| CRUD poslovnih podataka | Sale, oprema, artikli, rezervacije, bendovi, korisnici i studiji |
| CRUD šifarnika | Žanrovi, instrumenti, uloge, kategorije i svi statusi |
| Inline validacija | Flutter `Form`/`TextFormField` validatori i serverska validacija |
| Pretraga i filtriranje | Sale, bendovi, artikli, korisnici, izvještaji i dostupni termini |
| Obrada konkurentnosti | `RowVersion` optimistic locking i `SERIALIZABLE` provjera termina |
| Asinhrona obrada | RabbitMQ publisher i zaseban .NET Worker Service |
| Docker okruženje | SQL Server, RabbitMQ, API i Worker u `docker-compose.yml` |
| Plaćanje | Stripe Payment Intents, PaymentSheet/3D Secure, webhook i refund |
| Izvještaji | Prihod po salama, rezervacije po žanru, PDF i direktna štampa |
| Sistem preporuka | Cosine Similarity, item-based collaborative filtering i hibridni score |
| Testiranje | 44 .NET testa, 5 Flutter testa, analyzer i Docker E2E provjera |
| Dokumentacija pokretanja | Glavni `README.md` i dokumenti po fazama u `docs/` |

## Razvojni računi

| Username | Lozinka | Uloga |
|---|---|---|
| `admin` | `test` | Administrator |
| `zaposlenik` | `test` | Zaposlenik |
| `muzicar` | `test` | Muzičar |

Pravi Stripe ključevi nisu dio repozitorija. Postavljaju se lokalno kroz `.env`, .NET user-secrets ili Flutter `--dart-define`.

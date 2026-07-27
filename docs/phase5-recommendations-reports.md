# FAZA 5 - preporuke i izvještaji

## Sistem preporuka

`RecommendationEngine` kombinuje dva nezavisna signala:

1. Content-based rezultat koristi cosine similarity između vektora benda i sale.
2. Collaborative rezultat koristi item-based adjusted cosine similarity nad matricom `bend x sala`.

Vektor sale sadrži raspone kapaciteta i cijene, afinitet prema žanrovima, dostupne
kategorije opreme i akustičke karakteristike. Vektor benda se formira iz žanra,
broja članova, instrumenata i historije ranijih rezervacija.

Hibridni rezultat je:

```text
score = alpha * content_score + (1 - alpha) * collaborative_score
```

Faktor `alpha` se dinamički smanjuje sa rastom broja sala koje je bend ranije
rezervisao. Novi bend zato koristi većinski content-based preporuku (`alpha = 0.90`),
dok se za bend sa historijom povećava uticaj collaborative filtera. Vrijednost je
ograničena na raspon `0.35 - 0.90`.

Prije rangiranja servis:

- zadržava samo aktivne sale;
- primjenjuje filtere žanra, kapaciteta i opreme;
- izbacuje zauzete sale za odabrani termin;
- izbacuje sale koje bend već ima u aktivnoj rezervaciji;
- vraća najviše 20 rezultata, sortirano po hibridnom rezultatu.

### REST endpoint

```http
GET /api/recommendations/bands/{bandId}/halls
```

Podržani query parametri su `limit`, `fromUtc`, `toUtc`, `genre`,
`minimumCapacity` i `equipmentCategory`. Endpoint zahtijeva JWT i provjerava da
je prijavljeni korisnik osnivač ili aktivni član benda. Mobilna aplikacija prikazuje
prvih pet preporučenih sala iznad standardnih rezultata pretrage.

## Poslovni izvještaji

Izvještaj obuhvata samo plaćene i izvršene rezervacije. Prihod je neto vrijednost:

```text
prihod = ukupna_cijena - refundirani_iznos
```

Statistika sadrži ukupan prihod, broj rezervacija, prosječnu vrijednost rezervacije,
ukupan broj rezervisanih sati, prihod i udio po salama te broj i udio rezervacija
po žanru.

Filteri su period, sala i žanr. Period je poluotvoren `[fromUtc, toUtc)` i može
obuhvatiti najviše pet godina.

### REST endpointi

```http
GET /api/desktop/reports
GET /api/desktop/reports/pdf
```

Oba endpointa prihvataju `fromUtc`, `toUtc`, opcionalni `hallId` i opcionalni
`genreId`. Dozvoljeni su korisnici u ulogama `ADMIN` i `ZAPOSLENIK`.

Desktop ekran `Izvještaji` sadrži filtere, četiri ključne metrike, raspodjelu
prihoda po salama i rezervacija po žanru. Dugme `Izvezi PDF` otvara standardni
Windows dijalog za izbor lokacije.

## Provjera

```powershell
dotnet build OpenAmp.sln --configuration Release
dotnet test OpenAmp.sln --configuration Release

cd src/OpenAmp.Mobile
flutter analyze
flutter test
```

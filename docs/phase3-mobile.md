# FAZA 3 — Flutter mobilna aplikacija

## Tehnologije

- Flutter 3.44 / Dart 3.12
- Riverpod za stanje aplikacije
- Dio za REST komunikaciju i automatski refresh JWT tokena
- Flutter Secure Storage za lokalno čuvanje sesije
- flutter_stripe PaymentSheet za kartice i 3D Secure
- Material 3 tamni UI sa neutralnom paletom i OpenAmp narandžastom

## Ekrani

1. Autentikacija — registracija i prijava muzičara.
2. Sale — brza pretraga po tekstu, žanru, kapacitetu i opremi.
3. Detalji sale — galerija, cijena, kapacitet, akustika, oprema i recenzije.
4. Termin — bend, sedmodnevni izbor datuma i uzastopni satni slotovi.
5. Oprema i artikli — dodavanje stavki uz trenutni obračun cijene.
6. Plaćanje — kreiranje rezervacije, Payment Intenta i prikaz Stripe PaymentSheeta.
7. Moji bendovi — naslovna fotografija, članovi i pozivnice po usernameu.
8. Rezervacije — predstojeće probe i historija.
9. Profil — instrumenti, broj bendova/proba, sati, recenzije i omiljena sala.

## FAZA 3.1 — završni mobilni tokovi

- detalj rezervacije sa stavkama, nastavkom plaćanja i ponovnom rezervacijom iste sale
- izmjena termina uz ponovnu provjeru dostupnosti i optimistic concurrency zaštitu
- pregled politike otkazivanja, automatski izračun povrata i Stripe refund
- ocjena i komentar nakon završene plaćene probe
- trajno sačuvane sale i brzi filter „Sačuvane”
- lokacija studija otvorena u vanjskoj navigaciji
- uređivanje usernamea, profila, fotografije iz galerije, kontakta i instrumenata
- postavke push/email obavijesti, jezika i privatnosti profila
- Android/iOS lokalni podsjetnik dva sata prije probe, sistemska dozvola i testna notifikacija
- ekran obavijesti sa predstojećim rezervacijama i pozivnicama za bend
- promjena lozinke
- primljene pozivnice za bend, prihvatanje/odbijanje, uređivanje člana, uklanjanje i napuštanje benda
- Stripe Customer Session za sigurno čuvanje i uklanjanje kartica unutar PaymentSheeta
- zaštita od napuštenih rezervacija: korisnik može osloboditi termin ako prekine checkout

## Backend endpointi za mobilnu aplikaciju

| Metoda | Ruta | Namjena |
| --- | --- | --- |
| GET | `/api/mobile/lookups` | Žanrovi, instrumenti i kategorije opreme |
| GET | `/api/salas` | Filtrirana pretraga sala |
| GET | `/api/salas/{id}` | Detalji sale, oprema, artikli i recenzije |
| GET | `/api/reservations/availability` | Slobodni satni slotovi |
| GET | `/api/reservations/mine` | Historija korisnika i njegovih bendova |
| GET | `/api/bands/mine` | Bendovi prijavljenog korisnika |
| POST | `/api/bands` | Kreiranje benda |
| POST | `/api/bands/{id}/invitations` | Slanje pozivnice po usernameu |
| GET | `/api/bands/invitations/received` | Primljene pozivnice |
| POST | `/api/bands/invitations/{id}/respond` | Prihvatanje ili odbijanje pozivnice |
| PUT | `/api/bands/{id}` | Uređivanje benda |
| PUT/DELETE | `/api/bands/{id}/members/{userId}` | Upravljanje članom ili napuštanje benda |
| GET | `/api/users/me/overview` | Prošireni profil i statistike |
| PUT | `/api/users/me` | Username, kontakt i instrumenti |
| POST | `/api/images/profile` | Upload profilne slike u SQL bazu |
| POST | `/api/images/bands/{bandId}` | Upload naslovne slike benda |
| POST | `/api/users/me/change-password` | Promjena lozinke |
| GET/PUT | `/api/users/me/settings` | Notifikacije, jezik i privatnost |
| GET/PUT/DELETE | `/api/users/me/favorite-halls/{hallId}` | Sačuvane sale |
| GET | `/api/reservations/{id}/cancellation-preview` | Politika i mogući iznos povrata |
| PUT | `/api/reservations/{id}` | Izmjena termina |
| POST | `/api/reservations/{id}/cancel` | Otkazivanje i refund |
| POST | `/api/reservations/{id}/review` | Recenzija završene probe |

## Lokalna konfiguracija

Android emulator koristi `10.0.2.2` za pristup host računaru:

```powershell
flutter run --dart-define=OPENAMP_API_URL=http://10.0.2.2:5264 --dart-define=STRIPE_PUBLISHABLE_KEY=pk_test_...
```

Ako host mreža ne prosljeđuje HTTP odgovor prema `10.0.2.2`, koristi ADB reverse tunel:

```powershell
adb reverse tcp:5264 tcp:5264
flutter run --dart-define=OPENAMP_API_URL=http://127.0.0.1:5264 --dart-define=STRIPE_PUBLISHABLE_KEY=pk_test_...
```

Za fizički Android uređaj oba uređaja moraju biti na istoj mreži, API mora slušati na dostupnoj adresi, a `OPENAMP_API_URL` treba sadržavati LAN IP računara.

## Stripe tok

1. Flutter šalje stavke i termin na `POST /api/reservations`.
2. Backend ponovo provjerava dostupnost i računa konačnu cijenu.
3. Flutter traži Payment Intent i kratkotrajni Stripe Customer Session od `POST /api/payments/reservations/{id}/payment-intent`.
4. Stripe PaymentSheet prikuplja karticu, omogućava čuvanje/uklanjanje kartice i izvršava potrebnu 3D Secure autentikaciju.
5. Stripe webhook potvrđuje uplatu i mijenja status rezervacije u `Plaćena`.
6. Mobilna aplikacija kratko provjerava status nakon PaymentSheeta; webhook ostaje autoritativni izvor konačnog statusa.
7. Ako korisnik prekine checkout, aplikacija nudi otkazivanje rezervacije i oslobađanje termina/artikala.

U Git se ne upisuju `sk_test_...`, `whsec_...` niti produkcijski ključevi.

## Verifikacija

```powershell
flutter analyze
flutter test
flutter test integration_test/app_smoke_test.dart
```

Android APK build dodatno zahtijeva instaliran Android SDK i prihvaćene Google SDK licence:

```powershell
flutter doctor -v
flutter build apk --debug
```

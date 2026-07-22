# FAZA 3 — Flutter mobilna aplikacija

## Tehnologije

- Flutter 3.44 / Dart 3.12
- Riverpod za stanje aplikacije
- Dio za REST komunikaciju i automatski refresh JWT tokena
- Flutter Secure Storage za lokalno čuvanje sesije
- flutter_stripe PaymentSheet za kartice i 3D Secure
- Material 3 UI prilagođen mockupima iz poglavlja 4.2

## Ekrani

1. Autentikacija — registracija i prijava muzičara.
2. Sale — brza pretraga po tekstu, žanru, kapacitetu i opremi.
3. Detalji sale — galerija, cijena, kapacitet, akustika, oprema i recenzije.
4. Termin — bend, sedmodnevni izbor datuma i uzastopni satni slotovi.
5. Oprema i artikli — dodavanje stavki uz trenutni obračun cijene.
6. Plaćanje — kreiranje rezervacije, Payment Intenta i prikaz Stripe PaymentSheeta.
7. Moji bendovi — kreiranje benda, članovi, pozivnice i kodovi za pridruživanje.
8. Rezervacije — predstojeće probe i historija.
9. Profil — instrumenti, broj bendova/proba, sati, recenzije i omiljena sala.

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
| POST | `/api/bands/{id}/invitations` | Slanje pozivnice emailom |
| GET | `/api/users/me/overview` | Prošireni profil i statistike |

## Lokalna konfiguracija

Android emulator koristi `10.0.2.2` za pristup host računaru:

```powershell
flutter run --dart-define=OPENAMP_API_URL=http://10.0.2.2:5264 --dart-define=STRIPE_PUBLISHABLE_KEY=pk_test_...
```

Za fizički Android uređaj oba uređaja moraju biti na istoj mreži, API mora slušati na dostupnoj adresi, a `OPENAMP_API_URL` treba sadržavati LAN IP računara.

## Stripe tok

1. Flutter šalje stavke i termin na `POST /api/reservations`.
2. Backend ponovo provjerava dostupnost i računa konačnu cijenu.
3. Flutter traži Payment Intent od `POST /api/payments/reservations/{id}/payment-intent`.
4. Stripe PaymentSheet prikuplja karticu i izvršava potrebnu 3D Secure autentikaciju.
5. Stripe webhook potvrđuje uplatu i mijenja status rezervacije u `Plaćena`.

U Git se ne upisuju `sk_test_...`, `whsec_...` niti produkcijski ključevi.

## Verifikacija

```powershell
flutter analyze
flutter test
```

Android APK build dodatno zahtijeva instaliran Android SDK i prihvaćene Google SDK licence:

```powershell
flutter doctor -v
flutter build apk --debug
```

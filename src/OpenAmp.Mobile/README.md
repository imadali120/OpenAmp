# OpenAmp Mobile

Flutter aplikacija za muzičare, razvijena prema mockupima iz poglavlja 4.2 projektne prijave.

## Funkcionalnosti

- registracija, prijava i automatska rotacija JWT refresh tokena
- pretraga sala po žanru, kapacitetu i kategoriji opreme
- detalji sale, galerija, oprema, artikli i recenzije
- izbor benda, datuma i uzastopnih satnih slotova
- trenutna kalkulacija sale, opreme i artikala
- Stripe PaymentSheet, 3D Secure i sačuvane kartice preko Stripe Customer Sessiona
- slanje, prihvatanje i odbijanje pozivnica te upravljanje članovima benda
- detalj, izmjena, otkazivanje/refund i ponovna rezervacija termina
- recenzija nakon probe, trajno sačuvane sale i navigacija do studija
- uređivanje profila, instrumenata, postavki i lozinke
- sistemske notifikacije i automatski podsjetnik dva sata prije probe
- ekran obavijesti sa predstojećim terminima i pozivnicama

## Pokretanje

Pokrenuti bazu i API iz korijena repozitorija:

```powershell
docker compose up -d
dotnet run --project src/OpenAmp.Api --launch-profile http
```

Zatim:

```powershell
cd src/OpenAmp.Mobile
flutter pub get
flutter run --dart-define=OPENAMP_API_URL=http://10.0.2.2:5264 --dart-define=STRIPE_PUBLISHABLE_KEY=pk_test_...
```

Ako emulator ne dobija HTTP odgovor preko `10.0.2.2`, koristi tunel:

```powershell
adb reverse tcp:5264 tcp:5264
flutter run --dart-define=OPENAMP_API_URL=http://127.0.0.1:5264 --dart-define=STRIPE_PUBLISHABLE_KEY=pk_test_...
```

Za provjeru:

```powershell
flutter analyze
flutter test
```

Stripe secret key i webhook secret ostaju isključivo na backendu. Mobilna aplikacija dobija samo publishable key i `clientSecret` Payment Intenta.

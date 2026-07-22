# FAZA 2 — Backend API i Stripe

## Lokalno pokretanje

\`\`\`powershell
docker compose up -d
dotnet run --project src/OpenAmp.Api --launch-profile https
\`\`\`

Swagger je dostupan na \`https://localhost:7149/swagger\`.

## Autentikacija

| Metoda | Ruta | Opis |
| --- | --- | --- |
| POST | \`/api/auth/register\` | Registracija muzičara |
| POST | \`/api/auth/login\` | Prijava i izdavanje JWT/refresh tokena |
| POST | \`/api/auth/refresh\` | Rotacija refresh tokena |
| GET | \`/api/users/me\` | Profil prijavljenog korisnika |
| PUT | \`/api/users/me\` | Izmjena profila |

JWT se šalje kroz header:

\`\`\`http
Authorization: Bearer <access_token>
\`\`\`

## Rezervacije

| Metoda | Ruta | Opis |
| --- | --- | --- |
| GET | \`/api/reservations/availability\` | Slobodni slotovi sale za datum |
| POST | \`/api/reservations\` | Kreiranje i kalkulacija cijene |
| GET | \`/api/reservations/{id}\` | Detalji rezervacije |
| PUT | \`/api/reservations/{id}\` | Izmjena termina uz \`RowVersion\` |
| POST | \`/api/reservations/{id}/cancel\` | Otkazivanje i eventualni refund |

Vrijeme se šalje u UTC formatu, na primjer \`2026-08-10T16:00:00Z\`. Za izmjenu i otkazivanje treba poslati posljednju Base64 \`rowVersion\` vrijednost dobijenu iz API-ja.

Ukupna cijena je zbir cijene sale po satu, opreme po satu i kupljenih artikala. Servis provjerava radno vrijeme studija, članstvo u bendu, zalihe artikala te preklapanje sale i opreme.

## Stripe

Stripe ključevi se ne upisuju u Git. Prije pokretanja API-ja postaviti:

\`\`\`powershell
$env:Stripe__SecretKey='sk_test_...'
$env:Stripe__WebhookSecret='whsec_...'
$env:Stripe__Currency='eur'
dotnet run --project src/OpenAmp.Api --launch-profile https
\`\`\`

Kreiranje Payment Intenta:

\`\`\`http
POST /api/payments/reservations/{id}/payment-intent
Authorization: Bearer <access_token>
\`\`\`

API vraća \`clientSecret\` koji koristi frontend. Webhook endpoint je:

\`\`\`text
POST /api/webhooks/stripe
\`\`\`

Za lokalno testiranje sa Stripe CLI-em:

\`\`\`powershell
stripe listen --forward-to https://localhost:7149/api/webhooks/stripe --skip-verify
\`\`\`

Vrijednost \`whsec_...\` koju ispiše Stripe CLI treba postaviti kao \`Stripe__WebhookSecret\`.

Webhook provjerava \`Stripe-Signature\`, iznos i valutu, a \`payment_intent.succeeded\` mijenja status rezervacije u \`Plaćena\`. ID webhook događaja čuva se u bazi, pa ponovno slanje istog događaja ne obrađuje uplatu dva puta.

Pri otkazivanju plaćene rezervacije refund se obračunava prema pravilima studija. Seed politika vraća 100% najmanje 48 sati prije termina, 50% najmanje 24 sata prije, a nakon toga nema automatskog povrata.

## Konfiguracija za deployment

Obavezno postaviti sljedeće vrijednosti izvan repozitorija:

- \`ConnectionStrings__OpenAmp\`
- \`Jwt__Issuer\`
- \`Jwt__Audience\`
- \`Jwt__SigningKey\`
- \`Stripe__SecretKey\`
- \`Stripe__WebhookSecret\`
- \`Stripe__Currency\`

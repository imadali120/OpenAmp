# OpenAmp ERD — Faze 1–3

```mermaid
erDiagram
    ULOGA ||--o{ KORISNIK : ima
    KORISNIK ||--o{ REFRESH_TOKEN : posjeduje
    KORISNIK ||--o{ KORISNIK_INSTRUMENT : svira
    INSTRUMENT ||--o{ KORISNIK_INSTRUMENT : pripada
    KORISNIK ||--o{ BEND : osniva
    ZANR ||--o{ BEND : opisuje
    BEND ||--o{ CLAN_BENDA : sadrzi
    KORISNIK ||--o{ CLAN_BENDA : clanstvo
    INSTRUMENT ||--o{ CLAN_BENDA : uloga
    BEND ||--o{ POZIVNICA_BENDA : salje
    KORISNIK ||--o{ POZIVNICA_BENDA : poziva
    KORISNIK ||--o{ POZIVNICA_BENDA : prima
    STATUS_POZIVNICE ||--o{ POZIVNICA_BENDA : status

    KORISNIK o|--o{ STUDIO : vlasnik
    STUDIO ||--o{ SALA : sadrzi
    STATUS_SALE ||--o{ SALA : status
    SALA ||--o{ SALA_SLIKA : galerija
    KORISNIK ||--o{ MEDIJSKA_DATOTEKA : uploaduje
    MEDIJSKA_DATOTEKA o|--o| KORISNIK : profilna_slika
    MEDIJSKA_DATOTEKA o|--o| BEND : naslovna_slika
    MEDIJSKA_DATOTEKA o|--o| STUDIO : naslovna_slika
    MEDIJSKA_DATOTEKA o|--o{ SALA_SLIKA : sadrzaj
    SALA o|--o{ OPREMA : lokacija
    KATEGORIJA_OPREME ||--o{ OPREMA : kategorija
    STATUS_OPREME ||--o{ OPREMA : status
    STUDIO ||--o{ ARTIKAL : prodaje
    KATEGORIJA_ARTIKLA ||--o{ ARTIKAL : kategorija
    STATUS_ARTIKLA ||--o{ ARTIKAL : status

    SALA ||--o{ REZERVACIJA : termin
    BEND ||--o{ REZERVACIJA : rezervise
    KORISNIK ||--o{ REZERVACIJA : kreira
    STATUS_REZERVACIJE ||--o{ REZERVACIJA : status
    REZERVACIJA ||--o{ STAVKA_REZERVACIJE : sadrzi
    OPREMA o|--o{ STAVKA_REZERVACIJE : najam
    ARTIKAL o|--o{ STAVKA_REZERVACIJE : kupovina
    REZERVACIJA o|--o| RECENZIJA : rezultira
    KORISNIK ||--o{ RECENZIJA : ostavlja
    SALA ||--o{ RECENZIJA : prima

    KORISNIK {
        int Id PK
        string Username UK
        string Ime
        string Prezime
        string Email UK
        string PasswordHash
        string Telefon
        int UlogaId FK
    }
    REFRESH_TOKEN {
        int Id PK
        int KorisnikId FK
        string TokenHash UK
        datetime IsticeUtc
        datetime OpozvanUtc
    }
    BEND {
        int Id PK
        string Naziv
        int OsnivacId FK
        int ZanrId FK
    }
    CLAN_BENDA {
        int BendId PK_FK
        int KorisnikId PK_FK
        int InstrumentId FK
        string UlogaUBendu
    }
    MEDIJSKA_DATOTEKA {
        int Id PK
        string NazivDatoteke
        string ContentType
        binary Sadrzaj
        long Velicina
        int KreiraoKorisnikId FK
    }
    STUDIO {
        int Id PK
        string Naziv
        string VremenskaZona
        time RadnoVrijemeOd
        time RadnoVrijemeDo
        int RefundPuniPovratSati
        int RefundDjelimicniPovratSati
        int RefundDjelimicniPovratPostotak
    }
    SALA {
        int Id PK
        int StudioId FK
        string Naziv
        int Kapacitet
        decimal CijenaPoSatu
        int StatusSaleId FK
    }
    OPREMA {
        int Id PK
        string InventarskiBroj UK
        decimal CijenaNajmaPoSatu
        int KategorijaOpremeId FK
        int StatusOpremeId FK
        int SalaId FK
    }
    ARTIKAL {
        int Id PK
        string InventarskiBroj UK
        int KolicinaNaStanju
        int MinimalnaZaliha
        decimal CijenaKupovine
        int StudioId FK
    }
    REZERVACIJA {
        int Id PK
        int SalaId FK
        int BendId FK
        datetime TerminOdUtc
        datetime TerminDoUtc
        decimal UkupnaCijena
        int StatusRezervacijeId FK
        string StripePaymentIntentId UK
        string StripeRefundId
        decimal RefundiraniIznos
        rowversion RowVersion
    }
    STAVKA_REZERVACIJE {
        int Id PK
        int RezervacijaId FK
        int OpremaId FK
        int ArtikalId FK
        int Kolicina
        decimal JedinicnaCijena
        decimal UkupnaCijena
    }
    RECENZIJA {
        int Id PK
        int Ocjena
        string Komentar
        int KorisnikId FK
        int SalaId FK
        int RezervacijaId FK
    }
    STRIPE_WEBHOOK_DOGADJAJ {
        string Id PK
        string Tip
        datetime ObradjenUtc
    }
```

## Pravila integriteta

- `ClanBenda` i `KorisnikInstrument` koriste kompozitne primarne ključeve.
- Svaka stavka rezervacije referencira tačno jedan tip: `Oprema` ili `Artikal`.
- Ocjena recenzije je ograničena na raspon 1–5.
- `TerminDoUtc` mora biti nakon `TerminOdUtc`.
- `Rezervacija.RowVersion` je SQL Server `rowversion` concurrency token.
- Indeks `IX_Rezervacije_Sala_Termin` podržava atomsku provjeru preklapanja termina.
- Hash refresh tokena je jedinstven i stvarni token se nikada ne čuva u bazi.
- Username je normalizovan na mala slova i jedinstven; pozivnice za bend vežu se za postojećeg korisnika.
- JPEG, PNG i WebP slike do 5 MB čuvaju se u `MedijskeDatoteke`.
- ID Stripe webhook događaja je primarni ključ za idempotentnu obradu.

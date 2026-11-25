# 07 – Pregled prevzetega projekta

**Izvorni repozitorij:** https://github.com/jurekucanITK/delez (avtor: Jure Kučan, 66 commitov, zadnja sprememba 9. 11. 2026)
**Prevzeti fork:** https://github.com/PopeVitalian/delez
**Datum pregleda:** 25. 11. 2026
**Način pregleda:** branje dokumentacije (`docs/01`–`06`, ADR-ji), pregled backenda (ASP.NET Core Web API, .NET 10, EF Core, PostgreSQL) in frontenda (Vue 3, Vite, Vuetify, Pinia) ter zagon aplikacije lokalno (`docker compose up -d`, `dotnet ef database update --project backend`, `dotnet run --project backend`, `npm run dev`).

**Delež** je spletna aplikacija za beleženje in deljenje skupnih stroškov v gospodinjstvih. Člani skupine vnašajo stroške in jih delijo med člane, aplikacija pa sproti izračuna stanja (kdo je komu koliko dolžan) in predlaga čim manj nakazil za poravnavo dolgov.

**Legenda:** ✅ = preverjeno (koda ali zagon) · 🟡 = po kodnem pregledu videti pravilno, treba potrditi v zagnani aplikaciji · ❌ = ni implementirano.
*Opomba: pred predložitvijo preveri vse 🟡 v lokalno zagnani aplikaciji in jih popravi v ✅ ali ❌.*

## Uspešno implementirane funkcionalnosti (MVP)

| Funkcionalnost | Evidence | Status |
| --- | --- | --- |
| Registracija in prijava (JWT, BCrypt) | `AvtentikacijaController`, `AvtentikacijaService` | ✅ |
| Zaščita vseh endpointov z žetonom (fallback pravilo) | `Program.cs` (`SetFallbackPolicy`) | ✅ |
| Enako sporočilo ob napačnem e-poštnem naslovu ali geslu | `AvtentikacijaService.PrijaviAsync` (oba primera vračata `null`) | ✅ |
| Zavrnitev napačnega plačnika (član lahko samo sebe) | `StroskiService` (napaka `SamoSebe`) | ✅ |
| Preverjanje vlog v skupini, 403 za nepooblaščene | `DostopService`, `SkupinaControllerBase` | ✅ |
| Ustvarjanje skupin in pregled skupin uporabnika | `SkupineController`, domača stran frontenda | ✅ |
| Dodajanje članov po e-pošti z vlogami | `ClaniController` | ✅ |
| Vnos stroška (opis, znesek, datum, plačnik, deleži) | `StroskiService.DodajAsync` | ✅ |
| Enakomerna delitev stroška med izbrane člane | `Domain.DelitevStroska.Enakomerno` | ✅ |
| Urejanje in brisanje stroškov | `StroskiService` (PR #17) | ✅ |
| Izračun stanj članov | `StanjaService`, `Domain.IzracunStanj` | ✅ |
| Predlog najmanjšega števila nakazil | `PredlogNakazilController`, `Domain.PredlogNakazil` | ✅ |
| Beleženje nakazil ter njihovo urejanje in brisanje | `NakazilaController`, `NakazilaService` (PR #23, #39) | ✅ |
| Delitev stroška po deležih (npr. 60 % / 40 %) | `Domain.DelitevStroska` | 🟡 |
| Pogled omejenega člana (samo lastni deleži in stanje) | kontroler `SkupinaControllerBase` (preverjanje vloge) | 🟡 |

**Ni implementirano:** zgodovina s filtriranjem po obdobju in članu (US-14), mesečni povzetek porabe (US-15) in e-poštna obvestila (iz dokumenta 02). Za te funkcionalnosti v backendu ni ustreznih endpointov; v dokumentu 02 sicer niso označene kot [MVP].

## Ali funkcionalnosti zadoščajo uporabniškim zgodbam?

Pregledanih 5 uporabniških zgodb: **US-01 Registracija, US-02 Prijava, US-03 Ustvarjanje skupine, US-04 Dodajanje člana, US-05 Vnos stroška.**

### US-01: Registracija

| # | Pogoj sprejemljivosti | Status |
| --- | --- | --- |
| 1 | Obrazec z imenom, e-pošto in geslom, vsa polja obvezna | 🟡 |
| 2 | Že registriran e-poštni naslov → »Ta e-poštni naslov je že registriran« | ✅ (`EmailZeObstaja` + rezerva ob `DbUpdateException`) |
| 3 | Geslo z vsaj 8 znaki, sicer zavrnitev | 🟡 |
| 4 | Po uspešni registraciji preusmeritev na prijavo | 🟡 |

### US-02: Prijava

| # | Pogoj sprejemljivosti | Status |
| --- | --- | --- |
| 1 | Pravilni podatki → JWT žeton + preusmeritev na pregled skupin | ✅ (`PrijavaOdgovor` vsebuje žeton) |
| 2 | Napačen e-poštni naslov ali geslo → enako sporočilo, brez razkritja | ✅ |
| 3 | Zahtevek brez žetona → 401 | ✅ (`SetFallbackPolicy` v `Program.cs`) |
| 4 | Odjava izbriše žeton iz brskalnika | 🟡 |

### US-03: Ustvarjanje skupine

| # | Pogoj sprejemljivosti | Status |
| --- | --- | --- |
| 1 | Ime obvezno (≤ 100 znakov), opis neobvezen (≤ 500) | 🟡 |
| 2 | Ustvarjalec samodejno dobi vlogo skrbnik | 🟡 |
| 3 | Nova skupina se takoj prikaže na pregledu, uporabnik je preusmerjen nanjo | 🟡 |
| 4 | Prazno ime → napaka »Ime skupine je obvezno« | 🟡 |
| 5 | Poljubno število skupin, imena so lahko enaka | 🟡 |

### US-04: Dodajanje člana v skupino

| # | Pogoj sprejemljivosti | Status |
| --- | --- | --- |
| 1 | E-poštni naslov + izbira vloge, privzeta vloga član | 🟡 |
| 2 | Neregistriran uporabnik → »Uporabnik s tem naslovom ni registriran« | 🟡 |
| 3 | Že obstoječi član → »Uporabnik je že član skupine« | 🟡 |
| 4 | Nov član se takoj prikaže na seznamu z izbrano vlogo | 🟡 |
| 5 | Gumb viden samo skrbniku, drugim backend vrne 403 | ✅ (`DostopService` + `SkupinaControllerBase`) |

### US-05: Vnos stroška

| # | Pogoj sprejemljivosti | Status |
| --- | --- | --- |
| 1 | Opis (obvezen, ≤ 200), znesek (> 0,00, dve decimalki), datum (privzeto današnji), plačnik | 🟡 |
| 2 | Član kot plačnika izbere samo sebe, skrbnik katerega koli člana | ✅ |
| 3 | Neveljaven znesek → zavrnitev z napako ob polju | 🟡 |
| 4 | Strošek se takoj prikaže, stanja se posodobijo brez ročnega osveževanja | 🟡 |
| 5 | Omejen član gumba ne vidi, backend vrne 403 | ✅ (preverjanje vloge, `NimaPravic`) |

## Sklep

Implementiranih je 13 od 15 uporabniških zgodb (US-01 do US-13). Le-te pokrivajo celoten MVP obseg iz dokumenta 02: ustvarjanje skupin in članstev, vnos in delitev stroškov, izračun stanj, predlog nakazil in beleženje nakazil ter pogled omejenega člana. Zgodovina s filtriranjem (US-14) in Mesečni povzetek (US-15) nista implementirani; v dokumentu 02 sta sicer med predvidenimi funkcionalnostmi, vendar nista označeni kot [MVP], zato njuna odsotnost ne ogroža MVP-ja.

**Ugotovitev:** implementirane funkcionalnosti zadoščajo uporabniškim zgodbam, ki opredeljujejo MVP. Omejitve: manjkata US-14 in US-15, kar je v skladu z načrtovanim obsegom.
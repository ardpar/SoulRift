# Soul System

> **Status**: Designed
> **Author**: user + claude
> **Last Updated**: 2026-03-25
> **Implements Pillar**: Core Risk/Reward — tek kaynak etrafinda aktif karar verme

## 1. Overview

**Soul System**, SOULRIFT'in tek kaynagi olan Ruh'u (Soul) yoneten merkezi sistemdir. Ruh ayni anda oyuncunun cani, parasi ve guc kaynagi olarak isler. Dusman oldurmek Ruh kazandirir; hasar almak ve item satin almak Ruh tuketir. Ruh miktari %0–100 araliginda bir metre uzerinde takip edilir ve 5 farkli state'e (Hollow, Stable, Surging, Surge Warning, Overflow) bolunmustur. Her state, oyuncunun hasar carpanini, hareket hizini, dusman davranisini ve ekonomik avantajlarini degistirir. Oyuncu Soul System ile surekli ve aktif olarak etkilesir — her oldurmede, her hasarda ve her alisveriste Ruh seviyesi degisir ve bu degisim oyun deneyimini kokten donusturur. Soul System olmadan SOULRIFT'in risk/odul gerilimi, karakter farklilastirmasi ve item sinerji katmani var olamaz.

## 2. Player Fantasy

Oyuncu kendini **gucun esiginde bir kumarbaz** gibi hissetmeli. Her Ruh toplama ani kucuk bir zafer — ama ayni zamanda tehlikeye bir adim daha yaklasma. Surging'e ciktiginda "simdi gercekten guclu hissediyorum" hissi, Overflow'a gectiginde "bir vurusta olecegim ama tanri gibiyim" adrenalini. Hollow'a dustugunde "zayifim ama bu benim secimim, Hunger biriktiriyorum, hemen patlayacagim" stratejik sakinligi.

Referans hisler:
- **Brotato'nun "son wave'de 6 item sinerjisi patlamasi"** — ama SOULRIFT'te bu her 10 saniyede, mikro olcekte yasanir
- **Risk of Rain 2'nin "tanri modu" hissi** — Overflow'daki x3 hasar ani
- **Hades'in "Death Defiance bitti, son can" gerilimi** — Surge Warning'deki "simdi karar ver" penceresi

Soul System bir "arka planda calisan altyapi" degil, oyuncunun her saniye aktif olarak yonettigi, hissettigi ve karsilastigi bir sistemdir.

## 3. Detailed Design

### 3.1 Core Rules

**R1. Tek Kaynak Prensibi**
Ruh (Soul) oyundaki tek kaynaktir. Can, para ve guc carpani ayni degerden turetilir. Ayri HP veya coin degiskeni yoktur.

**R2. Ruh Metresi**
- Ruh, `0.0` ile `max_soul` (varsayilan `100.0`) arasinda bir float degerdir
- Run baslangicinda `50.0` (%50 — Stable state ortasi)
- `0.0`'a dustugunde oyuncu olur (permadeath, run sona erer)
- `max_soul` degeri karakter pasiflerine gore degisir (The Vessel: 150.0)

**R3. Ruh Kazanma**
- Dusman oldugunde yere **Soul Orb** duser
- Soul Orb'lar fiziksel pickup'tir — oyuncu uzerine yurumelidir
- Her dusman tipi sabit bir `soul_drop` degerine sahiptir (ScriptableObject)
- Soul Orb yere dustukten sonra `orb_lifetime` (varsayilan 8 sn) sonra kaybolur
- Orb'lar toplama animasyonu oynar (kisa cekim efekti, ~0.3 sn)
- Hunger System aktifken toplama carpani uygulanir (bkz. Hunger System GDD)

**R4. Ruh Kaybetme — Hasar**
- Oyuncu hasar aldiginda Ruh kaybeder
- Kayip formulu: `soul_loss = base_soul_loss + (damage_taken * soul_loss_ratio)`
- `base_soul_loss` varsayilan: `3.0`
- `soul_loss_ratio` varsayilan: `0.5`
- Ornek: 10 hasar → 3.0 + (10 * 0.5) = **8.0 Ruh kaybi**
- Kayip sonrasi kisa iframes (varsayilan 0.5 sn) — art arda hasar onlenir

**R5. Ruh Kaybetme — Harcama**
- Shop'ta item satin almak Ruh tuketir
- Harcanan miktar item'in `soul_cost` degeridir
- Hollow state'de fiyatlar %30 ucuz (bkz. Shop System GDD)

**R6. State Degisimi**
- Ruh miktari degistiginde mevcut state yeniden hesaplanir
- State degisimi anlik — gecis animasyonu VFX katmaninda islenir
- State degistiginde `OnSoulStateChanged(oldState, newState)` event'i tetiklenir
- Tum bagli sistemler bu event'e subscribe olur

### 3.2 States and Transitions

#### State Tablosu

| State | Aralik | Hasar Carpani | Hiz Carpani | Ozel Efektler |
|-------|--------|---------------|-------------|---------------|
| **Hollow** | 0% – 25% | x0.5 | x0.7 | Item fiyati x0.7, Hunger stack birikir |
| **Stable** | 25% – 60% | x1.0 | x1.0 | Yok — denge bolgesi |
| **Surging** | 60% – 85% | x1.75 | x1.0 | Dusmanlar oyuncuyu hedef alir (targeting AI) |
| **Surge Warning** | 85% – 90% | x1.75 | x1.0 | Gorsel/ses uyari, 3–4 sn karar penceresi |
| **Overflow** | 90% – 100% | x3.0 | x1.0 | 1 vurusta olum, elite dusman spawn |

#### Gecis Kurallari

- Gecisler **sadece** Ruh degerinin state araligini gecmesiyle tetiklenir
- Bir frame'de birden fazla state gecisi olabilir (ornegin: Hollow'dan direkt Surging'e — Hunger patlama ani)
- Gecis sirasi: once yeni state hesaplanir, sonra `OnSoulStateChanged` tetiklenir
- State gecisi geri alinabilir — Surging'den Stable'a dusmek normal akis

#### Gecis Matrisi

| Kaynak → Hedef | Tetikleyici | Notlar |
|----------------|-------------|--------|
| Hollow → Stable | Ruh >= 25% | Hunger stack tuketimi veya normal Ruh kazanimi |
| Stable → Hollow | Ruh < 25% | Hasar alma sonucu |
| Stable → Surging | Ruh >= 60% | Ruh toplama sonucu |
| Surging → Stable | Ruh < 60% | Hasar alma veya item satin alma |
| Surging → Surge Warning | Ruh >= 85% | Ruh toplama devam ederse |
| Surge Warning → Surging | Ruh < 85% | Oyuncu Ruh harcar (item al, kasitli hasar) |
| Surge Warning → Overflow | Ruh >= 90% | Oyuncu Ruh toplamaya devam eder |
| Overflow → Surge Warning | Ruh < 90% | Ruh harcama ile dusulebilir |
| Overflow → Stable/Hollow | Ruh < 60% / < 25% | Buyuk harcama veya hasar (nadir) |
| Herhangi → Olum | Ruh = 0% | Run sona erer |

#### Surge Warning Ozel Kurali

Surge Warning'e girildiginde bir `warning_timer` baslar (varsayilan 3.5 sn):
- Timer boyunca oyuncu Ruh harcayarak Surging'e donebilir
- Timer sona erdiginde VE Ruh hala >= 85% ise: state otomatik olarak Overflow'a GECMEZ — sadece Ruh >= 90% oldugunda Overflow'a gecer
- Timer gorsel urgency yaratir ama mekanik olarak state gecisini zorlamaz
- The Vessel bu timer'a +2 sn ekler (toplam 5.5 sn)

#### Karakter Pasif Etkileri (State Uzerinde)

| Karakter | State Etkisi |
|----------|-------------|
| The Vessel | `max_soul` = 150, Surge Warning timer +2 sn |
| The Hollow | Hollow'da hasar carpani x1.0 (normal, x0.5 degil), Hunger max stack 5 |
| The Fractured | Ruh seviyesi her 5 sn'de +/- rastgele 3-8 dalgalanir, Cursed slot her zaman acik |

### 3.3 Interactions with Other Systems

#### Upstream (Soul System'in ihtiyac duydugu veriler)

| Sistem | Veri Akisi | Arayuz |
|--------|-----------|--------|
| **Player Movement** | Oyuncu pozisyonu (pickup mesafesi icin) | `Transform` referansi |
| **Collision/Pickup** | Oyuncunun Soul Orb'a dokundugu ani | `OnTriggerEnter2D` → `SoulManager.AddSoul(amount)` |
| **Combat/Shooting** | Dusman olduruldu eventi (soul drop icin) | `OnEnemyKilled(enemyData)` → Soul Orb spawn |
| **Health/Damage** | Oyuncunun hasar aldigi an ve miktar | `OnPlayerDamaged(damage)` → `SoulManager.RemoveSoul(soul_loss)` |

#### Downstream (Soul System'in sagladigi veriler)

| Sistem | Veri Akisi | Arayuz |
|--------|-----------|--------|
| **Hunger System** | Mevcut state (Hollow mu?), Ruh miktari | `OnSoulStateChanged`, `GetCurrentState()`, `GetSoulPercent()` |
| **Surge Warning** | Surge Warning'e giris/cikis | `OnSoulStateChanged` (state == SurgeWarning) |
| **Shop System** | Mevcut Ruh miktari (fiyat hesabi, satin alma) | `GetCurrentSoul()`, `RemoveSoul(cost)`, `GetCurrentState()` |
| **Item System** | State bilgisi (item etkileri icin) | `GetCurrentState()`, `OnSoulStateChanged` |
| **Cursed Item System** | State >= Surging mi? (slot acma) | `GetCurrentState()` |
| **Character System** | `max_soul`, state modifikatorleri | `SetMaxSoul(value)`, `SetStateModifier(state, modifier)` |
| **Enemy AI** | State bilgisi (Surging targeting) | `GetCurrentState()` — AI sadece Surging+ ise aggro artirir |
| **VFX/Aura System** | State ve Ruh yuzde degeri (aura rengi, intensity) | `OnSoulStateChanged`, `GetSoulPercent()` |
| **Camera System** | State (Surge Warning vignette, Overflow distortion) | `OnSoulStateChanged` |
| **UI/HUD** | Ruh miktari, yuzde, state (metre gosterimi) | `GetCurrentSoul()`, `GetSoulPercent()`, `GetCurrentState()`, `OnSoulChanged(newValue)` |
| **Audio System** | State (Surge Warning ugultusu, Overflow sesleri) | `OnSoulStateChanged` |
| **Run Manager** | Olum (Ruh = 0) | `OnPlayerDeath` event |

#### Arayuz Kontrati (ISoulProvider)

Soul System asagidaki public arayuzu saglar:

```
// Okuma
float GetCurrentSoul()
float GetSoulPercent()          // 0.0 – 1.0
float GetMaxSoul()
SoulState GetCurrentState()     // enum: Hollow, Stable, Surging, SurgeWarning, Overflow

// Yazma
void AddSoul(float amount)      // Ruh ekleme (pickup, Hunger bonus)
void RemoveSoul(float amount)   // Ruh cikarma (hasar, harcama)
void SetMaxSoul(float value)    // Karakter pasifi

// Eventler
event Action<SoulState, SoulState> OnSoulStateChanged  // (oldState, newState)
event Action<float> OnSoulChanged                       // her deger degisiminde
event Action OnPlayerDeath                              // Ruh = 0
```

## 4. Formulas

### F1. State Hesaplama
```
soul_percent = current_soul / max_soul

if soul_percent < 0.25:      state = Hollow
elif soul_percent < 0.60:    state = Stable
elif soul_percent < 0.85:    state = Surging
elif soul_percent < 0.90:    state = SurgeWarning
else:                        state = Overflow
```

**Degiskenler:**
- `current_soul`: float, 0.0 – max_soul
- `max_soul`: float, varsayilan 100.0 (The Vessel: 150.0)
- `soul_percent`: float, 0.0 – 1.0

### F2. Hasar Carpani
```
effective_damage = base_damage * state_damage_multiplier * item_modifiers
```

| State | `state_damage_multiplier` |
|-------|--------------------------|
| Hollow | 0.5 (The Hollow: 1.0) |
| Stable | 1.0 |
| Surging | 1.75 |
| Surge Warning | 1.75 |
| Overflow | 3.0 |

**Ornek (erken oyun):** base_damage = 10, Surging → 10 * 1.75 = 17.5
**Ornek (gec oyun, item bonus):** base_damage = 25, Overflow, +50% item → 25 * 3.0 * 1.5 = 112.5

### F3. Hiz Carpani
```
effective_speed = base_speed * state_speed_multiplier
```

| State | `state_speed_multiplier` |
|-------|--------------------------|
| Hollow | 0.7 |
| Stable | 1.0 |
| Surging | 1.0 |
| Surge Warning | 1.0 |
| Overflow | 1.0 |

### F4. Ruh Kayip Hesabi (Hasar)
```
soul_loss = base_soul_loss + (damage_taken * soul_loss_ratio)
```

**Degiskenler:**
- `base_soul_loss`: float, varsayilan 3.0 (aralik: 1.0 – 8.0)
- `damage_taken`: float, dusman hasar degeri
- `soul_loss_ratio`: float, varsayilan 0.5 (aralik: 0.2 – 1.0)

**Ornek hesaplamalar:**
- Kucuk dusman (5 hasar): 3.0 + (5 * 0.5) = **5.5 Ruh kaybi**
- Orta dusman (15 hasar): 3.0 + (15 * 0.5) = **10.5 Ruh kaybi**
- Boss vurus (30 hasar): 3.0 + (30 * 0.5) = **18.0 Ruh kaybi**

### F5. Overflow Olum Kurali
```
if state == Overflow AND damage_taken > 0:
    player_dies = true  // 1 vurusta olum, hasar miktari onemli degil
```

### F6. Soul Orb Drop Miktari
```
soul_drop = enemy_base_soul_drop * hunger_multiplier * harvester_item_bonus
```

| Dusman Tipi | `enemy_base_soul_drop` |
|-------------|----------------------|
| Kucuk | 2.0 – 4.0 |
| Orta | 5.0 – 8.0 |
| Buyuk | 10.0 – 15.0 |
| Elite | 15.0 – 25.0 |
| Boss | 30.0 – 50.0 |

## 5. Edge Cases

| # | Durum | Ne Olur | Gerekce |
|---|-------|---------|---------|
| E1 | Ruh tam 0.0'a duser | Oyuncu olur, run sona erer. `OnPlayerDeath` tetiklenir. | Permadeath — roguelite temeli |
| E2 | Ruh max_soul'un uzerine cikmaya calisir | `current_soul = min(current_soul + amount, max_soul)` — clamp edilir | Overflow %100'de tavanlanir |
| E3 | Overflow'da herhangi bir hasar | Hasar miktari farketmez, oyuncu aninda olur (F5) | Overflow risk/reward'un odeme noktasi |
| E4 | Overflow'da iframes | iframes Overflow'da da calisir (0.5 sn). Ayni frame'de 2 hasar almak engellenir. | Art arda hasar unfair olur |
| E5 | Ayni frame'de Ruh kazanma + kaybetme | Once kayip uygulanir, sonra kazanc. Net sonuc hesaplanir, state bir kez guncellenir. | Ornek: hasar al (-8) + orb topla (+5) = net -3 |
| E6 | Tek frame'de birden fazla state atlama | Izinli. Ornek: Hollow'da Hunger patlama → +45 Ruh → direkt Surging. Ara state eventleri atlanir, sadece son state event'i tetiklenir. | Hunger patlamasi akici hissettirmeli |
| E7 | Surge Warning'deyken shop acilir | Shop acilir, Surge Warning timer DURAKLAR. Shop kapatilinca timer devam eder. | Shop'ta karar verirken timer baskisi olmamali |
| E8 | Overflow'da shop acilir | Shop acilir + Cursed Item slotu acik. Overflow tehlikesi devam eder (wave bittiginde shop acildigi icin dogrudan olum riski yok). | Cursed Item firsati anlamli |
| E9 | Soul Orb yerde, oyuncu Overflow'da | Orb hala toplanabilir. `AddSoul` cagirilir ama max_soul clamp'i nedeniyle deger degismez. | Orb'lari "gecmek" gerekmiyor, otomatik clamp yeterli |
| E10 | The Fractured'in rastgele dalgalanmasi state sinirinda | Dalgalanma sonucu state degisirse normal `OnSoulStateChanged` tetiklenir. Overflow'a gecis dahil — Fractured riski budur. | Chaos build kimliginin parcasi |
| E11 | max_soul degisikligi (The Vessel pasifi) | Run baslangicinda bir kez set edilir. Ortasinda degismez (item ile degisebilir — o zaman mevcut yuzde korunur: `current_soul = percent * new_max_soul`). | Yuzde korunmazsa state ani seker |
| E12 | Negatif soul_loss | Asla olamaz. `RemoveSoul(amount)` icinde `amount = max(0, amount)` kontrolu. | Negatif kayip = kazanc exploit'u onlenir |

## 6. Dependencies

### Hard Dependencies (Olmadan calismaz)

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Player Movement** | Upstream | Transform pozisyonu | Orb toplama mesafesi icin |
| **Collision/Pickup** | Upstream | OnTriggerEnter2D | Soul Orb fiziksel pickup tetikleyicisi |

### Soft Dependencies (Gelistirir ama olmadan da calisir)

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Combat/Shooting** | Upstream | OnEnemyKilled | Olmazsa Soul Orb spawn olmaz — test icin manuel spawn yeterli |
| **Health/Damage** | Upstream | OnPlayerDamaged | Olmazsa Ruh kaybi olmaz — test icin manuel RemoveSoul yeterli |
| **Hunger System** | Downstream | OnSoulStateChanged | Hollow state'de aktive olur |
| **Surge Warning** | Downstream | OnSoulStateChanged | SurgeWarning state'de aktive olur |
| **Shop System** | Downstream | GetCurrentSoul, RemoveSoul | Satin alma ve fiyat hesabi |
| **Item System** | Downstream | GetCurrentState | State bazli item efektleri |
| **Enemy AI** | Downstream | GetCurrentState | Surging targeting |
| **VFX/Aura** | Downstream | OnSoulStateChanged, GetSoulPercent | Gorsel feedback |
| **Camera** | Downstream | OnSoulStateChanged | Screen efektleri |
| **UI/HUD** | Downstream | OnSoulChanged, GetCurrentState | Metre gosterimi |
| **Audio** | Downstream | OnSoulStateChanged | Ses feedback |
| **Run Manager** | Downstream | OnPlayerDeath | Olum → game over |

### Bidirectional Referanslar
- Hunger System GDD, Soul System'in `GetCurrentState()` ve `AddSoul()` arayuzunu referans almali
- Shop System GDD, Soul System'in `RemoveSoul()` ve Hollow fiyat indirimi kuralini referans almali
- Character System GDD, Soul System'in `SetMaxSoul()` arayuzunu referans almali

## 7. Tuning Knobs

Tum degerler ScriptableObject (`SoulSystemConfig`) uzerinden yonetilir. Kod degisikligi gerektirmez.

| Parametre | Varsayilan | Guvenli Aralik | Cok Dusukse | Cok Yuksekse | Etkilesim |
|-----------|------------|----------------|-------------|-------------|-----------|
| `max_soul` | 100.0 | 50–200 | State gecisleri cok hizli, kaotik | State gecisleri cok yavas, siradan | Karakter pasifi (Vessel: 150) |
| `starting_soul_percent` | 0.50 | 0.20–0.70 | Hollow'da baslar, hemen baski | Surging'de baslar, cok guvenli | Run baslangic deneyimi |
| `hollow_threshold` | 0.25 | 0.15–0.35 | Hollow cok dar, nadir | Hollow cok genis, cok sikici | Hunger System erisimi |
| `surging_threshold` | 0.60 | 0.50–0.70 | Guc erken baslar, kolay | Guce erismek zor, frustrating | Item build hizi |
| `warning_threshold` | 0.85 | 0.80–0.92 | Warning penceresi genis, kolay | Warning cok dar, atlanir | Surge Warning suresi |
| `overflow_threshold` | 0.90 | 0.85–0.95 | Overflow'a girmek kolay, tehlikeli | Overflow'a girmek zor, guvenli | Risk/reward dengesi |
| `base_soul_loss` | 3.0 | 1.0–8.0 | Hasar onemli degil | Kucuk hasar bile yikici | Health/Damage sistemi |
| `soul_loss_ratio` | 0.5 | 0.2–1.0 | Buyuk vuruslar az etkili | Her vurus cok agir | Boss zorlugu |
| `iframes_duration` | 0.5 | 0.2–1.0 | Art arda hasar mumkun | Uzun dokunulmazlik, cok guvenli | Combat tempo |
| `orb_lifetime` | 8.0 | 4.0–15.0 | Orb'lar hizli kaybolur, baski | Orb'lar cok uzun kalir, risk yok | Toplama aciliyeti |
| `warning_timer` | 3.5 | 2.0–6.0 | Karar suresi az, panik | Cok rahat, gerilim yok | Vessel +2 sn pasifi |
| `hollow_speed_mult` | 0.7 | 0.5–0.9 | Hollow'da neredeyse hareketsiz | Hollow hiz cezasi onemli degil | Kacis zorlugu |

## 8. Acceptance Criteria

### Fonksiyonel Testler

| # | Test | Beklenen Sonuc | Oncelik |
|---|------|----------------|---------|
| T1 | Run basladiginda Ruh = %50 | State = Stable, metre dogru gosterir | P0 |
| T2 | Soul Orb toplama | Ruh miktari `soul_drop` kadar artar, OnSoulChanged tetiklenir | P0 |
| T3 | Hasar alma | Ruh miktari `base_soul_loss + damage * ratio` kadar azalir | P0 |
| T4 | Ruh 0'a dusme | OnPlayerDeath tetiklenir, run sona erer | P0 |
| T5 | Ruh %24 → %26 (Hollow → Stable gecisi) | OnSoulStateChanged(Hollow, Stable) tetiklenir | P0 |
| T6 | Ruh %59 → %61 (Stable → Surging gecisi) | OnSoulStateChanged(Stable, Surging) tetiklenir | P0 |
| T7 | Ruh %84 → %86 (Surging → SurgeWarning) | Warning timer baslar, event tetiklenir | P0 |
| T8 | Ruh %89 → %91 (SurgeWarning → Overflow) | Overflow state aktif, 1-hit-kill aktif | P0 |
| T9 | Overflow'da hasar alma | Hasar miktari ne olursa olsun oyuncu olur | P0 |
| T10 | Overflow'da iframes | Ilk hasar alindiktan sonra 0.5 sn icerisinde ikinci hasar engellenir | P1 |
| T11 | max_soul uzerine Ruh toplama | Ruh clamp edilir, max_soul'u gecmez | P1 |
| T12 | Hollow'da hasar carpani | Effective damage = base * 0.5 | P0 |
| T13 | Surging'de hasar carpani | Effective damage = base * 1.75 | P0 |
| T14 | Overflow'da hasar carpani | Effective damage = base * 3.0 | P0 |
| T15 | Hollow'da hiz carpani | Effective speed = base * 0.7 | P0 |
| T16 | Ayni frame'de kazanc + kayip | Once kayip uygulanir, sonra kazanc. State bir kez guncellenir. | P1 |
| T17 | Coklu state atlama (Hollow → Surging) | Tek OnSoulStateChanged(Hollow, Surging) tetiklenir, ara state yok | P1 |

### Performans Butcesi

| Metrik | Butce |
|--------|-------|
| State hesaplama | < 0.01 ms / frame |
| OnSoulChanged event dagitimi | < 0.1 ms / tetiklenme |
| Soul Orb object pool boyutu | 50 prealocated |
| Bellek kullanimi | < 1 KB (SoulManager instance) |

### Playtest Kriterleri (Hafta 2)

| # | Kriter | Olcum |
|---|--------|-------|
| P1 | Oyuncu state degisimlerini fark ediyor | Playtest sonrasi soru: "Kac farkli guc durumu fark ettin?" — hedef 3+ |
| P2 | Surge Warning gerginlik yaratiyor | Playtest gozlemi: oyuncu Warning'de duraksiyor mu? |
| P3 | Overflow risk/reward hissettiriyor | Playtest: oyuncu Overflow'da olup "tekrar yaparim" diyor mu? |
| P4 | State gecisleri cok hizli/yavas degil | Ortalama bir wave'de oyuncu 2–4 state gecisi yasiyor |

## 9. Visual/Audio Requirements

### Gorsel Feedback (State Bazli)

| State | Aura Rengi | Aura Yogunlugu | Ek Gorsel Efekt |
|-------|-----------|----------------|-----------------|
| Hollow | Gri (#6B7A99), soluk | %20 opacity | Hunger stack partikulleri (kucuk ruhani orblar) |
| Stable | Beyaz (#C8D0E0), sakin | %40 opacity | Yok — temiz gorunum |
| Surging | Altin (#F0B429), parlak | %70 opacity | Energy trail (karakter arkasinda iz) |
| Surge Warning | Turuncu (#FB923C), titresim | %85 opacity, pulse | Ekran kenari turuncu vignette (CameraSystem) |
| Overflow | Kirmizi (#E05252), parilti | %100 opacity, glow | Ekran distortion + chromatic aberration |

### Gecis Animasyonlari
- State gecisinde kisa flash efekti (0.15 sn) — yeni state rengiyle
- Hollow → Stable: yukari dogru partikul burst (toparlanma hissi)
- Stable → Surging: altin halka yayilmasi (guc hissi)
- SurgeWarning → Overflow: kirmizi sarsinti (tehlike hissi)

### Ses Gereksinimleri

| Olay | Ses Tipi | Notlar |
|------|----------|--------|
| Soul Orb toplama | Kisa, tatmin edici "pling" | Pitch toplanan miktara gore artar |
| State yukselis gecisi | Ascending chime | Her state icin farkli pitch |
| State dusus gecisi | Descending tone | Kayip hissi |
| Surge Warning baslangici | Dusuk frekanslı ugultu | Surekliler, Warning boyunca devam eder |
| Surge Warning timer son 1 sn | Kalp atisi efekti | Hizlanan tempo |
| Overflow giris | Derin bass hit + reverb | "Simdi tehlikedesin" sinyali |
| Olum (Ruh = 0) | Cam kirilma + ruh cikis sesi | Final, kesin |

## 10. UI Requirements

### Soul Metre (Surekli Gorunen)

- Ekranin alt ortasinda yatay bar
- Doluluk: `soul_percent` ile orantili
- Renk: mevcut state'in aura rengiyle eslenik
- State esik cizgileri: %25, %60, %85, %90 isaretli (ince beyaz cizgi)
- State ismi metrenin ustunde kucuk text: "HOLLOW" / "STABLE" / "SURGING" / "WARNING" / "OVERFLOW"
- Ruh degistiginde bar uzerinde kisa flash animasyonu (kazanc: beyaz flash, kayip: kirmizi flash)

### Hunger Stack Gostergesi

- Soul metrenin sol tarafinda 3 (veya 5) kucuk orb ikonu
- Dolu stack: parlak ruhani renk
- Bos stack: soluk/gri
- Stack dolunca pulse animasyonu — "simdi oldur" sinyali

### Surge Warning Overlay

- Warning state'de ekran kenarlarinda turuncu vignette
- Timer gostergesi: metrenin ustunde kucuk countdown (3.5... 3.0... 2.5...)
- Son 1 saniyede countdown kirmiziya doner ve buyur

### State Degisim Bildirimi

- State gecisinde ekranin ortasinda kisa (0.5 sn) buyuk text: "SURGING!" / "OVERFLOW!" vb.
- Sadece ana gecisler icin (Hollow→Stable gibi kucuk gecislerde gosterme)

## 11. Open Questions

| # | Soru | Sahip | Hedef Cozum |
|---|------|-------|-------------|
| Q1 | Soul Orb'larin magnet sistemi eklemeli miyiz ileriki versiyonlarda? Manuel toplama yeterli mi? | Game Designer | Hafta 2 playtest sonrasi |
| Q2 | The Fractured'in rastgele dalgalanmasi Overflow'da aninda olume yol acacak mi? Koruma mekanizmasi gerekli mi? | Game Designer | Character System GDD yazilirken |
| Q3 | Overflow'da iframes suresi farkli mi olmali (daha kisa)? | Balans | Hafta 2 playtest |
| Q4 | Item'lar state esiklerini modifiye edebilmeli mi? (orn: bir item Surging esigini %55'e dusurur) | Systems Designer | Item System GDD yazilirken |
| Q5 | Coop/multiplayer eklenmesi durumunda Soul System nasil calisir? Her oyuncunun ayri Soul'u mu? | Technical | Post-MVP kapsam karari |

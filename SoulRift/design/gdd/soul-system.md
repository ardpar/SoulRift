# Soul System — Core Mechanic

> **Status**: Tasarlandi
> **Son Guncelleme**: 2026-03-25
> **Katman**: Core
> **Oncelik**: MVP
> **Sutun**: Tek Kaynak Sonsuz Karar / Risk Odul Gerilimi / Her State Degerli

## Genel Bakis

**Ruh (Soul)**, oyundaki tek kaynak. Can, para ve guc kaynagi ayni anda. Dusman oldurmek Ruh verir. Hasar almak Ruh tuketir. Item satin almak Ruh tuketir. Miktarina gore oyuncu tamamen farkli bir karaktere donusur. Oyunun diger tum sistemleri bu sistemin uzerine insa edilir.

## Oyuncu Fantezisi

Gucun bedeli var. Ne kadar Ruh toplarsan o kadar guclu olursun — ama o kadar da kirilagansin. Her an "bir adim daha mi, yoksa geri mi cekileyim?" sorusu. Dusuk Ruh'ta bile stratejik bir cikis yolu var (Hunger). Oyuncu hicbir zaman caresiz degil.

## Detayli Tasarim

### Temel Kurallar

1. Oyuncu `CurrentSoul` (float, 0 - MaxSoul) degerine sahiptir
2. `MaxSoul` varsayilan 100. Karakter pasiflerine gore degisir (The Vessel: 150)
3. `SoulPercent = CurrentSoul / MaxSoul` degeri state'i belirler
4. Ruh kazanim kaynaklari: dusman oldurmek, Hunger stack patlama, ozel item efektleri
5. Ruh kayip kaynaklari: hasar almak, item satin almak, ozel item dezavantajlari
6. Ruh 0'in altina dusmez. 0'da oyuncu olmez — Hollow state'e girer.
7. Ruh MaxSoul'u asamaz. MaxSoul'da Overflow state'e girer.

### State ve Gecisler

| State | Aralik (SoulPercent) | Hasar Carpani | Hiz Carpani | Ozel Etki |
|-------|---------------------|---------------|-------------|-----------|
| **Hollow** | 0.00 – 0.25 | 0.5x | 0.8x | Item fiyati %30 ucuz, Hunger stack birikir |
| **Stable** | 0.25 – 0.60 | 1.0x | 1.0x | Denge bolgesi, ozel efekt yok |
| **Surging** | 0.60 – 0.85 | 1.75x | 1.0x | Dusmanlar oyuncuyu hedef alir, gorsel efektler |
| **Surge Warning** | 0.85 – 0.90 | 1.75x | 1.0x | 3-4 sn karar penceresi, gorsel/ses uyari |
| **Overflow** | 0.90 – 1.00 | 3.0x | 1.0x | 1 vurusta olum, Elite dusman spawn |

#### State Gecis Kurallari

```
Hollow ←→ Stable: SoulPercent 0.25 sinirini gectiginde
Stable ←→ Surging: SoulPercent 0.60 sinirini gectiginde
Surging → Surge Warning: SoulPercent 0.85'e ulastiginda Warning timer baslar
Surge Warning → Overflow: Warning timer bittiginde hala >= 0.85 ise
Surge Warning → Surging: Warning timer sirasinda SoulPercent < 0.85'e dustugunde
Overflow → Surging: SoulPercent < 0.85'e dustugunde (hasar alarak veya Ruh harcayarak)
Herhangi state → Hollow: SoulPercent < 0.25'e dustugunde (hasar alarak)
```

#### Gecis Hysteresis

State gecislerinde 0.02 (2%) hysteresis uygulanir. Ornek: Hollow→Stable gecisi 0.25'te olur ama Stable→Hollow geri donusu 0.23'te olur. Bu, sinirda titresimi (flickering) onler.

### Ruh Kazanim Sistemi

| Kaynak | Miktar | Not |
|--------|--------|-----|
| Normal dusman oldurmek | `EnemyData.SoulReward` (varsayilan 3-8) | Dusman tipine gore degisir |
| Elite dusman oldurmek | `EnemyData.SoulReward * 3` | Elite'ler Overflow'da spawn olur |
| Boss oldurmek | `EnemyData.SoulReward * 10` | Buyuk Ruh artisi |
| Hunger stack patlama | `killReward * 3 * stackCount` | Maks: 3x3=9x normal |
| Ruh Orbu toplama | Otomatik (yakinlik radius) | Orb'ler dusman oldugunde duser |

### Ruh Kayip Sistemi

| Kaynak | Miktar | Not |
|--------|--------|-----|
| Dusman hasari | `EnemyData.Damage` | Dogrudan Ruh'tan duser |
| Item satin alma | `ItemData.Cost` (Hollow'da x0.7) | Shop sirasinda |
| Cursed item satin alma | `ItemData.Cost * 2` (Fractured: x1) | Surging/Overflow'da |
| Ozel item dezavantajlari | Degisken | Item'a ozel |

### Hollow Hunger Mekanigi

Death spiral sorununu cozer. Hollow state artik bir ceza degil — **kasitli olarak girilip cikilan bir ritim.**

#### Hunger Kurallari

1. Hollow state'e girildiginde `hungerTimer` baslar
2. Her `HungerStackInterval` (2 sn) hayatta kalindiginda `hungerStacks += 1`
3. `hungerStacks` maksimum `MaxHungerStacks` (3, The Hollow: 5)
4. Herhangi bir dusman olduruldugunde, stack > 0 ise:
   - `soulGain = baseKillReward * HungerMultiplier * hungerStacks`
   - `hungerStacks = 0` (tum stackler tuketilir)
   - `hungerTimer` resetlenir
5. Stack biriktikce gorsel partikul yogunlugu artar (1 stack = az, 3 stack = yogun)
6. Hunger stack'ler sadece Hollow state'de birikir. State degisince timer durur ama stackler korunur.

#### Hunger Ornek Hesaplamalar

```
Normal dusman = 5 Ruh odul
Hollow'da 3 stack biriktir, oldur:
  soulGain = 5 * 3 * 3 = 45 Ruh

The Hollow karakteri, 5 stack:
  soulGain = 5 * 3 * 5 = 75 Ruh (MaxSoul'un %75'i!)
```

### Overflow Window — Surge Warning

Overflow artik "kazara girilen tehlike" degil, **bilincli alinan bir karar.**

#### Warning Kurallari

1. SoulPercent >= 0.85 oldugunda `warningTimer` baslar (`WarningDuration` = 3.5 sn)
2. Timer boyunca gorsel (turuncu vignette) ve ses (ugultu) feedback verilir
3. Timer sirasinda oyuncu Ruh harcayabilir (dusmanlara carparak veya ozel yetenekle)
4. Timer bittiginde SoulPercent hala >= 0.85 ise → Overflow'a gecis
5. Timer sirasinda SoulPercent < 0.85'e dusturulurse → Surging'e geri don, timer iptal
6. The Vessel karakteri: `WarningDuration + 2 sn = 5.5 sn`
7. Overflow'da Elite dusman spawn edilir (Wave Manager'a sinyal gonderilir)

### Diger Sistemlerle Etkilesim

| Sistem | Bu Sistem Verir | Bu Sistem Alir |
|--------|----------------|----------------|
| **Data Manager** | — | SoulStateData, HungerData (esik degerleri, carpanlar) |
| **Combat/Weapon** | Hasar carpanini (state'e gore) | Hasar olaylari (Ruh kaybi tetikler) |
| **Enemy System** | — | Dusman olum olaylari (Ruh kazanimi tetikler) |
| **Item System** | Mevcut SoulPercent | Item efektleri (Ruh degistirebilir) |
| **Shop System** | Mevcut state, SoulPercent | Ruh harcama (item satin alma) |
| **Character System** | — | Pasif modifikasyonlar (esikler, carpanlar) |
| **Wave Manager** | Elite spawn sinyali (Overflow) | — |
| **Soul VFX** | Mevcut state, hungerStacks | — |
| **HUD/UI** | SoulPercent, state, hungerStacks | — |
| **Audio System** | State degisim olaylari, Warning timer | — |
| **Camera System** | Overflow girisi olayi | — |

### Event Sistemi

```csharp
// Diger sistemlerin dinledigi olaylar
event Action<SoulState, SoulState> OnSoulStateChanged;  // (onceki, yeni)
event Action<float> OnSoulValueChanged;                  // normalize deger (0-1)
event Action<int> OnHungerStackChanged;                  // stack sayisi
event Action OnHungerConsumed;                           // stackler tuketildi
event Action OnOverflowEntered;                          // Overflow'a girildi
event Action OnWarningStarted;                           // Warning timer basladi
event Action OnPlayerDeath;                              // Overflow'da hasar alindi
```

## Formuller

### State Belirleme

```
SoulPercent = CurrentSoul / MaxSoul

if SoulPercent < HollowThreshold (0.25):     state = Hollow
elif SoulPercent < StableThreshold (0.60):    state = Stable
elif SoulPercent < SurgingThreshold (0.85):   state = Surging
elif SoulPercent < OverflowThreshold (0.90):  state = SurgeWarning (timer baslatir)
else:                                          state = Overflow
```

### Hasar Hesaplama (Combat'a cikti)

```
finalDamage = baseDamage * stateDamageMultiplier * itemBonuses * characterPassiveMultiplier

stateDamageMultiplier:
  Hollow    = 0.5  (The Hollow karakterinde: 1.0)
  Stable    = 1.0
  Surging   = 1.75
  Warning   = 1.75
  Overflow  = 3.0
```

### Hareket Hizi (Player Controller'a cikti)

```
finalSpeed = baseSpeed * stateSpeedMultiplier * itemBonuses

stateSpeedMultiplier:
  Hollow    = 0.8
  Stable    = 1.0
  Surging   = 1.0
  Warning   = 1.0
  Overflow  = 1.0
```

### Ruh Kazanim (kill)

```
baseSoulGain = EnemyData.SoulReward

if hungerStacks > 0:
  actualGain = baseSoulGain * HungerMultiplier * hungerStacks
  hungerStacks = 0
else:
  actualGain = baseSoulGain

actualGain *= itemHarvestBonuses
CurrentSoul = min(CurrentSoul + actualGain, MaxSoul)
```

### Ruh Kayip (hasar)

```
soulLoss = incomingDamage  // 1:1 oran (baslangic)

if state == Overflow:
  // 1 hit kill — CurrentSoul = 0, oyuncu olur
  TriggerDeath()
else:
  CurrentSoul = max(CurrentSoul - soulLoss, 0)
```

### Shop Fiyat Hesaplama

```
if state == Hollow:
  itemCost = ItemData.BaseCost * (1 - HollowDiscount)  // 0.70x
else:
  itemCost = ItemData.BaseCost

if item.Category == Cursed:
  if character == TheFractured:
    cursedCost = ItemData.BaseCost * 1  // indirimli
  else:
    cursedCost = ItemData.BaseCost * CursedCostMultiplier  // 2x
```

## Kenar Durumlar

| Durum | Davranis |
|-------|----------|
| Ruh tam 0 | Oyuncu olmez. Hollow state, Hunger biriktirmeye devam. |
| Ruh tam MaxSoul | Overflow state. Fazla Ruh kaybolur (cap). |
| Overflow'da hasar | Aninda olum. `OnPlayerDeath` olayi tetiklenir. |
| Hollow'da hasar (Ruh zaten 0) | Ruh 0'da kalir, hasar etkisiz. Oyuncu hayatta. |
| Surge Warning sirasinda wave biterse | Warning timer durur, shop acilir. Surging'e geri donulur. |
| Birden fazla Hunger stack + oldurme | Tum stackler tek seferde tuketilir. `gain = base * 3 * stackCount` |
| Hunger stack Hollow disinda | Stackler korunur ama timer durur. Hollow'a geri donunce timer devam eder. |
| Item efekti SoulPercent'i degistirirse | State yeniden hesaplanir. Gecis olaylar tetiklenir. |
| Iki state sinirinda tam deger (ornek: 0.25) | Ust state'e dahil (0.25 = Stable, Hollow degil). |
| The Vessel'in %150 kapasitesi | MaxSoul = 150. Esik yuzdeleri ayni kalir. Mutlak Ruh degerleri artar. |
| Ayni frame'de hem Ruh kazanim hem kayip | Once kayip islenir, sonra kazanim. Overflow olum kontrolu kayip sirasinda yapilir. |

## Bagimliliklar

| Yonu | Sistem | Arayuz | Tip |
|------|--------|--------|-----|
| Giren | Data Manager | SoulStateData, HungerData okur | Sert |
| Giren | Character System | Pasif modifikasyonlar (MaxSoul, esikler, carpanlar) | Sert |
| Giren | Enemy System | Dusman olum olayi → Ruh kazanimi | Sert |
| Giren | Combat/Weapon | Hasar olayi → Ruh kaybi | Sert |
| Giren | Item System | Item efektleri → Ruh degisimi | Yumusak |
| Cikan | Combat/Weapon | stateDamageMultiplier | Sert |
| Cikan | Player Controller | stateSpeedMultiplier | Sert |
| Cikan | Shop System | Mevcut state, SoulPercent, fiyat carpani | Sert |
| Cikan | Wave Manager | Elite spawn sinyali (Overflow) | Yumusak |
| Cikan | Soul VFX | State, hungerStacks, warningTimer | Sert |
| Cikan | HUD/UI | SoulPercent, state, hungerStacks | Sert |
| Cikan | Audio System | State degisim olaylari | Yumusak |
| Cikan | Camera System | Overflow/Warning olaylari | Yumusak |

## Ayar Dugumleri

| Dugum | Varsayilan | Guvenli Aralik | Asiri Dusuk Etkisi | Asiri Yuksek Etkisi |
|-------|-----------|----------------|--------------------|--------------------|
| HollowThreshold | 0.25 | 0.10 – 0.35 | Hollow'a girmek cok zor | Hollow cok baskın |
| StableThreshold | 0.60 | 0.40 – 0.70 | Stable dar, hizli Surging | Surging'e ulasmak zor |
| SurgingThreshold | 0.85 | 0.75 – 0.90 | Warning cok erken | Warning hissetmeden Overflow |
| OverflowThreshold | 0.90 | 0.85 – 0.95 | Warning penceresi yok | Warning cok uzun |
| HollowDamageMultiplier | 0.5 | 0.3 – 0.8 | Hollow'da savas imkansiz | Hollow'da kalma tesviki |
| SurgingDamageMultiplier | 1.75 | 1.3 – 2.5 | Surging cazip degil | Surging cok OP |
| OverflowDamageMultiplier | 3.0 | 2.0 – 5.0 | Overflow risike degmez | Overflow her seyi tek atar |
| HollowSpeedMultiplier | 0.8 | 0.6 – 0.95 | Hollow'da kacis imkansiz | Hollow cezasi yok |
| HungerStackInterval | 2.0 sn | 1.0 – 4.0 | Hunger trivial hizli | Hollow'da cok uzun bekleme |
| MaxHungerStacks | 3 | 2 – 5 | Hunger odul dusuk | Tek patlama run bitirir |
| HungerMultiplier | 3.0 | 2.0 – 5.0 | Hunger cazip degil | Hunger exploit olur |
| WarningDuration | 3.5 sn | 2.0 – 5.0 | Karar icin zaman yok | Gerilim hissi yok |
| HollowDiscount | 0.30 | 0.15 – 0.50 | Hollow shop avantaji zayif | Hollow exploit |
| CursedCostMultiplier | 2.0 | 1.5 – 3.0 | Cursed cok ucuz, herkes alir | Cursed almaya degmez |
| Hysteresis | 0.02 | 0.01 – 0.05 | Sinirda titresim | State gecisi gecikir |

## Gorsel/Ses Gereksinimleri

| State | Aura | Ek Gorsel | Ses |
|-------|------|-----------|-----|
| Hollow | Gri, soluk | Hunger partikuller (stack sayisina gore yogunluk) | Sessiz, bosluk hissi |
| Stable | Beyaz, sakin | — | Normal ambiyans |
| Surging | Altin, parlak | Energy trail | Guc hissi, hafif bas |
| Surge Warning | Turuncu, titresen | Ekran kenari vignette | Dusuk frekans ugultu, kalp atisi |
| Overflow | Kirmizi parilti | Ekran distortion | Agir bas, tehlike alarmi |

## Kabul Kriterleri

- [ ] `CurrentSoul` degeri 0 ile MaxSoul arasinda kalıyor
- [ ] 5 state dogru esik degerlerinde gecis yapiyor
- [ ] Hysteresis sinirda titresimi onluyor
- [ ] Hasar carpani state'e gore dogru uygulanıyor (0.5x / 1.0x / 1.75x / 3.0x)
- [ ] Hiz carpani Hollow'da 0.8x uygulanıyor
- [ ] Hollow'da Hunger timer basliyor, her 2 sn'de stack birikiyor
- [ ] Stack maks 3'te (The Hollow: 5) duruyor
- [ ] Stack varken oldurme: `reward * 3 * stacks` Ruh veriyor, stackler sifirlanıyor
- [ ] Surge Warning timer SoulPercent >= 0.85'te basliyor
- [ ] Warning timer bitince hala >= 0.85 ise Overflow'a geciliyor
- [ ] Warning sirasinda SoulPercent < 0.85 dusurulurse Surging'e donuluyor
- [ ] Overflow'da hasar → aninda olum
- [ ] Ruh 0'da oyuncu olmuyor, Hollow'da kaliyor
- [ ] Shop'ta Hollow indirimi (%30) dogru uygulanıyor
- [ ] Cursed item fiyat carpani (2x) dogru uygulanıyor
- [ ] Tum olaylar (OnSoulStateChanged vb.) dogru tetikleniyor
- [ ] Performans: Soul hesaplama < 0.1ms / frame
- [ ] The Vessel: MaxSoul = 150, Warning +2 sn calisiyor
- [ ] The Hollow: Hollow hasar = 1.0x, maks stack = 5 calisiyor
- [ ] The Fractured: Cursed fiyat x1 calisiyor

## Acik Sorular

- Overflow'dan cikis icin aktif bir mekanik olmali mi? (Ruh harcama yetenegigi?)
- Hunger stackleri Hollow disindayken zamanla bozulmali mi?
- Item efektleri state esiklerini degistirebilmeli mi?
- Ayni frame'de birden fazla dusman oldurulurse her biri icin ayri Hunger hesabi mi?

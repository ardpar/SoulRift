# Health/Damage System

> **Status**: Designed
> **Author**: user + claude
> **Last Updated**: 2026-03-25
> **Implements Pillar**: Core Risk/Reward — hasar alma = Ruh kaybi, Overflow'da olum

## 1. Overview

**Health/Damage System**, SOULRIFT'te hasar alma ve verme islemlerini merkezi olarak yoneten sistemdir. Oyuncunun "can"i yoktur — hasar almak direkt Ruh kaybettirir (Soul System R4). Dusmanlar ise klasik HP barına sahiptir. Bu sistem iki yonlu calisir: (1) oyuncunun mermileri dusmanlara hasar verir (Combat System → Health/Damage → dusman HP azalir → olum tetiklenir), (2) dusmanlar oyuncuya temas veya mermi ile hasar verir (dusman → Health/Damage → Soul System Ruh kaybi). Overflow'da herhangi bir hasar aninda olum demektir. iframes mekanigi art arda hasar almayi onler. Bu sistem olmadan hasar hesaplarinin merkezi bir noktasi olmaz ve Overflow olum kurali uygulanamaz.

---

## 2. Player Fantasy

Bu sistem "gorunmez altyapi" kategorisindedir — oyuncu Health/Damage System'i fark etmez, ama **hasarin agirligi hissedilmelidir**. Kucuk dusmanin temasi "hafif bir itme", orta dusmanin vurusu "ciddi bir uyari", elite'in darbesi "felakete yakin". Overflow'da herhangi bir temas "kalp durması" — ani, kesin, dramatik. iframes ani kisa bir "kurtuldun" rahatligi verir. Hasar almak SOULRIFT'te "can kaybetmek" degil **"guc kaybetmek"** hissi yaratmali — cunku Ruh ayni zamanda hasar carpanin.

---

## 3. Detailed Design

### 3.1 Core Rules

**HD1. Dusman → Oyuncu Hasari (Contact Damage)**
- Dusmanlar oyuncuya **temas hasari** verir (Collider2D trigger)
- Her dusman tipinin sabit bir `contact_damage` degeri vardir (ScriptableObject: `EnemyData`)
- Temas aninda:
  1. `contact_damage` degeri alinir
  2. Soul System'e iletilir: `OnPlayerDamaged(contact_damage)`
  3. Soul System Ruh kaybi hesaplar: `soul_loss = base_soul_loss + (contact_damage * soul_loss_ratio)` (Soul System R4)
  4. Overflow kontrolu: eger state == Overflow → aninda olum (Soul System F5)
  5. iframes baslar (Soul System E4: 0.5 sn)

**HD2. Dusman Mermi → Oyuncu Hasari**
- Bazi dusmanlar mermi atar (ranged enemy — post-MVP veya gec wave'lerde)
- Mermi isabet ettikten sonra ayni HD1 akisi uygulanir
- Mermi hasar degeri `projectile_damage` olarak tanimlanir (EnemyData'dan)

**HD3. Oyuncu → Dusman Hasari**
- Combat System CF1 ile hesaplanir: `final_damage = floor(weapon_base * soul_multiplier * item_mod)`
- Health/Damage System'in rolu: `ApplyDamage(target, final_damage)` cagrilir
- Dusman HP azaltilir: `target.current_hp -= final_damage`
- HP <= 0 ise `OnEnemyKilled(enemyData)` tetiklenir

**HD4. Dusman HP**
- Her dusman tipi `base_hp` degerine sahiptir (EnemyData ScriptableObject)
- Wave olceklemesi uygulanir: `enemy_hp = base_hp * (1 + (wave - 1) * hp_scale)` (Wave System WF2)
- HP bar gosterimi UI tarafindan yonetilir

**HD5. iframes (Invincibility Frames)**
- Oyuncu hasar aldiktan sonra `iframes_duration` (varsayilan 0.5 sn) boyunca hasar almaz
- iframes sirasinda oyuncu gorunur ama "blink" animasyonu oynar (gorsel feedback)
- iframes Overflow'da da calisir — art arda 2 hasar engellenir (Soul System E4)
- iframes sadece oyuncuya uygulanir, dusmanlara uygulanmaz

**HD6. Knockback**
- Hasar aldiginda oyuncu kisa knockback yer (hasar yonunun tersine)
- Knockback mesafesi: `knockback_distance` (varsayilan 1.5 birim)
- Knockback suresi: 0.15 sn (hareket kontrolu kisa sureligine devre disi)

### 3.2 States and Transitions

Oyuncunun hasar durumu:

| State | Kosul | Davranis |
|-------|-------|----------|
| **Vulnerable** | Normal durum, iframes aktif degil | Hasar alinabilir |
| **Invincible** | iframes aktif (0.5 sn) | Hasar alinmaz, blink animasyonu |
| **Knockback** | Hasar aninda (0.15 sn) | Hareket kontrolu devre disi, geri itilme |
| **Dead** | Ruh = 0 veya Overflow + hasar | Run sona erer |

| Gecis | Tetikleyici |
|-------|-------------|
| Vulnerable → Invincible + Knockback | Hasar alindi |
| Knockback → Invincible | 0.15 sn sonra (knockback biter, iframes devam) |
| Invincible → Vulnerable | iframes_duration (0.5 sn) doldu |
| Vulnerable → Dead | Ruh = 0 (Soul System OnPlayerDeath) |
| Invincible → Dead | Olamaz (iframes korur) |

### 3.3 Interactions with Other Systems

| Sistem | Yon | Veri Akisi | Arayuz |
|--------|-----|-----------|--------|
| **Soul System** | Downstream | Hasar alindi → Ruh kaybi | `OnPlayerDamaged(damage)` → `RemoveSoul(soul_loss)` |
| **Soul System** | Upstream | Overflow state kontrolu | `GetCurrentState()` — Overflow ise aninda olum |
| **Soul System** | Upstream | iframes suresi | `iframes_duration` tuning knob (Soul System config'den) |
| **Combat System** | Upstream | Oyuncu hasar hesabi | `ApplyDamage(target, final_damage)` |
| **Wave System** | Upstream | Dusman HP/hasar olceklemesi | `wave_number` → WF2, WF3 formulleri |
| **Enemy AI** | Upstream | Dusman temas/mermi tetikleyicisi | Dusman collider trigger |
| **VFX** | Downstream | Hasar flash, knockback efekti | `OnPlayerDamaged`, `OnEnemyHit(pos, damage)` |
| **Audio** | Downstream | Hasar sesleri | `OnPlayerDamaged`, `OnEnemyHit` |
| **UI/HUD** | Downstream | Dusman HP bar guncelleme | `enemy.GetHPPercent()` |
| **Camera** | Downstream | Hasar screen shake | `OnPlayerDamaged` → kisa shake |

---

## 4. Formulas

### HDF1. Oyuncuya Hasar → Ruh Kaybi
```
soul_loss = base_soul_loss + (damage_taken * soul_loss_ratio)
```
**Kaynak:** Soul System R4, F4 — bu formul Soul System'de tanimli, burada referans.

### HDF2. Overflow Olum
```
if soul_state == Overflow AND damage_taken > 0 AND NOT iframes_active:
    player_dies = true
```
**Kaynak:** Soul System F5

### HDF3. Dusman HP (Wave Olcekli)
```
enemy_hp = base_hp * (1 + (wave_number - 1) * hp_scale_per_wave)
```
**Kaynak:** Wave System WF2

### HDF4. Dusman Hasar (Wave Olcekli)
```
enemy_damage = base_contact_damage * (1 + (wave_number - 1) * damage_scale_per_wave)
```
**Kaynak:** Wave System WF3

### HDF5. Dusman Tipleri — Base Degerleri

| Dusman Tipi | Base HP | Base Contact Damage | Soul Drop | Hareket Hizi |
|-------------|---------|---------------------|-----------|-------------|
| **Kucuk** | 15 | 5 | 2–4 | 3.0 u/sn |
| **Orta** | 40 | 10 | 5–8 | 2.5 u/sn |
| **Buyuk** | 80 | 18 | 10–15 | 1.8 u/sn |
| **Elite** | 150 | 25 | 15–25 | 2.2 u/sn |
| **Boss (Wave 5)** | 500 | 30 | 30–50 | 1.5 u/sn |
| **Boss (Wave 10)** | 1000 | 35 | 40–60 | 1.5 u/sn |
| **Boss (Wave 15)** | 1800 | 40 | 50–70 | 1.5 u/sn |
| **Final Boss (18)** | 3000 | 50 | 80–100 | 1.2 u/sn |

### HDF6. Hasar Ornek Senaryolari

| Senaryo | Damage | soul_loss (3 + d*0.5) | Ruh Etkisi (max 100) |
|---------|--------|----------------------|---------------------|
| Kucuk dusman temas (wave 1) | 5 | 5.5 | -%5.5 |
| Orta dusman temas (wave 8) | 13.5 | 9.75 | -%9.75 |
| Elite temas (wave 14) | 41.25 | 23.6 | -%23.6 (buyuk darbe) |
| Boss vurus (wave 10) | 50.75 | 28.4 | -%28.4 (yikici) |
| Overflow'da herhangi hasar | >0 | N/A | OLUM |

---

## 5. Edge Cases

| # | Durum | Ne Olur | Gerekce |
|---|-------|---------|---------|
| HDE1 | Ayni frame'de 2 dusman temasi | iframes ilk temasta baslar, ikinci temas engellenir | iframes mekaniginin amaci |
| HDE2 | Knockback oyuncuyu duvara iter | Oyuncu duvarda durur, fazla mesafe kaybolur. Duvara sikisma onlenir. | Fizik cozunurluk |
| HDE3 | Overflow + iframes aktif + hasar | iframes korur — hasar alinmaz, olum olmaz | iframes Overflow'da da gecerli (Soul System E4) |
| HDE4 | Dusman HP negatife duser | HP 0'da clamp edilir, `OnEnemyKilled` bir kez tetiklenir | Coklu mermi ayni frame'de isabet edebilir |
| HDE5 | Boss olurken kalan dusmanlar yok olur (Wave WE3) | Yok olan dusmanlar soul drop verir ama hasar hesabi yapilmaz | Wave System kurali |
| HDE6 | 0 hasarli dusman (tasarim hatasi) | `damage_taken = max(1, damage_taken)` — minimum 1 hasar garanti | 0 hasar = etkisiz dusman, bug onlenir |
| HDE7 | Knockback + shop acilma ayni an | Shop acilmaz wave bitene kadar. Knockback sadece wave icinde olur. | Zamanlama catismasi yok |
| HDE8 | iframes sirasinda Hunger stack timer | Timer devam eder — iframes sadece hasari engeller, diger sistemleri etkilemez | Hunger bagimsiz calisir |

---

## 6. Dependencies

### Hard Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Soul System** | Downstream | `RemoveSoul(soul_loss)`, `GetCurrentState()`, `OnPlayerDeath` | Hasar → Ruh kaybi koprusu — bu olmadan oyuncu hasar mekanigi calismaz |

### Soft Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Combat System** | Upstream | `ApplyDamage(target, damage)` | Oyuncu → dusman hasari. Olmazsa dusmanlar hasar almaz. |
| **Wave System** | Upstream | `GetCurrentWave()` | HP/hasar olceklemesi. Olmazsa base degerler kullanilir. |
| **Enemy AI** | Upstream | Dusman collider trigger | Dusman → oyuncu hasari tetikler |
| **VFX** | Downstream | `OnPlayerDamaged`, `OnEnemyHit` | Gorsel feedback |
| **Audio** | Downstream | `OnPlayerDamaged`, `OnEnemyHit` | Ses feedback |
| **Camera** | Downstream | `OnPlayerDamaged` | Screen shake |
| **UI/HUD** | Downstream | `enemy.GetHPPercent()` | Dusman HP bar |

### Bidirectional Referanslar
- Soul System R4: `soul_loss` formulu → **HDF1 referans**
- Soul System F5: Overflow olum → **HDF2 referans**
- Soul System E4: iframes → **HD5 detaylandirildi**
- Combat System CF1: hasar hesabi → **HD3 uygulama**
- Wave System WF2, WF3: olcekleme → **HDF3, HDF4 referans**

---

## 7. Tuning Knobs

Degerler `EnemyData` (dusman bazli), `DifficultyConfig` (olcekleme) ve `SoulSystemConfig` (kayip formulu) ScriptableObject'lerinde.

| Parametre | Varsayilan | Guvenli Aralik | Cok Dusukse | Cok Yuksekse | Kaynak |
|-----------|------------|----------------|-------------|-------------|--------|
| `base_soul_loss` | 3.0 | 1.0–8.0 | Hasar onemli degil | Kucuk temas yikici | Soul System config |
| `soul_loss_ratio` | 0.5 | 0.2–1.0 | Buyuk vurus hafif | Her vurus agir | Soul System config |
| `iframes_duration` | 0.5 sn | 0.2–1.0 | Art arda hasar mumkun | Uzun dokunulmazlik | Soul System config |
| `knockback_distance` | 1.5 birim | 0.5–3.0 | Knockback hissedilmez | Oyuncu cok uzaga firlar | Health/Damage config |
| `knockback_duration` | 0.15 sn | 0.05–0.3 | Ani, fark edilmez | Kontrol kaybi uzun, frustrating | Health/Damage config |
| Dusman base_hp | Tipe gore | Tablo HDF5 | Dusmanlar cok kolay | Dusmanlar cok dayanikli | EnemyData |
| Dusman contact_damage | Tipe gore | Tablo HDF5 | Temas onemli degil | Tek temas yikici | EnemyData |

**Not:** `base_soul_loss` ve `soul_loss_ratio` Soul System config'de tanimlidir — burada duplike edilmez, sadece referans.

---

## 8. Acceptance Criteria

### Fonksiyonel Testler

| # | Test | Beklenen Sonuc | Oncelik |
|---|------|----------------|---------|
| HDT1 | Dusman temas hasari | contact_damage Soul System'e iletilir, Ruh azalir | P0 |
| HDT2 | iframes: hasar sonrasi 0.5 sn koruma | Ikinci temas engellenir | P0 |
| HDT3 | Overflow + hasar = olum | State Overflow iken herhangi hasar → OnPlayerDeath | P0 |
| HDT4 | Overflow + iframes = yasam | iframes aktifken hasar alinmaz, Overflow'da bile | P0 |
| HDT5 | Oyuncu mermi → dusman HP azalir | ApplyDamage dogru uygulanir | P0 |
| HDT6 | Dusman HP 0 → OnEnemyKilled | Event tetiklenir, soul orb spawn olur | P0 |
| HDT7 | Knockback | Oyuncu hasar yonunun tersine 1.5 birim geri itilir | P1 |
| HDT8 | Wave olceklemesi | Wave 10'da dusman HP ~1.72x base | P1 |
| HDT9 | Kucuk dusman temas (wave 1): soul_loss | 3 + 5*0.5 = 5.5 Ruh kaybi | P1 |
| HDT10 | Minimum dusman hasar = 1 | 0 hasarli dusman olamaz | P1 |

### Performans Butcesi

| Metrik | Butce |
|--------|-------|
| Hasar hesaplama | < 0.01 ms / hit |
| Collision check (trigger) | Unity Physics2D standart |
| iframes state kontrolu | < 0.001 ms / frame |

---

## 9. Visual/Audio Requirements

### Gorsel

| Eleman | Detay |
|--------|-------|
| **iframes blink** | Oyuncu sprite %50 opacity ile blink (0.1 sn on/off toggle) |
| **Hasar flash** | Oyuncu sprite kirmizi flash (0.05 sn) hasar aninda |
| **Knockback** | Oyuncu geri kayma animasyonu (0.15 sn) |
| **Dusman hasar flash** | Dusman sprite beyaz flash (0.05 sn) mermi isabet aninda |
| **Dusman olum** | Kisa patlama partikulu + fade out (0.3 sn) |
| **Floating damage** | Dusmanin ustunde hasar rakami (Soul state renginde) |

### Ses

| Olay | Ses |
|------|-----|
| Oyuncu hasar aldı | Kisa "hit" + dusuk "ugh" (karakter sesi) |
| Overflow olum | Agir impact + cam kirilma (Soul System audio ile ortak) |
| Dusman hasar aldi | Hafif "thud" (Combat System ile ortak) |
| Dusman oldu | "Crunch" + soul chime (Combat System ile ortak) |

---

## 10. UI Requirements

### Dusman HP Bar
- Her dusman ustunde kucuk HP bar (sprite ustunde, 1 birim yukarda)
- Bar genisligi dusman boyutuna orantili (kucuk = ince, boss = genis)
- Renk: yesil → sari → kirmizi (HP yuzdesiyle)
- Hasar aldiginda bar kisa beyaz flash
- Boss HP bar: ekranin ustunde ayri, buyuk bar + boss ismi

### Hasar Rakamlari
- Dusmanin ustunde floating damage text (yukari kayar, 0.8 sn fade)
- Renk: Soul state renginde (Surging = altin, Overflow = kirmizi)
- Buyuk hasar (>20): daha buyuk font
- Critical hit (gelecek): ozel efekt (post-MVP)

### Oyuncu Hasar Gostergesi
- Hasar aninda ekran kenari kisa kirmizi flash (0.1 sn vignette)
- Screen shake: kucuk (0.5 px, 0.1 sn)

---

## 11. Open Questions

| # | Soru | Sahip | Hedef Cozum |
|---|------|-------|-------------|
| HDQ1 | Ranged dusman mermileri MVP'de mi post-MVP'de mi? | Game Designer | Hafta 3-4 — ilk playtest'te sadece contact damage test et |
| HDQ2 | Dusman knockback (oyuncunun mermileri dusmani geri iter) olacak mi? | Game Designer | Hafta 2 playtest — mermi hissi test et |
| HDQ3 | Armor/defense item'lari soul_loss_ratio'yu mu degistirir yoksa ayri multiplier mi? | Systems Designer | Item System GDD yazilirken |
| HDQ4 | Boss'larin faz (phase) sistemi olacak mi? (HP %50'de davranis degisikligi) | Game Designer | Boss tasarimi detayi — Hafta 5-6 |

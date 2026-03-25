# Combat System

> **Status**: Designed
> **Author**: user + claude
> **Last Updated**: 2026-03-25
> **Implements Pillar**: Core Risk/Reward — hasar carpani Soul state ile degisir

## 1. Overview

**Combat System**, SOULRIFT'in top-down ates mekanigini yoneten sistemdir. Oyuncu aim yonunu gosterir, silah otomatik olarak ates eder (auto-fire + manual aim, Brotato tarzi). Mermiler dusmanlara carpisarak hasar verir (hitbox collision). Hasar miktari Soul System'deki state'e gore carpanlanir — Hollow'da x0.5, Overflow'da x3.0. Dusman olduruldugunde `OnEnemyKilled` event'i tetiklenir ve Soul Orb spawn + Hunger bonusu zinciri baslar. MVP'de 4 silah tipi bulunur: Straight Shot (baslangic), Spread Shot, Orbital ve Beam. Combat System olmadan SOULRIFT'in micro loop'u (hareket et → ates et → Ruh topla) calismaz.

---

## 2. Player Fantasy

Oyuncu kendini **dusmanlar arasinda dans eden bir ates cemberi** gibi hissetmeli. Auto-fire sayesinde odak "ne zaman ates edeyim"den "nereye pozisyon alayim" a kayar. Surging'de hasar yukseldikce "her mermim eziyor" tatmini, Overflow'da "bir tanri gibi vuruyorum ama bir hata olum demek" gerilimi. Silah secimi playstyle'i belirler: Spread Shot kaotik kalabalik temizligi, Beam cerrahi hassasiyet, Orbital savunma odakli.

---

## 3. Detailed Design

### 3.1 Core Rules

**C1. Aim Mekanigi**
- Oyuncu mouse/sag analog stick ile aim yonunu belirler
- Aim yonu surekli guncellenir (frame bazli)
- Aim gostergesi (crosshair veya yon imleci) ekranda gorsel olarak gosterilir
- Gamepad destegi: sag stick ile 360 derece aim

**C2. Auto-Fire**
- Silah otomatik olarak `fire_rate` (ates/saniye) hizinda ates eder
- Oyuncunun ates tusu yoktur — aim yonu varsa silah ates eder
- Ates etmek icin en az bir dusman "algilama alaninda" olmali (opsiyonel — Brotato'da her zaman atesley)
- Varsayilan: dusman yoksa da ates eder (mermi israf olur ama akis bozulmaz)

**C3. Mermi (Projectile) Temelleri**
- Her ates bir veya daha fazla mermi spawn eder (Object Pool'dan)
- Mermi ozellikleri silah tipine gore degisir (ScriptableObject: `WeaponData`)
- Mermi parametreleri: `speed`, `damage`, `lifetime`, `pierce_count`, `size`
- Mermiler dusmanin Collider2D'sine carpisinca hasar uygular

**C4. Hasar Hesabi**
- `final_damage = weapon_base_damage * soul_state_multiplier * item_damage_modifiers`
- `soul_state_multiplier` Soul System F2'den okunur (Hollow x0.5, Stable x1.0, Surging x1.75, Overflow x3.0)
- Hasar tamsayi olarak yuvarlanir (floor): min 1 hasar garanti

**C5. Dusman Olumu**
- Dusman HP'si 0 veya altina dustugunde `OnEnemyKilled(enemyData)` event'i tetiklenir
- `enemyData` icerigi: dusman tipi, pozisyon, `soul_drop` degeri
- Bu event Soul System (Orb spawn) ve Hunger System (bonus hesabi) tarafindan dinlenir

**C6. Silah Tipleri (MVP — 4 Adet)**

| Silah | Mermi Davranisi | Base Damage | Fire Rate | Ozel | Zorluk |
|-------|----------------|-------------|-----------|------|--------|
| **Straight Shot** | Tek dumduz mermi | 10 | 3/sn | Pierce 1 (bir dusmandan gecer) | Baslangic |
| **Spread Shot** | 3–5 mermi yelpaze seklinde | 6 | 2/sn | Kisa menzil, genis alan | Kalabalik temizlik |
| **Orbital** | Oyuncunun etrafinda donen 2–4 kure | 8 | Surekli (donus hizi) | Yakin mesafe, savunma | Savunma |
| **Beam** | Surekli lazer, aim yonunde | 4/tick | 10 tick/sn (surekli) | Uzun menzil, tek hedef | Cerrahi |

**C7. Silah Degistirme**
- MVP'de oyuncu run boyunca tek silah kullanir (run baslangicinda veya shop'tan secilir)
- Silah degistirme butonu yok — item'lar silahi modifiye eder (ates hizi, mermi sayisi, hasar)

### 3.2 States and Transitions

Combat System kendi basina state yonetmez — surekli aktiftir. Silah tipi run boyunca sabittir (item modifikasyonlari haric). Ates durumu:

| Durum | Kosul | Davranis |
|-------|-------|----------|
| **Firing** | Aim input aktif | Auto-fire calisir, mermiler spawn olur |
| **Idle** | Aim input yok (gamepad: sag stick neutral) | Ates duraklar, son aim yonu korunur |
| **Shop** | Shop acik | Ates tamamen durur |

### 3.3 Interactions with Other Systems

| Sistem | Yon | Veri Akisi | Arayuz |
|--------|-----|-----------|--------|
| **Player Movement** | Upstream | Oyuncu pozisyonu (mermi spawn noktasi) | `Transform.position` |
| **Input System** | Upstream | Aim yonu (mouse/gamepad) | `GetAimDirection()` — Vector2 |
| **Projectile System** | Downstream | Mermi spawn komutu | `SpawnProjectile(weaponData, position, direction)` |
| **Object Pool** | Downstream | Mermi havuzu | Projectile System uzerinden erisim |
| **Soul System** | Upstream | State hasar carpani | `GetCurrentState()` → state_damage_multiplier (F2) |
| **Item System** | Upstream | Silah modifikatorleri | `GetDamageModifier()`, `GetFireRateModifier()`, `GetProjectileCountModifier()` |
| **Health/Damage** | Downstream | Dusmana hasar uygulama | `ApplyDamage(target, final_damage)` |
| **Soul System** | Downstream | Dusman olduruldu event'i | `OnEnemyKilled(enemyData)` → Soul Orb spawn |
| **Hunger System** | Downstream | Dusman olduruldu event'i | `OnEnemyKilled(enemyData)` → Hunger bonus (ayni event) |
| **VFX** | Downstream | Mermi trail, impact efekti | `OnProjectileHit(position, damage)` |
| **Audio** | Downstream | Ates sesi, impact sesi | `OnWeaponFired()`, `OnProjectileHit()` |

---

## 4. Formulas

### CF1. Hasar Hesabi
```
final_damage = floor(weapon_base_damage * soul_state_multiplier * item_damage_modifier)
final_damage = max(1, final_damage)  // minimum 1 hasar
```

**Ornek hesaplamalar:**

| Silah | Base Dmg | Soul State | Multiplier | Item Mod | Final |
|-------|----------|------------|------------|----------|-------|
| Straight | 10 | Hollow | x0.5 | x1.0 | 5 |
| Straight | 10 | Stable | x1.0 | x1.0 | 10 |
| Straight | 10 | Surging | x1.75 | x1.0 | 17 |
| Straight | 10 | Overflow | x3.0 | x1.0 | 30 |
| Straight | 10 | Overflow | x3.0 | x1.5 (item) | 45 |
| Spread | 6 | Surging | x1.75 | x1.0 | 10 (x5 mermi = 50 toplam) |
| Beam | 4/tick | Overflow | x3.0 | x1.0 | 12/tick (120/sn) |

### CF2. DPS (Damage Per Second)
```
dps = final_damage * projectiles_per_shot * fire_rate
```

| Silah | Dmg | Proj | Fire Rate | DPS (Stable) | DPS (Overflow) |
|-------|-----|------|-----------|-------------|----------------|
| Straight | 10 | 1 | 3/sn | 30 | 90 |
| Spread | 6 | 5 | 2/sn | 60 | 180 |
| Orbital | 8 | 3 | 2 rot/sn | 48 | 144 |
| Beam | 4 | 1 | 10 tick/sn | 40 | 120 |

**Not:** Spread Shot en yuksek potansiyel DPS'e sahip ama tum mermilerin isabet etmesi gerekir (kisa menzil). Beam en tutarli ama tek hedef.

### CF3. Mermi Ozellikleri
```
travel_distance = projectile_speed * projectile_lifetime
```

| Silah | Speed | Lifetime | Range | Pierce |
|-------|-------|----------|-------|--------|
| Straight | 12 u/sn | 1.5 sn | 18 birim | 1 |
| Spread | 10 u/sn | 0.8 sn | 8 birim | 0 |
| Orbital | 6 u/sn (donus) | Surekli | 2 birim yaricap | Surekli |
| Beam | Anlik | Surekli (aim boyunca) | 15 birim | Sinirsiz |

---

## 5. Edge Cases

| # | Durum | Ne Olur | Gerekce |
|---|-------|---------|---------|
| CE1 | Hollow'da hasar cok dusuk (6 * 0.5 = 3) | Minimum 1 garanti, ama 3 hala gecerli. Oyuncu Hollow'da daha uzun surer — Hunger stratejisi. | Hollow cezasi kasitli |
| CE2 | Overflow'da hic dusman yokken ates | Mermiler bosa gider. Sorun degil — auto-fire'in dogas\u0131. | Mermi israfi mekanik degil gorsel sorun |
| CE3 | Beam silahla birden fazla dusmana isabet | Beam pierce sinirsiz — hizadaki tum dusmanlar hasar alir. Her dusman icin ayri hasar hesabi. | Beam'in avantaji: crowd control potansiyeli |
| CE4 | Orbital + hareket: kure duvara carpar mi? | Hayir. Orbital kureler fizik katmaninda "trigger only" — duvarlari gecerler ama dusmanlara hasar verirler. | Orbital savunma silahi, duvarla etkilesim frustrating olur |
| CE5 | Spread Shot tum mermileri ayni dusmana isabet eder | Izinli. Yakin mesafede tum hasari tek hedefe yigmak Spread'in burst damage potansiyeli. | Risk/reward: yakin mesafeye girmen gerekiyor |
| CE6 | Item fire_rate'i cok artirirsa performans | Object Pool boyutu fire_rate * projectiles * lifetime ile sinirli. Pool doluysa en eski mermi geri donusturulur. | Performans guvenlik agi |
| CE7 | Oyuncu shop'tayken dusman gelir mi? | Hayir. Shop wave arasinda acilir, dusmanlar yok. Ates durur (Shop state). | Wave loop kurali |
| CE8 | Silah degisimi (gelecek versiyon) | MVP'de yok. Item'lar silahi modifiye eder ama tipi degistirmez. Post-MVP icin slot. | Scope siniri |

---

## 6. Dependencies

### Hard Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Player Movement** | Upstream | `Transform.position` | Mermi spawn noktasi |
| **Input System** | Upstream | `GetAimDirection()` | Aim yonu |
| **Object Pool** | Upstream | `GetFromPool<Projectile>()` | Mermi spawn |

### Soft Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Soul System** | Upstream | `GetCurrentState()` → multiplier | Hasar carpani. Olmazsa x1.0. |
| **Item System** | Upstream | `GetDamageModifier()`, `GetFireRateModifier()` | Item bonuslari. Olmazsa x1.0. |
| **Health/Damage** | Downstream | `ApplyDamage(target, damage)` | Hasar uygulama |
| **Hunger System** | Downstream | `OnEnemyKilled(enemyData)` | Ayni event — Hunger bonus tetiklenir |
| **Soul System** | Downstream | `OnEnemyKilled(enemyData)` | Ayni event — Soul Orb spawn |
| **VFX** | Downstream | `OnProjectileHit(pos, dmg)` | Impact efektleri |
| **Audio** | Downstream | `OnWeaponFired()`, `OnProjectileHit()` | Ates + impact sesleri |

### Bidirectional Referanslar
- Soul System F2: `effective_damage = base_damage * state_damage_multiplier * item_modifiers` → **Combat System CF1 ile ayni formul**
- Hunger System H3: `OnEnemyKilled(enemyData)` → **Combat System C5 bu event'i tetikler**

---

## 7. Tuning Knobs

Silah degerleri `WeaponData` ScriptableObject'leri uzerinden yonetilir. Genel combat degerleri `CombatConfig` altinda.

| Parametre | Varsayilan | Guvenli Aralik | Cok Dusukse | Cok Yuksekse | Etkilesim |
|-----------|------------|----------------|-------------|-------------|-----------|
| `straight_base_damage` | 10 | 6–18 | Wave temizleme cok yavas | Oyun cok kolay | DPS balans |
| `straight_fire_rate` | 3/sn | 1–6 | Mermi arasi cok uzun, tatminsiz | Mermi spam, performans | Object Pool boyutu |
| `spread_projectile_count` | 5 | 3–8 | Spread etkisi zayif | Performans + overkill | Object Pool boyutu |
| `spread_cone_angle` | 45 derece | 20–90 | Neredeyse duz cizgi | Her yone dagiliyor | Isabet orani |
| `orbital_count` | 3 | 2–6 | Savunma zayif | Dokunulmaz, cok guclu | Donus hizi |
| `orbital_radius` | 2.0 birim | 1.0–4.0 | Cok yakin, gorunmez | Cok uzak, kapsama alani buyuk | Hareket alani |
| `beam_range` | 15 birim | 8–25 | Kisa, yaklasma zorlar | Ekranin otesi, cok guvenli | Arena boyutu |
| `beam_tick_rate` | 10/sn | 5–20 | DPS dusuk hisseder | Performans | Hasar geri bildirimi |
| `min_damage` | 1 | 1 | Sabit | Sabit | Hollow'da alt sinir |

---

## 8. Acceptance Criteria

### Fonksiyonel Testler

| # | Test | Beklenen Sonuc | Oncelik |
|---|------|----------------|---------|
| CT1 | Aim + auto-fire: mouse aim, mermi spawn | Mermi aim yonunde cikiyor, fire_rate'e uygun | P0 |
| CT2 | Mermi carpisma: dusmana isabet | Hasar uygulanir, mermi yok olur (pierce 0) veya devam eder (pierce > 0) | P0 |
| CT3 | Hasar hesabi: Stable state | weapon_base * 1.0 * 1.0 = weapon_base | P0 |
| CT4 | Hasar hesabi: Overflow state | weapon_base * 3.0 = expected | P0 |
| CT5 | Dusman olumu | OnEnemyKilled tetiklenir, Soul Orb spawn olur | P0 |
| CT6 | Spread Shot: 5 mermi yelpaze | 5 mermi cone_angle icinde dagilir | P0 |
| CT7 | Orbital: etrafda donme | Kureler oyuncu etrafinda doner, dusmanlara hasar verir | P1 |
| CT8 | Beam: surekli hasar | Aim yonundeki dusmana tick bazli hasar | P1 |
| CT9 | Object Pool: mermi geri donusumu | Pool doluysa en eski mermi recycle edilir | P1 |
| CT10 | Shop state: ates durur | Shop acikken mermi spawn olmaz | P1 |
| CT11 | Min damage: Hollow'da Spread | 6 * 0.5 = 3, min 1 garanti (3 > 1, gecerli) | P1 |

### Performans Butcesi

| Metrik | Butce |
|--------|-------|
| Aktif mermi limiti | 200 (Object Pool max) |
| Mermi collision kontrolu | < 1 ms / frame (Physics2D spatial hashing) |
| Hasar hesabi | < 0.01 ms / hit |

### Playtest Kriterleri

| # | Kriter | Olcum |
|---|--------|-------|
| CP1 | Auto-fire tatmin edici hissediyor | Ates ritminin "pompali" hissi var mi? |
| CP2 | Aim okunakli | Oyuncu nereye ates ettigini biliyor mu? |
| CP3 | 4 silah farki hissediliyor | Her silah farkli oynaniyor mu? |
| CP4 | Soul state hasar farkini hissettiriyor | Surging'de "cok guclu" hissi var mi? |

---

## 9. Visual/Audio Requirements

### Gorsel

| Eleman | Detay |
|--------|-------|
| **Mermi sprite** | Silah tipine gore farkli: Straight = kucuk top, Spread = parcacik, Orbital = parlak kure, Beam = enerji cizgisi |
| **Mermi rengi** | Soul state'e gore renk tonu: Hollow=gri, Stable=beyaz, Surging=altin, Overflow=kirmizi |
| **Impact efekti** | Carpismada kucuk patlama partikulu (0.15 sn) |
| **Muzzle flash** | Ates aninda silah ucunda kisa flash (0.05 sn) |
| **Aim gostergesi** | Ince cizgi veya crosshair, oyuncudan aim yonune dogru |

### Ses

| Olay | Ses | Notlar |
|------|-----|--------|
| Ates (Straight) | Kisa "pew" | Her atista, pitch varyasyonu (±10%) |
| Ates (Spread) | "Shotgun blast" | Daha ağır, daha geniş ses |
| Ates (Beam) | Surekliler "zap" loop | Beam aktifken devam eder |
| Orbital donus | Hafif "whoosh" | Surekliler, subtle |
| Impact (normal) | "Thud" | Dusmana isabet |
| Impact (kill) | "Crunch" + soul chime | Olum daha tatmin edici |

---

## 10. UI Requirements

### Aim Gostergesi
- Oyuncudan aim yonune dogru ince cizgi (opacity %30, 3-4 birim uzunluk)
- Mouse modunda: crosshair mouse pozisyonunda
- Gamepad modunda: yon imleci oyuncudan sabit mesafede

### Silah Bilgisi
- HUD'un sag alt kosesinde kucuk silah ikonu
- Silah ismi ve base damage gosterilir (opsiyonel — playtest ile karar)

### Hasar Rakamlari
- Dusmana isabet ettikce kucuk floating damage numbers
- Soul state renginde gosterilir (Surging=altin, Overflow=kirmizi)
- Crit veya buyuk hasar: daha buyuk font

---

## 11. Open Questions

| # | Soru | Sahip | Hedef Cozum |
|---|------|-------|-------------|
| CQ1 | Dusman yokken ates etmek mi yoksa aim varken her zaman mi? Brotato tarzinda her zaman ates eder. | Game Designer | Hafta 2 playtest — her iki modu test et |
| CQ2 | Silah item ile tier upgrade (Straight Shot → Piercing Shot) sistemi olacak mi? | Game Designer | Item System GDD yazilirken |
| CQ3 | Ikinci silah slotu (dual-wield) eklenecek mi? | Game Designer | Post-MVP scope karari |
| CQ4 | Beam silahinin performans etkisi (surekli raycast) kabul edilebilir mi? | Technical | Hafta 1 prototype'da test et |
| CQ5 | Orbital silahi item ile kure sayisi artinca performans? | Technical | Object Pool stress test |
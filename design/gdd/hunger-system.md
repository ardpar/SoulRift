# Hunger System

> **Status**: Designed
> **Author**: user + claude
> **Last Updated**: 2026-03-25
> **Implements Pillar**: Core Risk/Reward — Hollow'u cezadan stratejik firsata donusturur

## 1. Overview

**Hunger System**, Soul System'in Hollow state'inde (%0–25 Ruh) aktive olan bir comeback mekanigid. Oyuncu Hollow'da hayatta kaldikca "Hunger stack" biriktirir. Her stack, sonraki dusman oldurmede kazanilan Ruh'u katlar ve oyuncuyu hizla Stable veya ustune firlatir. Bu mekanizma death spiral'i (dusuk Ruh → dusuk hasar → daha fazla hasar alma → daha dusuk Ruh) kirar ve Hollow'u kasitli olarak girilip cikilan bir ritme donusturur. Hunger System olmadan Hollow state tek yonlu bir cokustu; Hunger ile her state'in bir degeri olan bir spektrum haline gelir.

## 2. Player Fantasy

Oyuncu Hollow'a dustugunde panik yerine **stratejik bir firsat** hissetmeli. "Zayifim ama bu benim secimim" — biriktirme aninin gerilimi ve "simdi!" diyerek bir dusmani oldurdugunde gelen **Ruh patlamasi** tatmini. Hunger doluyken karakterin etrafindaki partikuller buyuyor, oyuncu "hazir" oldugunu UI'a bakmadan hissediyor, ve o oldurmede gelen x3-x15 Ruh akisi bir slot makinesi jackpot'u gibi hissettirmeli.

Bu his **kasitli bir dusus-yukselis ritmidir**: Hollow'a dustugunde "tamam, biriktiriyorum", stack doldugunda "simdi vuracagim", patlama aninda "isste geri geldim!" — ve tekrar Surging'e dogru tirmanmaya baslarsin.

## 3. Detailed Design

### 3.1 Core Rules

**H1. Aktivasyon Kosulu**
Hunger System sadece oyuncu **Hollow state'deyken** (%0–25 Ruh) aktiftir. Hollow'dan cikildiginda Hunger tamamen deaktive olur ve stack sifirlanir.

**H2. Stack Biriktirme**
- Hollow'da her `hunger_tick_interval` (varsayilan 2.0 sn) hayatta kalindikca 1 Hunger stack eklenir
- Maksimum stack: `max_hunger_stack` (varsayilan 3, The Hollow: 5)
- Timer Hollow'a girildiginde baslar, cikildiginda sifirlanir
- Stack biriktirmek icin oyuncunun hayatta olmasi yeterli — ozel bir aksiyon gerekmez

**H3. Stack Tuketme — Kill Bonus**
- Hollow'dayken bir dusman olduruldugunde:
  - Normal soul drop hesaplanir (Soul System F6)
  - Hunger bonusu eklenir: `hunger_bonus = soul_drop * current_stacks * hunger_multiplier`
  - `hunger_multiplier` varsayilan: 1.0 (yani x3 stack = x3 ekstra, toplam x4 Ruh)
  - Tum mevcut stack'ler tek seferde tuketilir
  - Stack sifirlanir, timer sifirdan baslar
- Ornek: 3 stack, dusman 4 Ruh drop → 4 + (4 * 3 * 1.0) = **16 Ruh** toplam

**H4. Hollow'dan Cikis**
- Hunger bonusu buyuk bir Ruh artisi saglar — bu cogu zaman oyuncuyu Stable veya ustune firlatir
- State gecisi Soul System tarafindan normal sekilde islenir (E6: coklu state atlama izinli)
- Hollow'dan cikildiginda kalan stack'ler kaybolur (tasiyici degil)

**H5. The Hollow Karakter Pasifi**
- `max_hunger_stack` = 5 (normal: 3)
- Hollow'da hasar carpani x1.0 (Soul System'den — normal oyuncular x0.5)
- Bu kombinasyon The Hollow'u "Hollow'da kal, biriktir, patla" playstyle'ina iter

### 3.2 States and Transitions

Hunger System'in 3 dahili durumu vardir:

| State | Kosul | Davranis |
|-------|-------|----------|
| **Inactive** | Soul state != Hollow | Sistem deaktif, stack = 0, timer durmus |
| **Stacking** | Soul state == Hollow, stack < max | Timer calisiyor, her tick +1 stack |
| **Full** | Soul state == Hollow, stack == max | Timer durmus, gorsel "hazir" sinyali aktif |

| Gecis | Tetikleyici |
|-------|-------------|
| Inactive → Stacking | `OnSoulStateChanged` → Hollow'a giris |
| Stacking → Full | `current_stacks == max_hunger_stack` |
| Full → Stacking | Dusman olduruldu → stack tuketildi → timer sifirdan baslar |
| Stacking → Inactive | `OnSoulStateChanged` → Hollow'dan cikis |
| Full → Inactive | `OnSoulStateChanged` → Hollow'dan cikis |

### 3.3 Interactions with Other Systems

| Sistem | Yon | Veri Akisi | Arayuz |
|--------|-----|-----------|--------|
| **Soul System** | Upstream | Mevcut state (Hollow mu?) | `OnSoulStateChanged` subscribe, `GetCurrentState()` |
| **Soul System** | Downstream | Hunger bonus Ruh ekleme | `AddSoul(hunger_bonus)` |
| **Combat/Shooting** | Upstream | Dusman olduruldu eventi | `OnEnemyKilled(enemyData)` — Hunger bonus hesabi tetiklenir |
| **Soul Pickup/Drop** | Interaction | Normal soul drop + Hunger bonus ayni anda | Hunger bonusu `AddSoul` ile direkt eklenir (orb olarak degil) |
| **Character System** | Upstream | The Hollow pasif degerleri | `SetMaxHungerStack(5)` run baslangicinda |
| **VFX/Aura** | Downstream | Stack sayisi (partikul yogunlugu) | `OnHungerStackChanged(count)`, `GetCurrentStacks()` |
| **UI/HUD** | Downstream | Stack sayisi (orb ikonlari) | `GetCurrentStacks()`, `GetMaxStacks()`, `OnHungerStackChanged` |
| **Audio** | Downstream | Stack biriktirme ve patlama sesleri | `OnHungerStackChanged`, `OnHungerConsumed(stacksUsed, bonusSoul)` |

**Onemli: Hunger bonus orb olarak yere dusmez.** Normal soul drop orb olarak duser ve oyuncu toplar. Hunger bonusu ise direkt `SoulManager.AddSoul()` ile eklenir — bu sayede "patlama" ani anlik ve tatmin edici hisseder, yere dusen orblari toplamak gerekmez.

## 4. Formulas

### HF1. Stack Biriktirme Zamanlayicisi
```
if soul_state == Hollow AND current_stacks < max_hunger_stack:
    hunger_timer += delta_time
    if hunger_timer >= hunger_tick_interval:
        current_stacks += 1
        hunger_timer = 0
        fire OnHungerStackChanged(current_stacks)
```

### HF2. Hunger Kill Bonus
```
hunger_bonus = soul_drop * current_stacks * hunger_multiplier
total_soul_gain = soul_drop + hunger_bonus
```

**Degiskenler:**
- `soul_drop`: float, dusman bazli (Soul System F6)
- `current_stacks`: int, 0 – max_hunger_stack
- `hunger_multiplier`: float, varsayilan 1.0 (aralik: 0.5 – 2.0)

**Ornek hesaplamalar:**

| Stack | Dusman Drop | Hunger Bonus | Toplam | Ruh Yuzdesi Artisi (max 100) |
|-------|------------|-------------|--------|------------------------------|
| 1 | 4 | 4 * 1 * 1.0 = 4 | 8 | +8% |
| 2 | 4 | 4 * 2 * 1.0 = 8 | 12 | +12% |
| 3 | 4 | 4 * 3 * 1.0 = 12 | 16 | +16% |
| 3 | 8 (orta) | 8 * 3 * 1.0 = 24 | 32 | +32% |
| 5 (Hollow) | 8 | 8 * 5 * 1.0 = 40 | 48 | +48% |

**Not:** 3 stack + orta dusman = +32 Ruh. Hollow esigi 25 oldugundan, bu oyuncuyu Hollow(%<25) → Surging(%57+) arasina firlatabilir. Tam da istedigimiz davranis.

---

## 5. Edge Cases

| # | Durum | Ne Olur | Gerekce |
|---|-------|---------|---------|
| HE1 | Hollow'da 0 stack ile dusman oldurme | Normal soul drop uygulanir, Hunger bonusu yok (0 * anything = 0) | Timer henuz dolmamis, ceza yok |
| HE2 | Stack doluyken (Full) Hollow'dan hasar ile cikilir | Stack'ler kaybolur, Inactive'e gecer. Tuketilmemis stack "israf" olur. | Risk: Hollow'da uzun kalmak tehlikeli |
| HE3 | Ayni frame'de 2 dusman oldurme | Ilk oldurme stack'leri tuketir + bonus. Ikinci oldurme 0 stack ile normal drop. | Stack tek seferde tuketilir, paylastirilmaz |
| HE4 | Hunger bonus ile Overflow'a cikmak | Izinli. AddSoul cagrilir, Soul System state'i hesaplar, Overflow'a gecis tetiklenir. | Riskli ama mumkun — ozellikle cok stack + buyuk dusman |
| HE5 | The Fractured'in rastgele dalgalanmasi Hollow'a dusurur | Hunger normal aktive olur. Dalgalanma tekrar yukariya cikarirsa stack kaybolur. | Fractured'in kaosu Hunger ile de etkilesir |
| HE6 | Hollow'da shop acik, timer devam ediyor mu? | Timer DEVAM EDER. Shop'tayken stack biriktirmek mumkun. | Shop suresi = bedava stack biriktirme firsati |
| HE7 | max_hunger_stack runtime'da degisir (item ile) | Eger mevcut stack < yeni max: devam. Eger mevcut stack > yeni max: clamp edilir. | Item ile stack max degisebilir |
| HE8 | Ruh tam 0'da (olum siniri), Hunger aktif | Oyuncu olmus demektir — OnPlayerDeath tetiklenir, Hunger deaktive. | Olum her seyi durdurur |

---

## 6. Dependencies

### Hard Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Soul System** | Upstream | `OnSoulStateChanged`, `GetCurrentState()`, `AddSoul()` | Hollow tespiti ve Ruh ekleme — bu olmadan Hunger calismaz |

### Soft Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Combat/Shooting** | Upstream | `OnEnemyKilled(enemyData)` | Olmazsa stack tuketimi tetiklenmez — test icin manuel tetikleme |
| **Character System** | Upstream | `SetMaxHungerStack(value)` | Olmazsa varsayilan max (3) kullanilir |
| **VFX/Aura** | Downstream | `OnHungerStackChanged`, `GetCurrentStacks()` | Gorsel feedback |
| **UI/HUD** | Downstream | `GetCurrentStacks()`, `GetMaxStacks()` | Stack gostergesi |
| **Audio** | Downstream | `OnHungerConsumed(stacksUsed, bonusSoul)` | Patlama sesi |

### Soul System GDD Bidirectional Referanslar
- Soul System R3: "Hunger System aktifken toplama carpani uygulanir" → **bu referans Hunger kill bonusunu isaret eder**
- Soul System F6: `hunger_multiplier` degiskeni → **Hunger System HF2'de tanimli**
- Soul System E6: coklu state atlama izinli → **Hunger patlamasi bu durumu tetikler**

---

## 7. Tuning Knobs

Tum degerler ScriptableObject (`HungerSystemConfig`) uzerinden yonetilir.

| Parametre | Varsayilan | Guvenli Aralik | Cok Dusukse | Cok Yuksekse | Etkilesim |
|-----------|------------|----------------|-------------|-------------|-----------|
| `hunger_tick_interval` | 2.0 sn | 1.0–4.0 | Stack cok hizli dolar, Hollow'da kalmak oduller | Stack cok yavas, Hollow sikici | Hollow suresi |
| `max_hunger_stack` | 3 | 2–5 | Patlama kucuk, comeback zayif | Patlama cok buyuk, tek oldurmede Overflow'a cikar | The Hollow pasifi (5) |
| `hunger_multiplier` | 1.0 | 0.5–2.0 | Bonus az, death spiral hala risk | Bonus cok, Hollow kasitli olarak abuse edilir | Balans kritik |

**Etkilesim uyarisi:** `max_hunger_stack` * `hunger_multiplier` * buyuk dusman drop = tek oldurmede kazanilabilecek maks Ruh. Ornek: 5 * 1.0 * 25 (elite) = 125 bonus + 25 normal = **150 Ruh**. max_soul = 100 ise bu Overflow'a firlatir. The Hollow icin bu kasitli bir risk.

---

## 8. Acceptance Criteria

### Fonksiyonel Testler

| # | Test | Beklenen Sonuc | Oncelik |
|---|------|----------------|---------|
| HT1 | Hollow'a giris | Hunger timer baslar, state = Stacking | P0 |
| HT2 | 2 sn Hollow'da hayatta kalma | Stack 0→1 olur, OnHungerStackChanged tetiklenir | P0 |
| HT3 | 6 sn Hollow'da hayatta kalma | Stack 0→1→2→3, Full state'e gecer | P0 |
| HT4 | Full state'de dusman oldurme | Bonus = drop * 3 * 1.0, stack sifirlanir, Stacking'e doner | P0 |
| HT5 | 2 stack ile dusman oldurme | Bonus = drop * 2 * 1.0, stack sifirlanir | P0 |
| HT6 | 0 stack ile dusman oldurme | Bonus = 0, sadece normal drop | P0 |
| HT7 | Hollow'dan hasar ile cikilir (stack dolu) | Stack sifirlanir, Inactive'e gecer | P0 |
| HT8 | Hunger bonus ile Stable'a gecis | AddSoul(bonus) → Soul System state gunceller → Hollow→Stable | P0 |
| HT9 | Hunger bonus ile Surging'e atlama | Coklu state gecisi (Hollow→Surging), tek event | P1 |
| HT10 | The Hollow: max stack = 5 | 5 stack biriktirilebilir, 10 sn surede (5 * 2sn) | P1 |
| HT11 | Shop acikken timer devam | Stack biriktirme shop'ta da calisir | P1 |

### Playtest Kriterleri (Hafta 2)

| # | Kriter | Olcum |
|---|--------|-------|
| HP1 | Hunger patlamasi "tatmin edici" hissediyor | Playtest: oyuncu Hollow'da kasitli kaliyor mu? |
| HP2 | Stack biriktirme suresi dogru | 2 sn/stack hissi: cok hizli degil (bedava), cok yavas degil (sikici) |
| HP3 | Death spiral kiriliyor | Playtest: Hollow'a dusen oyuncu toparlanabiliyor mu? |
| HP4 | Gorsel/ses feedback yeterli | Oyuncu stack'lerini UI'a bakmadan takip edebiliyor mu? |

---

## 9. Visual/Audio Requirements

### Gorsel — Hunger Stack Partikulleri

- Her stack icin karakterin etrafinda 1 kucuk ruhani orb donmeye baslar
- 0 stack: partikul yok
- 1 stack: 1 soluk, kucuk orb
- 2 stack: 2 orb, biraz daha parlak
- 3 stack (Full): 3 orb, parlak + pulse animasyonu ("hazir" sinyali)
- 5 stack (The Hollow Full): 5 orb, yogun parlaklik + daha hizli donis

### Gorsel — Patlama Ani

- Stack tuketildiginde: biriken orblar dusmana dogru akis animasyonu
- Ekranda kisa flash (0.1 sn, altin renk)
- Buyuk Ruh kazanim rakami yuzer: "+32 SOUL" gibi (buyuk font, altin)

### Ses

| Olay | Ses | Notlar |
|------|-----|--------|
| Stack +1 | Kisa "ding", yukselen pitch | Her stack'te pitch biraz artar |
| Full state | Surekliler dusuk hum + parıltı | "Hazir" hissi |
| Stack tuketme (patlama) | Satisfying "whoosh" + chime | En onemli ses — tatmin verici olmali |
| Stack kaybi (Hollow'dan cikildi) | Kisa "dissipate" | Kucuk kayip hissi |

---

## 10. UI Requirements

### Stack Gostergesi

- Soul metrenin sol tarafinda, yatay sirali kucuk orb ikonlari
- Orb sayisi = `max_hunger_stack` (3 veya 5)
- Dolu: parlak ruhani renk (mor/beyaz)
- Bos: soluk gri outline
- Full state'de tum orblar pulse yapar

### Patlama Bildirimi

- Stack tuketildiginde Soul metrenin ustunde kisa text: "+[bonus] HUNGER"
- Altin renk, 0.8 sn fade out
- Buyuk bonuslarda (>20 Ruh) text daha buyuk gosterilir

### Timer Gostergesi (Opsiyonel)

- Sonraki stack'e kalan sureyi gosteren ince dolum cubugu (stack gostergesinin altinda)
- Cok ince, dikkat dagitmayacak kadar subtle

---

## 11. Open Questions

| # | Soru | Sahip | Hedef Cozum |
|---|------|-------|-------------|
| HQ1 | Item'lar Hunger stack hizini veya max'ini modifiye edebilmeli mi? | Game Designer | Item System GDD yazilirken |
| HQ2 | Hunger bonus patlama ani icin ozel slow-motion efekti (hitstop) eklenmeli mi? | Game Designer | Hafta 2 playtest — hissi test et |
| HQ3 | Hollow'da shop'tayken stack biriktirme exploit mi yoksa feature mi? | Balans | Hafta 3-4 playtest — kasitli Hollow shop stratejisi gozlemle |
| HQ4 | Hunger stack visual'lari particle system mi yoksa sprite animasyonu mu olmali? | Technical Artist | Performans testine gore (Hafta 2) |

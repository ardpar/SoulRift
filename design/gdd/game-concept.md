# SOULRIFT — Game Design Document v0.2

**Genre:** Bullet-Heaven Roguelite | **Perspektif:** Top-Down Arena | **Stil:** Pixel Art
**Motor:** Unity | **Platform:** Steam PC | **Hedef Fiyat:** 4–6 USD | **Solo Dev**

---

## 1. Overview

Brotato'nun item sinerji derinligini, tek bir kaynak olan Ruh uzerine insa edilmis bir risk/odul sistemiyle birlestiren top-down arena roguelite. Ruh topladikca guclenirsin — ama ayni zamanda oldurucu hale gelirsin. Session suresi 20–35 dk.

---

## 2. Player Fantasy

Oyuncu surekli Ruh miktarini yonetir. Fazla Ruh toplarsan cok guclu ama cok kirilgansin. Az tutarsan Hollow Hunger ile toparlanabilirsin. Her an bir karar ani: daha fazla guc mu, yoksa guvenlik mi?

Temel gerilim: "Overflow'dayken hem en guclu hem en kirilgansin, hem de en cazip item onunde."

---

## 3. Detailed Rules

### 3.1 Core Mechanic — Soul System

Ruh (Soul), oyundaki tek kaynak. Can, para ve guc kaynagi ayni anda.
- Dusman oldurmek Ruh verir
- Hasar almak Ruh tuketir
- Item satin almak Ruh tuketir
- Miktarina gore oyuncu tamamen farkli bir karaktere donusur

### 3.2 Soul Level — 5 State Sistemi

| State | Aralik | Dezavantaj | Avantaj |
|-------|--------|------------|---------|
| **Hollow** | 0–25% | Hasar %50 dusuk, hareket yavas | Item fiyati %30 ucuz, Hunger stack birikir |
| **Stable** | 25–60% | — | Normal hasar, normal hiz, denge bolgesi |
| **Surging** | 60–85% | Dusmanlar hedef alir | Hasar %75 yuksek, gorsel efektler |
| **Surge Warning** | 85–90% | 3–4 sn karar zamani | Ekran kenari titrer, ses efekti baslar |
| **Overflow** | 90–100% | 1 vurusta olum, elite spawn | Hasar x3 |

### 3.3 Hollow Hunger Mekanigi (v0.2)

Death spiral sorununu cozer. Hollow state artik bir ceza degil — kasitli olarak girilip cikilan bir ritim.

**Akis:** Hollow State → Her 2 sn hayatta kal → Hunger x1 birikir → Sonraki oldurme x3 Ruh → Stable'a zipla

- Hunger stack maksimum 3 adet birikir (The Hollow karakterinde 5)
- Dolunca karakterin etrafinda kucuk ruhani partikuller belirir
- UI'a bakmadan oyuncu "simdi oldur" anini gorur ve hisseder

### 3.4 Surge Warning (v0.2)

Overflow artik "kazara girilen tehlike" degil, bilincli alinan bir karar.

**Akis:** Surging (%85) → Surge Warning baslar → 3–4 sn: Ruh harca → Surging'e don VEYA Devam et → Overflow

- Ekran kenarlari turuncu titrer, dusuk frekanslı ugultu sesi baslar
- The Vessel karakteri bu window'dan ek 2 saniye kazanir

### 3.5 Gameplay Loop

| Katman | Sure | Aciklama |
|--------|------|----------|
| Micro Loop | ~10 sn | Hareket et, ates et, Ruh topla, Soul state takip et |
| Wave Loop | ~90 sn | Wave temizle → Shop → 3 item sec → Sinerji planla |
| Run Loop | ~25 dk | 15–20 wave, boss wave'leri, final boss |
| Meta Loop | Uzun vadeli | Unlock sistemi: yeni karakterler, item pool genislemesi, zorluk modlari |

### 3.6 Shop Mekanigi

- Wave sonunda Ruh harcanarak item alinir
- Hollow'dayken fiyatlar %30 ucuz — kasitli "ucuz alisveris ani"
- Surging veya Overflow'da shop acilirsa normal 3 item + 1 Cursed Item slotu eklenir

---

## 4. Formulas

### Soul State Esikleri
- Hollow: 0–25% Soul
- Stable: 25–60% Soul
- Surging: 60–85% Soul
- Surge Warning: 85–90% Soul
- Overflow: 90–100% Soul

### Hasar Carpanlari
- Hollow: base_damage x 0.5
- Stable: base_damage x 1.0
- Surging: base_damage x 1.75
- Overflow: base_damage x 3.0

### Hollow Hunger
- Stack biriktirme: 1 stack / 2 saniye (Hollow'da hayatta kalma)
- Maks stack: 3 (The Hollow icin 5)
- Ruh bonus: sonraki oldurme x3 Ruh (stack basina)

### Fiyat Mekanigi
- Hollow'da item fiyati: base_price x 0.7
- Cursed item fiyati: base_price x 2.0 (The Fractured icin x1.0)

### Surge Warning Suresi
- Varsayilan: 3–4 saniye
- The Vessel bonusu: +2 saniye

---

## 5. Edge Cases

- **Hollow'da olum:** Hunger stack sifirlanir, run sona erer (roguelite — permadeath)
- **Overflow'da shop:** Cursed item slotu acilir ama Overflow hasari devam eder — shop ani bile tehlikeli
- **Surge Warning'de item satin alma:** Ruh harcamak state'i dusurur, Warning'den cikis yolu olabilir
- **The Fractured rastgele dalga:** Soul seviyesi state sinirlarina denk gelirse en yakin stable state uygulanir
- **Hunger stack doluyken oldurme:** Stack tuketilir, x3 Ruh kazanimi uygulanir, stack sifirdan baslar
- **Boss wave'de Hollow:** Hunger mekanigi aktif, boss hasar modifikatoru ayri uygulanir

---

## 6. Dependencies

### Sistem Bagimliliklari
- **Soul System** → Tum sistemlerin temeli (item, karakter, shop, VFX)
- **Item System** → Soul System + Shop System
- **Cursed Item System** → Item System + Soul State kontrolu
- **Hunger System** → Soul System (Hollow state)
- **Shop System** → Wave System + Soul System
- **Karakter Pasifleri** → Soul System + ilgili alt sistemler
- **VFX/Aura System** → Soul System (5 state gorsel feedback)
- **Wave System** → Bagimsiz (spawn logic)
- **Enemy AI** → Soul System (Surging targeting)

### Harici Bagimliliklar
- Unity 2D Pixel Art pipeline
- Object pooling (wave basi 50+ dusman)
- ParticleSystem (Hunger partikuller, Soul aura)

---

## 7. Tuning Knobs

| Parametre | Varsayilan | Aralik | Notlar |
|-----------|------------|--------|--------|
| Hollow esigi | 25% | 15–35% | Hollow suresini etkiler |
| Surging esigi | 60% | 50–70% | Guc erisim hizi |
| Surge Warning esigi | 85% | 80–92% | Warning pencere genisligi |
| Overflow esigi | 90% | 85–95% | Tehlike baslangici |
| Hollow hasar carpani | 0.5x | 0.3–0.7x | Hollow cezasi |
| Surging hasar carpani | 1.75x | 1.5–2.5x | Surging odulu |
| Overflow hasar carpani | 3.0x | 2.0–5.0x | Risk/odul dengesi |
| Hunger stack suresi | 2 sn | 1–4 sn | Stack biriktirme hizi |
| Hunger maks stack | 3 | 2–5 | Patlama buyuklugu |
| Hunger Ruh carpani | 3x | 2–5x | Hollow'dan cikis hizi |
| Hollow fiyat indirimi | %30 | %15–50% | Hollow alisveris avantaji |
| Cursed item fiyat carpani | 2x | 1.5–3x | Cursed risk/maliyet |
| Surge Warning suresi | 3–4 sn | 2–6 sn | Karar penceresi |
| Wave basi dusman sayisi | 50+ | 30–100 | Performans butcesi |
| Wave suresi | ~90 sn | 60–120 sn | Tempo |
| Toplam wave sayisi | 15–20 | 12–25 | Run uzunlugu |

---

## 8. Acceptance Criteria

### MVP Tanimi (10 Hafta)
- [ ] Top-down hareket + carpisma sistemi calisiyor
- [ ] Soul meter 5 state arasinda dogru gecis yapiyor
- [ ] Hollow Hunger stack sistemi calisir: stack birikir, oldurme ile x3 Ruh kazanimi
- [ ] Surge Warning gorsel + ses feedback'i oyuncuya net sinyal veriyor
- [ ] Shop sistemi wave arasi calisir, Hollow fiyat indirimi uygulanir
- [ ] Cursed item slotu Surging/Overflow'da acilir
- [ ] 40–50 item (4 kategoriden) sinerjileriyle calisir
- [ ] 3 karakter (The Vessel, The Hollow, The Fractured) pasif ozellikleriyle oynanabilir
- [ ] 15–20 wave + boss wave'leri tamamlanir
- [ ] 3–4 dusman tipi implement edilir
- [ ] Soul aura efektleri tum 5 state icin gorsel feedback saglar
- [ ] Pixel art sprite'lar finalize edilir (16x16 / 32x32)
- [ ] Ses efektleri + muzik entegre edilir
- [ ] Run suresi 20–35 dk araliginda
- [ ] Steam sayfasi hazirlenir

### Playtest Kriterleri (Hafta 2)
- [ ] Hunger stack dolunca oldurmenin Ruh "patlamasi" hissi veriyor mu?
- [ ] Surge Warning'de "simdi karar ver" gerilimi hissediliyor mu?
- [ ] Gorsel feedback (aura + partikuller) UI'a bakmadan durumu okutuyor mu?

---

## Ek: Item Kategorileri

| Kategori | Renk | Etki Turu | Ornek |
|----------|------|-----------|-------|
| **Soul Anchor** | Mor | Ruh seviyesini stabilize eder | "Overflow'da olmek yerine %50'ye indir" |
| **Amplifier** | Altin | Surging/Overflow hasari artirir | "Overflow'da her vurus alan hasari acar" |
| **Harvester** | Yesil | Ruh toplama oranini degistirir | "Her 10. oldurmede %20 bonus Ruh" |
| **Cursed** | Kirmizi | Guclu bonus + kalici dezavantaj | "Tum hasarin x2 ama max Ruh %80'e duser" |

## Ek: Karakter Tasarimi

| Karakter | Pasif Ozellik | Zorluk | Playstyle |
|----------|--------------|--------|-----------|
| **The Vessel** | Ruh kapasitesi %50 artik. Surge Warning suresi +2 sn. | Kolay | Overflow'u kontrollu kullanmayi ogren. |
| **The Hollow** | Hollow state'de hasar normale esit. Hunger stack maks 5. | Orta | Hollow'da kal, Hunger biriktir, patla. Burst damage. |
| **The Fractured** | Ruh seviyesi rastgele dalgalanir ama Cursed item slotu her zaman acik, fiyat x1. | Zor | Chaos build. Cursed item sinerjilerine odaklan. |

## Ek: Art Direction

- **Stil:** Pixel Art — 16x16 / 32x32 sprite, sinirli palet (koyu + parlak accent)
- **Ton:** Dark Fantasy — koyu arka plan, neon-soul efektleri, moody ama okunakli
- **Renk Paleti:** 6–8 renk. Mor = soul. Altin = tehlike. Turuncu = uyari. Kirmizi = olum.
- **Referans:** Stoneshard + Brotato renk okunabilirligi + Hades atmosferi

### Soul State Gorsel Feedback

| State | Aura Rengi | Ek Efekt |
|-------|-----------|----------|
| Hollow | Gri, soluk | Hunger stack partikulleri birikir |
| Stable | Beyaz, sakin | — |
| Surging | Altin, parlak | Energy trail |
| Surge Warning | Turuncu, titresim | Ekran kenari turuncu vignette + ses ugultusu |
| Overflow | Kirmizi parilti | Ekran distortion + elite spawn sesi |

## Ek: Rekabet Analizi

| Oyun | Fiyat | Satis | Fark | SOULRIFT Avantaji |
|------|-------|-------|------|-------------------|
| Vampire Survivors | 5$ | 10M+ | Resource yok, pasif gameplay | Soul sistemi aktif karar vermeyi zorlar |
| Brotato | 5$ | 5M+ | Coklu resource, karmasik item sistemi | Tek resource = daha net, erisilebilir |
| 20 Min Till Dawn | 5$ | 1M+ | Manuel aim, reload mekanigi | Soul state sistemi daha derin risk/odul |

## Ek: Riskler & Onlemler

| Seviye | Risk | Onlem |
|--------|------|-------|
| Yuksek | Soul sistemi "hissi" vermeyebilir | Hafta 2 playtest. Hunger patlamasi + Warning gerilimi test et. Gorsel feedback erken ekle. |
| Yuksek | Hollow Hunger balansi | Hunger degerlerini degisken tut (2–3 sn/stack, max 3–5). |
| Orta | Genre doygunlugu | Steam sayfasi "Soul Risk/Reward" USP'yi one cikar. Trailer'da Hunger + Overflow ani goster. |
| Orta | Scope kaymasi | 40 item MVP limiti asilmaz. Sinerji derinligi > item cesitliligi. |
| Dusuk | Unity performans | Object pooling, sprite renderer. Wave basi 50+ dusman icin pool Hafta 1'de kur. |

## Ek: MVP Roadmap — 10 Hafta

| Hafta | Odak | Detay |
|-------|------|-------|
| 1–2 | Core Prototype | Hareket, carpisma, Soul meter (5 state), ates mekanigi, wave spawner, Hunger stack, Surge Warning VFX |
| 3–4 | Game Loop & Playtest | Shop, 10 item, Soul state efektleri, Hollow fiyat indirimi, ilk playtest |
| 5–6 | Content & Balance | 40 item, 3 karakter, 15 wave + boss, 3–4 dusman tipi |
| 7–8 | Polish & Art | Sprite finalize, Soul aura VFX (5 state), Hunger partikul, ses + muzik, UI/UX |
| 9–10 | Steam Launch Prep | Steam sayfasi, wishlist kampanyasi, trailer (60 sn), playtest + bug fix + balance |

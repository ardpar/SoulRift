# Shop System

> **Status**: Designed
> **Author**: user + claude
> **Last Updated**: 2026-03-25
> **Implements Pillar**: Core Risk/Reward — item satin alma = Ruh harcama, Hollow indirimi, Cursed firsati

## 1. Overview

**Shop System**, her wave sonunda acilan item satin alma ekranidir. 3 rastgele item sunulur + 1 bedava reroll hakki. Oyuncu bir item satin alabilir (Ruh harcayarak), reroll yapabilir (yeni 3 item gosterilir) veya skip edebilir (+5 Ruh bonus kazanir). Item fiyatlari Soul state'e gore degisir: Hollow'da %30 ucuz. Surging veya Overflow'dayken normal 3 item'a ek olarak 1 Cursed Item slotu acilir. Shop acikken wave baslamaz — oyuncu kendi hizinda karar verir. Shop System item ekonomisinin giris noktasidir ve Soul System'in risk/reward mekanigini alisveris kararlarinia tasir.

---

## 2. Player Fantasy

"Her wave sonunda kucuk bir Noel" — 3 hediye kutusu, hangisini acacaksin? Hollow'dayken "ucuzluk firsati, simdi mi alsam?" karari. Surging/Overflow'dayken kirmizi Cursed Item slotu "tehlikeli ama cazip" hissi. Skip secenegi "hayir, Ruh'umu koruyacagim" stratejik disiplini. Reroll "belki daha iyisi cikar" kumar heyecani.

---

## 3. Detailed Design

### 3.1 Core Rules

**S1. Shop Acilma**
- `OnWaveCompleted(waveNumber)` event'i ile tetiklenir
- Shop UI acilir, oyun duraklamaz ama yeni wave baslamaz
- Dusmanlar yok (wave arasi), oyuncu guvenli

**S2. Item Gosterimi**
- 3 rastgele item, oyuncunun mevcut item pool'undan secilir
- Her item kart olarak gosterilir: isim, ikon, aciklama, fiyat
- Duplicate kontrolu: ayni shop'ta ayni item 2 kez cikmaz
- Item pool wave numarasina gore genisler (gec wave = nadir item'lar acilir)

**S3. Fiyatlandirma**
- Her item'in base fiyati vardir (`item_base_cost`, ScriptableObject)
- Aktif fiyat: `display_price = item_base_cost * state_price_modifier`
- Hollow (%0–25): `state_price_modifier = 0.7` (-%30 indirim)
- Diger state'ler: `state_price_modifier = 1.0`
- Fiyat tamsayiya yuvarlanir (floor), minimum 1

**S4. Satin Alma**
- Oyuncu bir item'a tiklar/secer
- Yeterli Ruh varsa (`current_soul >= display_price`):
  - `SoulManager.RemoveSoul(display_price)` cagirilir
  - Item oyuncunun envanterine eklenir
  - Shop kapatilir, sonraki wave baslar
- Yetersiz Ruh: satin alma engellenir, gorsel uyari

**S5. Reroll**
- Her shop'ta 1 bedava reroll hakki
- Reroll tiklaninca: mevcut 3 item degisir, yeni 3 rastgele item gosterilir
- Ek reroll'ler `reroll_cost` Ruh harcar (varsayilan 5, her reroll +3 artar)
- Reroll Cursed slotunu etkilemez (Cursed item ayni kalir)

**S6. Skip (Gecme)**
- Oyuncu "Skip" butonuna basarsa hicbir item almaz
- `skip_soul_bonus` kadar Ruh kazanir (varsayilan 5)
- `SoulManager.AddSoul(skip_soul_bonus)` cagirilir
- Shop kapatilir, sonraki wave baslar

**S7. Cursed Item Slotu**
- Oyuncu Surging (%60+) veya Overflow (%90+) state'deyken shop acilirsa:
  - Normal 3 item'a ek olarak 1 Cursed Item slotu gosterilir
  - Cursed item fiyati: `item_base_cost * 2.0` (normal'in 2 kati)
  - The Fractured: Cursed slot **her zaman** acik (state bagimsiz), fiyat x1.0
- Cursed item almak zorunlu degil — ek bir firsattir
- Cursed item alinirsa normal item secimi de yapilabilir (2 item alinabilir: 1 normal + 1 cursed)

**S8. Shop Kapatma**
- Item satin aldiginda otomatik kapanir
- Skip tiklandiginda otomatik kapanir
- Manuel kapatma butonu yok — ya al ya gec

### 3.2 States and Transitions

| State | Kosul | Davranis |
|-------|-------|----------|
| **Closed** | Wave aktif veya shop kapandi | Shop UI gorunmez |
| **Open — Browsing** | Shop acik, item secimi yapilmadi | 3 item + (opsiyonel Cursed) gosteriliyor |
| **Open — Rerolling** | Reroll tiklandı | Kisa animasyon, yeni item'lar gosterilir |
| **Purchasing** | Item secildi, Ruh yeterli | Satin alma islemi, Ruh azalir |
| **Skipping** | Skip secildi | Ruh bonusu eklenir |

| Gecis | Tetikleyici |
|-------|-------------|
| Closed → Open Browsing | `OnWaveCompleted` |
| Open Browsing → Open Rerolling | Reroll butonu tiklandi |
| Open Rerolling → Open Browsing | Animasyon bitti, yeni item'lar gosterildi |
| Open Browsing → Purchasing | Item secildi + yeterli Ruh |
| Open Browsing → Skipping | Skip butonu tiklandi |
| Purchasing → Closed | Satin alma tamamlandi |
| Skipping → Closed | Skip bonusu eklendi |

### 3.3 Interactions with Other Systems

| Sistem | Yon | Veri Akisi | Arayuz |
|--------|-----|-----------|--------|
| **Wave System** | Upstream | Wave tamamlandi event'i | `OnWaveCompleted(waveNumber)` — shop acar |
| **Soul System** | Upstream | Mevcut Ruh ve state | `GetCurrentSoul()`, `GetCurrentState()` — fiyat hesabi + Cursed slot |
| **Soul System** | Downstream | Ruh harcama/kazanma | `RemoveSoul(price)`, `AddSoul(skip_bonus)` |
| **Item System** | Upstream | Item pool, item data | `GetAvailableItems(waveNumber)`, `ItemData` ScriptableObject'leri |
| **Cursed Item System** | Upstream | Cursed item pool | `GetCursedItem()` — Surging+ ise |
| **Character System** | Upstream | The Fractured Cursed pasifi | `HasAlwaysCursedSlot()` |
| **Hunger System** | Indirect | Hollow'da shop acikken Hunger timer devam eder | Hunger HE6 — shop suresi = stack biriktirme firsati |
| **Surge Warning** | Indirect | Warning'de shop acikken timer duraklar | Soul System E7, Surge Warning SW5 |
| **UI/HUD** | Downstream | Item kartlari, fiyatlar, butonlar | Shop UI tamamen bu sistem tarafindan yonetilir |

---

## 4. Formulas

### SF1. Item Fiyat Hesabi
```
display_price = floor(item_base_cost * state_price_modifier)
display_price = max(1, display_price)
```

| State | Modifier | Ornek (base 20) | Ornek (base 8) |
|-------|----------|-----------------|----------------|
| Hollow | 0.7 | 14 | 5 |
| Stable | 1.0 | 20 | 8 |
| Surging | 1.0 | 20 | 8 |
| Overflow | 1.0 | 20 | 8 |

### SF2. Cursed Item Fiyat
```
cursed_price = floor(item_base_cost * cursed_price_multiplier)
// Normal oyuncu: cursed_price_multiplier = 2.0
// The Fractured: cursed_price_multiplier = 1.0
```

### SF3. Reroll Maliyet
```
reroll_cost = reroll_base_cost + (rerolls_used_this_shop * reroll_increment)
// Ilk reroll: bedava (rerolls_used = 0, maliyet hesaplanmaz)
// Ikinci: 5 + 0*3 = 5
// Ucuncu: 5 + 1*3 = 8
// Dorduncu: 5 + 2*3 = 11
```

### SF4. Item Pool Genislemesi
```
available_rarities = base_rarities
if wave_number >= 5:  available_rarities += Uncommon
if wave_number >= 10: available_rarities += Rare
if wave_number >= 15: available_rarities += Legendary
```

### SF5. Hollow Shop Stratejisi — Ruh Analizi
Hollow'da shop acilirsa: fiyatlar %30 ucuz. Eger oyuncu 20 Ruh'a sahipse:
- Normal: 20 Ruh'luk item alabilir
- Hollow: 20 / 0.7 = 28 Ruh degerinde item alabilir
- **Hollow'da alisveris = %42 daha fazla deger** — kasitli Hollow stratejisi

---

## 5. Edge Cases

| # | Durum | Ne Olur | Gerekce |
|---|-------|---------|---------|
| SE1 | Ruh 0'da shop acilir (olu olmadiysa — Hollow'da 1 Ruh ile wave bitti) | Shop acilir, item'lar gosterilir ama hicbiri alinamaz. Skip +5 Ruh verir. | Skip = comeback yolu |
| SE2 | Hollow'da shop acik, Hunger stack birikiyor | Normal — Hunger timer devam eder (Hunger HE6). Stack biriktirip skip yapip wave'e donmek gecerli strateji. | Feature, bug degil |
| SE3 | Warning'de shop acik, timer duraklatilmis | Normal — Surge Warning SW5 ile uyumlu. Oyuncu sakin karar verebilir. | Shop'ta timer baskisi olmamali |
| SE4 | Shop'ta item alindi, Ruh dusup state degisti | Satin alma ani state kontrolu yapmaz — fiyat acilis anindaki state ile hesaplandi. State degisimi satin alma sonrasi normal islenir. | Fiyat "anlik goruntudur" |
| SE5 | Reroll sonrasi Cursed item degisti mi? | Hayir. Reroll sadece normal 3 item'i degistirir. Cursed slot sabit kalir. | Cursed slot ozel firsattir |
| SE6 | The Fractured: Hollow'da Cursed slot + ucuz fiyat | Evet — Hollow fiyat indirimi Cursed item'a da uygulanir (base_cost * 1.0 * 0.7). Fractured Hollow'da cok ucuza Cursed alabilir. | Karakter sinerjisi — kasitli |
| SE7 | Item pool bos (tum item'lar alinmis) | Pratikte olmaz (40-50 item, run basina ~18 satin alma). Olursa: reroll devre disi, mevcut 3 item gosterilir. | Scope siniri |
| SE8 | Normal item + Cursed item ayni anda alinabilir mi? | Evet. Oyuncu once normal item alir (shop kapanmaz), sonra Cursed alabilir. VEYA sadece birini alir. Sirasi: normal sec → Cursed sec → onay → shop kapanir. | 2 item alma firsati (yuksek Ruh maliyeti) |

---

## 6. Dependencies

### Hard Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Soul System** | Upstream + Downstream | `GetCurrentSoul()`, `GetCurrentState()`, `RemoveSoul()`, `AddSoul()` | Fiyat, state, satin alma, skip bonus |
| **Wave System** | Upstream | `OnWaveCompleted(waveNumber)` | Shop acma tetikleyicisi |

### Soft Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Item System** | Upstream | `GetAvailableItems(wave)`, `ItemData` | Item pool. Olmazsa bos shop — anlamsiz. |
| **Cursed Item System** | Upstream | `GetCursedItem()` | Cursed slot. Olmazsa Cursed slot gosterilmez. |
| **Character System** | Upstream | `HasAlwaysCursedSlot()` | Fractured pasifi |
| **Hunger System** | Indirect | Timer devam eder | Shop suresi Hunger firsati |
| **Surge Warning** | Indirect | Timer duraklar | Shop'ta Warning baskisi yok |

---

## 7. Tuning Knobs

| Parametre | Varsayilan | Guvenli Aralik | Cok Dusukse | Cok Yuksekse | Etkilesim |
|-----------|------------|----------------|-------------|-------------|-----------|
| `hollow_price_modifier` | 0.7 | 0.5–0.9 | Hollow'da cok ucuz, abuse | Indirim hissedilmez | Hollow stratejisi |
| `cursed_price_multiplier` | 2.0 | 1.5–3.0 | Cursed cok ucuz, risk az | Cursed cok pahali, alinmaz | Risk/reward |
| `fractured_cursed_multiplier` | 1.0 | 0.5–1.5 | Fractured cok avantajli | Fractured pasifi zayif | Karakter dengesi |
| `skip_soul_bonus` | 5 | 2–10 | Skip odutsuz, her zaman al | Skip cok odulli, item almama | Ekonomi dengesi |
| `reroll_base_cost` | 5 | 3–10 | Reroll ucuz, cok kullan | Reroll pahali, hic kullanma | Rastgelelik kontrolu |
| `reroll_increment` | 3 | 1–5 | Ard arda reroll ucuz | Ikinci reroll bile pahali | Reroll spam onleme |
| `items_per_shop` | 3 | 2–4 | Az secenek, sıkıcı | Cok secenek, karar yorgunlugu | Tempo |

---

## 8. Acceptance Criteria

### Fonksiyonel Testler

| # | Test | Beklenen Sonuc | Oncelik |
|---|------|----------------|---------|
| ST1 | Wave bittikten sonra shop acilir | 3 item gosterilir, fiyatlar dogru | P0 |
| ST2 | Hollow'da fiyat indirimi | Fiyatlar %30 ucuz | P0 |
| ST3 | Satin alma — yeterli Ruh | Item eklenir, Ruh azalir, shop kapanir | P0 |
| ST4 | Satin alma — yetersiz Ruh | Engellenir, uyari gosterilir | P0 |
| ST5 | Skip | +5 Ruh eklenir, shop kapanir | P0 |
| ST6 | Reroll (bedava) | Yeni 3 item gosterilir | P0 |
| ST7 | Surging'de Cursed slot | 4. slot (Cursed) gosterilir, fiyat x2 | P0 |
| ST8 | Stable'da Cursed slot yok | Sadece 3 normal item | P0 |
| ST9 | Fractured: her zaman Cursed slot | State ne olursa olsun Cursed slot var, fiyat x1 | P1 |
| ST10 | Normal + Cursed birlikte alinabilir | Iki item birden satin alinir, toplam Ruh dogru azalir | P1 |

---

## 9. Visual/Audio Requirements

### Shop UI Tasarimi
- Tam ekran overlay (arka plan %60 karartilmis)
- 3 item karti yan yana (orta ekran)
- Cursed slot (varsa): sag tarafta, kirmizi kenarli, ayri
- Skip butonu: sol alt
- Reroll butonu: sag alt (bedava ise yesil, ucretli ise fiyat gosterir)

### Item Kart Tasarimi
- Koyu arka plan, kategori renginde kenar (Mor/Altin/Yesil/Kirmizi)
- Ikon (64x64 piksel), isim, 1 satirlik aciklama, fiyat
- Fiyat: Hollow'da yesil renk + "INDIRIM" etiketi
- Alinamaz (yetersiz Ruh): gri overlay

### Ses
| Olay | Ses |
|------|-----|
| Shop acilir | Kisa "ka-ching" |
| Item hover | Hafif "tick" |
| Item satin alma | Tatmin edici "purchase" chime |
| Skip | Kisa "whoosh" |
| Reroll | Kart karistirma sesi |
| Yetersiz Ruh | Dusuk "buzz" (red) |

---

## 10. UI Requirements

(Section 9'da detayli tanimlandi — Shop System'in UI'i sistemin kendisidir.)

### Ek UI Notlari
- Mevcut Ruh miktari shop'un ust ortasinda buyuk gosterilir
- Soul state gostergesi (Hollow/Stable/Surging vb.) shop'ta da gorunur
- Hollow'dayken UI'da "HOLLOW DISCOUNT -30%" banner'i

---

## 11. Open Questions

| # | Soru | Sahip | Hedef Cozum |
|---|------|-------|-------------|
| SQ1 | Shop suresi sinirli mi (timer)? Yoksa oyuncu istedigince mi bekler? | Game Designer | Hafta 3-4 playtest |
| SQ2 | Item raresi (Common/Uncommon/Rare/Legendary) nasil gosterilecek? | Art Director | Item System GDD ile birlikte |
| SQ3 | "Lock" mekanigi (bir item'i reroll'dan koruma) eklenecek mi? | Game Designer | Post-MVP |
| SQ4 | Cursed + normal ayni anda alma UX'i nasil hissettiriyor? | UX Designer | Hafta 3-4 playtest |

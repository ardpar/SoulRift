# Item Kategorileri & Sinerjiler

## Genel Bakis

Item'lar Soul System'in uzerine insa edilen sinerji katmanidir. 4 kategori, her biri farkli bir stratejiyi destekler. MVP hedefi: **40-50 item**.

## Oyuncu Fantezisi

"Bu item kombinasyonuyla tamamen farkli bir build yapabilirim." Her run farkli hissettiren, kesfedilecek sinerjilerle dolu.

## Item Kategorileri

| Kategori | Renk | Etki Turu | Ornek |
|----------|------|-----------|-------|
| **Soul Anchor** | Mor | Ruh seviyesini stabilize eder | "Overflow'da olmek yerine %50'ye indir" |
| **Amplifier** | Altin | Surging/Overflow hasarini artirir | "Overflow'da her vurus alan hasari acar" |
| **Harvester** | Yesil | Ruh toplama oranini degistirir | "Her 10. oldurmede %20 bonus Ruh" |
| **Cursed** | Kirmizi | Guclu bonus + kalici dezavantaj | "Tum hasarin x2 ama max Ruh %80'e duser" |

## Cursed Item Sistemi — Tanim Cercevesi (v0.2)

| Alan | Kural |
|------|-------|
| **Tetiklenme** | Surging (%60+) veya Overflow'dayken shop acilirsa normal 3 item + 1 Cursed slot eklenir. Stable veya Hollow'da Cursed slot cikmaz. |
| **Maliyet** | Normal item fiyatinin **2x**'i (Ruh olarak). Yuksek riski yuksek maliyet dengeler. |
| **Slot Yapisi** | Normal 3 item seceneginden bagimsiz, ek bir slot. Oyuncu normal secimini de yapabilir. Cursed almak zorunlu degil. |
| **The Fractured** | Her zaman 1 Cursed slot acik (state bagimsiz). Fiyat normal x1 (digerleri x2). Bu karakter icin risk degil, core playstyle. |

### Tasarim Notu

Cursed itemlar "yuksek Ruh aninda firsata donusen tehlike" kimligini kazandi. Overflow'dayken hem en guclu hem en kirilagansin, hem de en cazip item onunde. Bu uclu gerilim Soul sisteminin vaadini somutlastirir.

## Sinerji Ornekleri

- **Soul Anchor + Amplifier**: Overflow'da guclu vur, olumden korun
- **Harvester + Hollow strateji**: Cok Ruh topla, Hollow'a dus, ucuz alisveris yap
- **Cursed + The Fractured**: Cursed item stackle, chaos build olustur
- **Amplifier + Surging**: Surging'de kal, surekli yuksek hasar

## Formuller

| Parametre | Deger |
|-----------|-------|
| MVP item sayisi | 40-50 |
| Kategori basina item | ~10-12 |
| Cursed item fiyat carpani | x2 (The Fractured: x1) |
| Cursed slot tetiklenme | Surging (%60+) veya Overflow |
| Shop item secenegi | 3 + 1 Cursed (kosullu) |

## Kenar Durumlar

- Ayni item birden fazla alinabilir mi? (stacklenme kurallari belirlenmeli)
- Cursed item dezavantajlari birbirini iptal edebilir mi?
- Maksimum item tasima limiti var mi?
- Shop'ta tekrar eden itemlar cikar mi?

## Ayar Dugumleri

- Her item'in stat degerleri
- Sinerji carpanlari
- Cursed dezavantaj siddeti
- Item drop/shop olasiliklari
- Rarity sistemi (varsa)

## Kabul Kriterleri

- [ ] 4 kategoriden item'lar shop'ta gorunuyor
- [ ] Cursed slot sadece Surging/Overflow'da aciliyor
- [ ] Cursed item fiyati 2x uygulanıyor
- [ ] The Fractured icin Cursed slot her zaman acik
- [ ] Item sinerji efektleri calisiyor
- [ ] 40-50 item MVP'de mevcut

## Bagimliliklar

- [Soul System](soul-system.md) — State'ler item etkilerini belirler
- [Gameplay Loop](gameplay-loop.md) — Shop mekanigi
- [Karakterler](characters.md) — Karakter pasifleri item secimini etkiler

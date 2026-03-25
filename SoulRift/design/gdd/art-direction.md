# Art Direction

## Genel Bakis

SOULRIFT, koyu arka planlar uzerine parlak ruhani efektlerle insa edilmis bir dark fantasy pixel art oyunudur. Gorsel dil, Soul state'ini UI'a bakmadan okunaklı kilar.

## Oyuncu Fantezisi

Karanlik bir dunyada Ruh'un gucuyle parlayan bir karakter. Guc arttikca gorsel olarak daha etkileyici — ama tehlike de o kadar belirgin.

## Stil

| Alan | Deger |
|------|-------|
| **Stil** | Pixel Art |
| **Sprite Boyutu** | 16x16 / 32x32 |
| **Ton** | Dark Fantasy — koyu arka plan, neon-soul efektleri. Moody ama okunaklı. |
| **Renk Paleti** | 6-8 renk. Mor/mor tonlari soul icin. Altin = tehlike. Turuncu = uyari. Kirmizi = olum. |
| **Referans** | Stoneshard + Brotato renk okunabilirligi + Hades atmosferi |

## Soul State Gorsel Geri Bildirimi

| State | Aura Rengi | Ek Efekt |
|-------|-----------|----------|
| **Hollow** | Gri, soluk | Hunger stack'i temsil eden kucuk partikuller birikir |
| **Stable** | Beyaz, sakin | — |
| **Surging** | Altin, parlak | Karakterin etrafinda energy trail |
| **Surge Warning** | Turuncu, titresen | Ekran kenari turuncu vignette + ses ugultusu |
| **Overflow** | Kirmizi parilti | Ekran distortion + elite spawn sesi |

### Tasarim Notu

Oyuncu UI'a bakmadan durumunu okuyabilmeli. Gorsel feedback olmadan Soul sistemi hissettirmiyor. Bu, en kritik erken implementasyon maddesi.

## Renk Paleti

- **Arka plan**: Koyu lacivert/siyah tonlari (#080a0f, #0d1018)
- **Soul Mor**: #7c5cfc (ana accent)
- **Soul Acik Mor**: #c084fc (ikincil accent)
- **Altin/Tehlike**: #f0b429
- **Turuncu/Uyari**: #fb923c
- **Kirmizi/Olum**: #e05252
- **Yesil/Pozitif**: #4ade80
- **Metin**: #c8d0e0 (ana), #6b7a99 (soluk)

## Kabul Kriterleri

- [ ] Karakter sprite'lari 16x16 veya 32x32
- [ ] 5 farkli aura efekti calisiyor
- [ ] Hunger partikuller gorsel olarak beliriyor
- [ ] Surge Warning vignette efekti calisiyor
- [ ] Overflow ekran distortion efekti calisiyor
- [ ] Oyuncu UI'a bakmadan state'ini okuyabiliyor

## Bagimliliklar

- [Soul System](soul-system.md) — 5 state'in gorsel temsili

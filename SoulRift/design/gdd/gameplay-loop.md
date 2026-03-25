# Gameplay Loop

## Genel Bakis

SOULRIFT'in dongusu dort katmandan olusur. Her katman bir ust katmani besler ve oyuncuyu surekli karar vermeye zorlar.

## Oyuncu Fantezisi

"Bir wave daha dayanabilirim" hissi. Her wave sonunda daha guclu donmek, ama bir sonraki wave'de o gucu yonetmek.

## Loop Katmanlari

| Katman | Sure | Aciklama |
|--------|------|----------|
| **Micro Loop** | ~10 sn | Hareket et, ates et, Ruh topla, Soul state takip et |
| **Wave Loop** | ~90 sn | Wave temizle → Shop → 3 item sec → Sinerji planla |
| **Run Loop** | ~25 dk | 15-20 wave, boss wave'leri, final boss |
| **Meta Loop** | Uzun vadeli | Unlock sistemi: yeni karakterler, item pool genislemesi, zorluk modlari |

## Micro Loop Detay

1. Oyuncu hareket eder ve ates eder (top-down twin-stick)
2. Dusmanlar olur, Ruh orblari duser
3. Oyuncu Ruh toplar → Soul state degisir
4. State'e gore hasar/hiz/gorsel efektler degisir
5. Oyuncu surekli "daha mi toplayayim, yoksa kacayim mi?" kararini verir

## Wave Loop Detay

1. Wave baslar — dusmanlar spawn olur
2. Oyuncu wave boyunca savasar, Ruh toplar
3. Wave biter → **Shop acilir**
4. 3 item secenegi sunulur
5. Oyuncu Ruh harcayarak item satin alir
6. Yeni wave baslar (zorluk artar)

## Shop Mekanigi

- Wave sonunda Ruh harcanarak item alinir
- **Hollow'dayken** fiyatlar **%30 ucuz** — Hollow'u kasitli kullanilan bir "ucuz alisveris ani"na donusturur
- **Surging veya Overflow'da** shop acilirsa: normal 3 item + 1 **Cursed Item slotu** eklenir
- Stable'da veya Hollow'da Cursed slot cikmaz (The Fractured haric)

## Run Loop Detay

- 15-20 wave, giderek artan zorluk
- Boss wave'leri (her 5. wave?)
- Final boss
- Run sonunda: skor, toplanan item'lar, istatistikler

## Meta Loop Detay

- Unlock sistemi: yeni karakterler
- Item pool genislemesi (yeni run'larda yeni item'lar)
- Zorluk modlari
- Istatistik takibi

## Formuller

| Parametre | Deger |
|-----------|-------|
| Wave suresi | ~90 sn (zorlukla degisir) |
| Wave sayisi (MVP) | 15 + Boss wave'ler |
| Shop item sayisi | 3 (+ 1 Cursed slotu kosullu) |
| Hollow indirim | %30 |
| Cursed item fiyat carpani | x2 (The Fractured: x1) |

## Kenar Durumlar

- Wave sirasinda Ruh 0'a dustugunde oyun bitmez (Hollow Hunger devreye girer)
- Shop'ta yeterli Ruh yoksa item alinamaz (skip secenegi)
- Overflow'da shop acildiginda Cursed slot eklenir ama satin alma zorunlu degil
- Boss wave'de shop yok, boss'tan sonra shop acilir

## Ayar Dugumleri

- Wave baslangic zorlugu
- Wave zorluk artis egrisi
- Shop item sayisi (3)
- Hollow indirim orani (%30)
- Cursed slot tetiklenme esigi (Surging+)
- Boss wave araligi
- Run suresi hedefi (25 dk)

## Kabul Kriterleri

- [ ] Wave'ler spawn oluyor ve zorluk artiyor
- [ ] Wave arasi shop aciliyor
- [ ] Shop'ta 3 item secenegi sunuluyor
- [ ] Hollow'da %30 indirim uygulanıyor
- [ ] Surging/Overflow'da Cursed slot ekleniyor
- [ ] Run 15-20 wave icerisinde tamamlanıyor
- [ ] Boss wave'ler calisiyor

## Bagimliliklar

- [Soul System](soul-system.md) — State'ler shop ve savaş mekanigini etkiler
- [Item Sinerjileri](items-synergies.md) — Shop'ta sunulan itemlar
- [Karakterler](characters.md) — Karakter pasifleri loop'u degistirir

# Karakter Tasarimi

## Genel Bakis

MVP icin **3 baslangic karakteri**. Her karakter Soul System'i farkli yorumlar. Brotato gibi her karakter baslangic item'i ve pasif ozelligiyle gelir.

## Oyuncu Fantezisi

Her karakter ayni Ruh sistemiyle tamamen farkli bir deneyim sunar. "Bu sefer Hollow oynayayim, bambaşka hissedecek."

## Karakter Tablosu

| Karakter | Pasif Ozellik | Zorluk | Playstyle |
|----------|---------------|--------|-----------|
| **The Vessel** | Ruh kapasitesi %50 artik. Surge Warning suresi +2 sn. | Kolay | Overflow'u kontrollu kullanmayi ogren. Yeni oyuncular icin. |
| **The Hollow** | Hollow state'de hasar normale esit. Hunger stack maks 5'e cikar (normal: 3). | Orta | Hollow'da kal, Hunger biriktir, patla. Burst damage playstyle. |
| **The Fractured** | Ruh seviyesi rastgele dalgalanir ama Cursed item slotu her zaman acik, fiyat x1. | Zor | Chaos build. Cursed item sinerjilerine odaklan. |

## The Vessel — Detay

### Tasarim Amaci
Yeni oyuncuya Soul System'i ogretmek. Genis Ruh kapasitesi ve uzun Warning suresi hata payı bırakır.

### Pasif: Genis Kap
- Ruh kapasitesi %50 artik (daha gec Overflow'a girer)
- Surge Warning suresi 3-4 sn yerine **5-6 sn**

### Baslangic Item'i
Belirlenmeli — muhtemelen bir Soul Anchor kategorisinden stabilize edici item.

### Ogrenme Egrisi
1. Stable'da guvenle oyna
2. Surging'in gucunu kesfet
3. Warning penceresinde bilinçli Overflow kararlari ver

## The Hollow — Detay

### Tasarim Amaci
Hunger mekaniğini merkeze alan, burst damage odakli bir playstyle. Orta seviye oyuncular icin.

### Pasif: Bos Kabuk
- Hollow state'de hasar normale esit (%50 dusuk degil, 1.0x)
- Hunger stack maksimum **5** (normal: 3)
- Stack doluyken oldurme: 5 x3 = **x15 Ruh patlamasi**

### Baslangic Item'i
Belirlenmeli — muhtemelen bir Harvester kategorisinden Ruh toplama item'i.

### Ogrenme Egrisi
1. Hollow'da kalmanin artik ceza olmadigini kesfet
2. Hunger stack ritimini ogren (2 sn aralikla)
3. Kasitli Hollow'a dusup stack biriktir, patlat, ucuz alisveris yap

## The Fractured — Detay

### Tasarim Amaci
Chaos/high-risk playstyle. Deneyimli oyuncular icin. Cursed item sinerjileri kesfetmeye tesvik eder.

### Pasif: Kirik Ruh
- Ruh seviyesi rastgele dalgalanir (kontrol azalir)
- Cursed item slotu **her zaman acik** (state bagimsiz)
- Cursed item fiyati **x1** (normal: x2)

### Baslangic Item'i
Belirlenmeli — muhtemelen bir Cursed kategorisinden item.

### Ogrenme Egrisi
1. Ruh dalgalanmasina adapte ol
2. Cursed item sinerjilerini kesfet
3. Kaotik state gecislerini avantaja cevir

## Formuller

| Parametre | Vessel | Hollow | Fractured |
|-----------|--------|--------|-----------|
| Ruh kapasitesi | %150 | %100 | %100 |
| Hollow hasar | 0.5x | 1.0x | 0.5x |
| Hunger stack maks | 3 | 5 | 3 |
| Surge Warning suresi | 5-6 sn | 3-4 sn | 3-4 sn |
| Cursed slot | Kosullu | Kosullu | Her zaman |
| Cursed fiyat | x2 | x2 | x1 |

## Kenar Durumlar

- The Fractured'in Ruh dalgalanmasi wave arasi (shop sirasinda) da devam eder mi?
- Bir karakter olup yeniden baslarsa pasif degisir mi? (meta unlock sistemi)
- Karakter pasifi ile item efekti cakisirsa hangisi oncelikli?

## Ayar Dugumleri

- Her karakter icin: Ruh kapasitesi, hasar carpanlari, ozel mekanik degerleri
- Baslangic item secimi
- Unlock kosullari (meta loop)

## Kabul Kriterleri

- [ ] 3 karakter secilebilir
- [ ] The Vessel: %50 artik kapasite ve +2 sn Warning calisiyor
- [ ] The Hollow: Hollow'da 1.0x hasar ve 5 stack calisiyor
- [ ] The Fractured: Ruh dalgalanmasi ve surekli Cursed slot calisiyor
- [ ] Her karakterin farkli hissi ve stratejisi var

## Bagimliliklar

- [Soul System](soul-system.md) — Karakterler Soul System'i modifiye eder
- [Item Sinerjileri](items-synergies.md) — Baslangic item'lari ve Cursed erisimi

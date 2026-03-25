# Data Manager

> **Status**: Tasarlandı
> **Son Guncelleme**: 2026-03-25
> **Katman**: Foundation
> **Oncelik**: MVP

## Genel Bakis

Data Manager, oyundaki tum gameplay verilerini (item istatistikleri, dusman konfigurasyonlari, Soul state esik degerleri, karakter pasifleri) ScriptableObject'ler uzerinden yoneten altyapi sistemidir. Kod icerisinde hardcoded deger bulunmaz — her deger bir data asset'ten okunur.

## Oyuncu Fantezisi

Oyuncu bu sistemi dogrudan gormez. Arka planda tum sayilarin tutarli ve dengelenmis olmasini saglar. Tasarimci icin: "Bir degeri degistirmek icin kod acmama gerek yok."

## Detayli Tasarim

### Temel Kurallar

1. Her gameplay degeri bir ScriptableObject asset'inde tanimlanir
2. MonoBehaviour'lar data'yi Inspector uzerinden veya runtime'da DataManager araciligiyla alir
3. Degerler Unity Editor'de degistirilebilir, build almadan test edilebilir
4. Tum data asset'leri `assets/data/` dizininde kategorize edilir

### Veri Tipleri

| ScriptableObject | Icerik | Dosya Yolu |
|------------------|--------|------------|
| `SoulStateData` | 5 state icin esik degerleri, hasar carpanlari, hiz carpanlari | `assets/data/soul/` |
| `HungerData` | Stack suresi, maks stack, Ruh carpani | `assets/data/soul/` |
| `ItemData` | Item adi, kategori, maliyet, efektler, aciklama | `assets/data/items/` |
| `EnemyData` | Dusman adi, HP, hasar, hiz, Ruh odul miktari | `assets/data/enemies/` |
| `CharacterData` | Karakter adi, pasif turu, pasif degerleri, baslangic item'i | `assets/data/characters/` |
| `WaveData` | Wave numarasi, dusman tipleri, spawn sayilari, zorluk carpani | `assets/data/waves/` |
| `ShopData` | Fiyat carpanlari, Hollow indirimi, Cursed slot kurallari | `assets/data/shop/` |

### Diger Sistemlerle Etkilesim

- **Soul System**: `SoulStateData` ve `HungerData` okur
- **Item System**: `ItemData` koleksiyonunu okur
- **Enemy System**: `EnemyData` koleksiyonunu okur
- **Character System**: `CharacterData` okur
- **Wave Manager**: `WaveData` listesini okur
- **Shop System**: `ShopData` ve `ItemData` okur

## Formuller

Bu sistem formul icermez — diger sistemlerin kullandigi degerleri depolar. Formuller ilgili sistem GDD'lerinde tanimlanir.

| Parametre | Tip | Ornek |
|-----------|-----|-------|
| Esik degerleri | float (0-1) | HollowThreshold = 0.25 |
| Carpanlar | float | SurgingDamageMultiplier = 1.75 |
| Sureler | float (saniye) | HungerStackInterval = 2.0 |
| Sayilar | int | MaxHungerStacks = 3 |
| Fiyatlar | float | BaseCursedCostMultiplier = 2.0 |

## Kenar Durumlar

- **Data asset bulunamazsa**: Console'a hata logla, varsayilan (fallback) deger kullan. Oyun crashlememeli.
- **Data asset bos alanlar iceriyorsa**: Validasyon sistemi Editor'de uyari gosterir (OnValidate).
- **Runtime'da data degisirse**: ScriptableObject'ler runtime'da degistirilebilir ama build'e yansimaz. Debug amacli kullanilabilir.
- **Ayni isimde iki item**: ItemData'da unique ID zorunlu (string veya enum).

## Bagimliliklar

| Yonu | Sistem | Arayuz |
|------|--------|--------|
| Bagimlilik yok | — | Foundation katmani, hicbir sisteme bagimli degil |
| Bagimli olan | Soul System | SoulStateData, HungerData okur |
| Bagimli olan | Item System | ItemData koleksiyonu okur |
| Bagimli olan | Enemy System | EnemyData koleksiyonu okur |
| Bagimli olan | Character System | CharacterData okur |
| Bagimli olan | Wave Manager | WaveData okur |
| Bagimli olan | Shop System | ShopData, ItemData okur |
| Bagimli olan | Combat/Weapon | Silah istatistikleri (ItemData uzerinden) |

## Ayar Dugumleri

Tum ayar dugumleri bu sistemin **iceriginde** tanimlanir. Her ScriptableObject'in alanlari birer ayar dugumudur. Ornekler:

| Dugum | SO Tipi | Guvenli Aralik | Etki |
|-------|---------|----------------|------|
| HollowThreshold | SoulStateData | 0.1 - 0.35 | Cok dusuk = Hollow'a girmek imkansiz |
| SurgingDamageMultiplier | SoulStateData | 1.2 - 2.5 | Cok yuksek = Surging OP |
| HungerStackInterval | HungerData | 1.0 - 4.0 sn | Cok kisa = Hollow trivial |
| BaseCursedCostMultiplier | ShopData | 1.5 - 3.0 | Cok yuksek = kimse Cursed almaz |
| HollowDiscountRate | ShopData | 0.15 - 0.50 | Cok yuksek = Hollow exploit |

## Kabul Kriterleri

- [ ] Her gameplay degeri bir ScriptableObject'ten okunuyor, hardcoded deger yok
- [ ] Tum data asset'leri `assets/data/` altinda kategorize edilmis
- [ ] Inspector'da deger degistirince oyun aninda tepki veriyor (Play mode)
- [ ] Eksik data asset'inde oyun crashlamiyor, fallback calisiyor
- [ ] OnValidate ile bos/gecersiz alanlar Editor'de uyari veriyor
- [ ] Her ItemData'da unique ID mevcut

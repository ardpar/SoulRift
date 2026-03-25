# Object Pool

> **Status**: Tasarlandi
> **Son Guncelleme**: 2026-03-25
> **Katman**: Foundation
> **Oncelik**: MVP

## Genel Bakis

Wave basina 50+ dusman, cok sayida mermi, Ruh orbu ve partikul efekti spawn eden bir roguelite'ta Instantiate/Destroy performans darbogazdir. Object Pool sistemi, Unity 6'nin yerlesik `ObjectPool<T>` API'sini kullanarak tum tekrar eden objeleri onbellekte tutar.

## Oyuncu Fantezisi

Oyuncu bu sistemi gormez. Ekranda 100 dusman + 200 mermi varken bile 60fps. "Bu oyun cok akici" hissi.

## Detayli Tasarim

### Temel Kurallar

1. `UnityEngine.Pool.ObjectPool<T>` kullanilir (Unity 6 yerlesik)
2. Her poollanabilir obje tipi icin ayri bir pool olusturulur
3. Pool'dan alinan objeler `SetActive(true)`, iade edilenler `SetActive(false)`
4. Pool kapasitesi asildiyinda yeni obje olusturulur (soft limit)
5. Pool'a iade edilmeyen objeler bellekte kalir — her obje mutlaka iade edilmeli

### Pool Kategorileri

| Kategori | Obje Tipleri | Baslangic Kapasitesi | Maks Kapasite |
|----------|-------------|---------------------|---------------|
| **Dusmanlar** | Her dusman tipi icin ayri pool | 20 | 100 |
| **Mermiler** | Oyuncu mermisi, dusman mermisi | 50 | 300 |
| **Ruh Orbleri** | SoulOrb | 30 | 150 |
| **VFX** | Hasar rakami, patlama, aura partikul | 20 | 100 |
| **Pickup** | Item drop, ozel pickup | 10 | 50 |

### Pool Lifecycle

```
Pool.Get() → obje.SetActive(true) → obje.OnSpawn() → [oyunda yasam] → obje.OnDespawn() → Pool.Release() → obje.SetActive(false)
```

### IPoolable Arayuzu

```csharp
public interface IPoolable {
    void OnSpawn();   // Pool'dan alindiginda: state reset, pozisyon ayarla
    void OnDespawn(); // Pool'a iade edildiginde: efektleri durdur, referanslari temizle
}
```

### Diger Sistemlerle Etkilesim

- **Enemy System**: Dusman spawn/despawn icin pool kullanir
- **Combat/Weapon**: Mermi spawn/despawn icin pool kullanir
- **Soul System**: Ruh orbu spawn icin pool kullanir
- **Soul VFX**: Partikul efektleri icin pool kullanir
- **Wave Manager**: Wave basinda pool'lari on-isitir (warmup)

## Formuller

| Parametre | Formul |
|-----------|--------|
| Warmup sayisi | `initialCapacity = maxWaveEnemyCount * 1.2` |
| Bellek tahmini | `memPerPool = objectSize * maxCapacity` |
| Performans hedefi | 0 GC allocation per wave (pool disinda Instantiate yok) |

## Kenar Durumlar

- **Pool bos ve maks kapasiteye ulasildi**: Yeni obje olustur (soft limit, hard crash yok)
- **Obje iade edilmeden sahne degisirse**: Sahne gecisinde tum pool'lar temizlenir
- **Ayni obje iki kez iade edilirse**: Guard check — zaten pool'daysa ignore et, log uyarisi
- **Pool'dan alinan obje hic iade edilmezse**: Bellek sizintisi — debug modda uyari timer'i

## Bagimliliklar

| Yonu | Sistem | Arayuz |
|------|--------|--------|
| Bagimlilik yok | — | Foundation katmani |
| Bagimli olan | Enemy System | PoolManager.Get<Enemy>(), PoolManager.Release() |
| Bagimli olan | Combat/Weapon | PoolManager.Get<Projectile>(), PoolManager.Release() |
| Bagimli olan | Soul System | PoolManager.Get<SoulOrb>(), PoolManager.Release() |
| Bagimli olan | Soul VFX | PoolManager.Get<VFXInstance>(), PoolManager.Release() |
| Bagimli olan | Wave Manager | PoolManager.Warmup<T>(count) |

## Ayar Dugumleri

| Dugum | Varsayilan | Guvenli Aralik | Etki |
|-------|-----------|----------------|------|
| Enemy pool baslangic | 20 | 10 - 50 | Cok dusuk = ilk wave'de stutter |
| Projectile pool baslangic | 50 | 20 - 100 | Cok dusuk = ates sirasinda stutter |
| SoulOrb pool baslangic | 30 | 10 - 60 | Cok dusuk = cok oldurmede stutter |
| Maks kapasite carpani | 5x baslangic | 2x - 10x | Cok dusuk = late-game obje olusturma |

## Kabul Kriterleri

- [ ] Instantiate/Destroy yerine pool kullaniliyor (0 GC allocation per wave)
- [ ] 50+ dusman ayni anda ekranda, 60fps koriiniyor
- [ ] Pool'dan alinan objeler dogru state ile spawn oluyor (OnSpawn)
- [ ] Iade edilen objeler tamamen resetleniyor (OnDespawn)
- [ ] Wave baslangicinda warmup calisarak ilk wave'de stutter yok
- [ ] Profiler'da GC spike yok (wave sirasinda)

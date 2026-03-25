# Sprint 1 — Core Prototype

> **Milestone**: Milestone 1 — Core Prototype
> **Baslangic**: 2026-03-25
> **Bitis**: 2026-04-07
> **Sure**: 2 hafta (14 gun)

## Sprint Hedefi

Oynanabilir bir prototip: oyuncu hareket eder, ates eder, dusmanlar spawn olur, Ruh toplanir, 5 Soul state calısır, Hunger stack birikir, Surge Warning gerilimi hissedilir.

## Kapasite

- Toplam gun: 14
- Buffer (%20): 3 gun (bug fix, beklenmedik sorunlar)
- Kullanilabilir: 11 gun

---

## Gorevler

### Must Have (Kritik Yol)

| ID | Gorev | Tahmini Gun | Bagimlilik | Kabul Kriteri |
|----|-------|-------------|------------|---------------|
| S1-01 | **Unity proje kurulumu** — URP 2D, Input System paketi, dizin yapisi, Assembly Definitions | 0.5 | — | Proje aciliyor, URP 2D Renderer aktif, dizin yapisi CLAUDE.md ile uyumlu |
| S1-02 | **Data Manager altyapisi** — ScriptableObject base class'lar: SoulStateData, HungerData, EnemyData | 0.5 | — | SO'lar olusturulabiliyor, Inspector'da duzenlenebiliyor |
| S1-03 | **Object Pool sistemi** — PoolManager + IPoolable arayuzu (Unity ObjectPool<T> uzerine) | 0.5 | — | Pool.Get/Release calisiyor, OnSpawn/OnDespawn tetikleniyor |
| S1-04 | **Input System kurulumu** — Input Actions asset, Gameplay action map (Move, Aim, Fire) | 0.5 | — | WASD hareket, mouse aim, sol tik ates action'lari calisiyor |
| S1-05 | **Player Controller** — Top-down hareket, carpismma, mouse'a dogru bak | 1 | S1-04 | 8 yonlu hareket, karakter mouse yonune bakiyor, duvarlarla carpisiyor |
| S1-06 | **Soul System — core** — CurrentSoul, MaxSoul, 5 state gecisi, event'ler | 1.5 | S1-02 | State gecisleri dogru esiklerde, hysteresis calisiyor, event'ler tetikleniyor |
| S1-07 | **Soul System — Hunger** — Hollow timer, stack birikim, patlama mekanigi | 1 | S1-06 | Hollow'da 2 sn'de stack, oldurme ile patlama, stack sifirlama |
| S1-08 | **Soul System — Surge Warning** — Warning timer, Overflow gecisi | 0.5 | S1-06 | Warning 3.5 sn, timer bitince Overflow, Ruh dusurulurse iptal |
| S1-09 | **Combat — temel ates** — Mermi spawn (pooled), mermi hareketi, dusmana hasar | 1 | S1-03, S1-04 | Mermi ates ediyor, dusmana carpinca hasar veriyor, pool'a donuyor |
| S1-10 | **Enemy — temel dusman** — 1 dusman tipi, oyuncuya dogru hareket, temas hasari, olunce Ruh orbu birak | 1 | S1-02, S1-03 | Dusman spawn, oyuncuya yuruyor, temas hasari, olunce SoulOrb biraikiyor |
| S1-11 | **Wave Spawner — basit** — Zamanla dusman spawn, wave numarasi, zorluk artisi (sayi) | 1 | S1-10 | Wave basliyor, dusmanlar spawn, her wave daha fazla dusman |
| S1-12 | **Ruh Orbu toplama** — Dusman oldugunde Ruh orbu duser, oyuncuya yakinlasinca toplanir | 0.5 | S1-03, S1-06 | Orb'ler duser, yakinlik radius'unda toplanir, CurrentSoul artar |

### Should Have

| ID | Gorev | Tahmini Gun | Bagimlilik | Kabul Kriteri |
|----|-------|-------------|------------|---------------|
| S1-13 | **Soul VFX — temel** — 5 state icin farkli aura rengi (basit partikul/sprite tint) | 0.5 | S1-06 | Her state'de karakter rengi/aurasi farkli |
| S1-14 | **HUD — Soul Meter** — Ekranda Ruh yuzde gostergesi, state renk gostergesi | 0.5 | S1-06 | Ruh miktari ekranda gorunuyor, state'e gore renk degisiyor |
| S1-15 | **Surge Warning VFX** — Ekran kenari turuncu vignette, basit ses efekti | 0.5 | S1-08 | Warning'de ekran kenari turuncu, Overflow'da kirmizi |
| S1-16 | **Hunger partikulleri** — Stack sayisina gore yogunlasan partikul efekti | 0.5 | S1-07 | 1 stack = az partikul, 3 stack = yogun partikul |

### Nice to Have

| ID | Gorev | Tahmini Gun | Bagimlilik | Kabul Kriteri |
|----|-------|-------------|------------|---------------|
| S1-17 | **Overflow olum efekti** — Hasar alinca patlama animasyonu + restart | 0.5 | S1-08 | Overflow'da hasar → patlama → game over ekrani |
| S1-18 | **Placeholder art** — Basit pixel karakter, dusman, mermi, Ruh orbu sprite'lari | 0.5 | — | Beyaz kutular yerine tanimlanbilir sprite'lar |
| S1-19 | **Wave sayaci HUD** — Mevcut wave numarasi ekranda | 0.25 | S1-11 | Wave numarasi gorunuyor |

---

## Toplam Tahmin

| Kategori | Gorev Sayisi | Toplam Gun |
|----------|-------------|------------|
| Must Have | 12 | 9.5 |
| Should Have | 4 | 2.0 |
| Nice to Have | 3 | 1.25 |
| **Toplam** | **19** | **12.75** |
| **Buffer** | — | **3** |
| **Kapasite** | — | **14** |

Must Have + Should Have = 11.5 gun — buffer dahil kapasiteye sigiyor.

---

## Riskler

| Risk | Olasilik | Etki | Onlem |
|------|----------|------|-------|
| Soul System "hissi" vermeyebilir | Yuksek | Yuksek | Sprint sonunda playtest, Hunger/Warning feedback'i erken ekle (S1-15, S1-16) |
| Unity 6.3 LTS bilinmeyen bug | Dusuk | Orta | Engine reference docs mevcut, web search ile dogrula |
| Hysteresis/state gecis buglari | Orta | Orta | Unit testler yaz (Soul System icin test-first yaklasiim) |
| Object pooling performans yetersiz | Dusuk | Dusuk | Profiler ile kontrol, warmup miktarini ayarla |

## Dis Bagimliliklar

- Unity 6.3 LTS kurulu olmali (Unity Hub)
- Git repo hazir (mevcut)

## Sprint Tamamlanma Tanimi

- [ ] Tum Must Have gorevleri tamamlandi
- [ ] Tum gorevler kabul kriterlerini karsiladı
- [ ] Oyuncu hareket edip ates edebiliyor
- [ ] Dusmanlar wave olarak spawn oluyor
- [ ] Ruh toplaniyor, 5 state calisiyor
- [ ] Hunger stack birikip patlıyor
- [ ] Surge Warning → Overflow gecisi calisiyor
- [ ] Overflow'da hasar = olum
- [ ] 60fps korunuyor (50+ dusman)
- [ ] Tasarim dokumanlari sapmalar icin guncellendi

---

## Uygulama Sirasi (Onerilen)

```
Gun 1:    S1-01 (proje kurulumu) + S1-02 (Data Manager) + S1-04 (Input)
Gun 2:    S1-03 (Object Pool) + S1-05 (Player Controller)
Gun 3-4:  S1-06 (Soul System core) — test-first, en kritik
Gun 5:    S1-07 (Hunger) + S1-08 (Warning)
Gun 6:    S1-09 (Combat — ates mekanigi)
Gun 7:    S1-10 (Enemy) + S1-12 (Ruh Orbu)
Gun 8:    S1-11 (Wave Spawner)
Gun 9:    S1-13 (Soul VFX) + S1-14 (HUD)
Gun 10:   S1-15 (Warning VFX) + S1-16 (Hunger partikul)
Gun 11:   S1-17 + S1-18 + S1-19 (Nice to Have)
Gun 12-14: Buffer — bug fix, balance tweak, playtest
```

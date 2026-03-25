# Wave System

> **Status**: Designed
> **Author**: user + claude
> **Last Updated**: 2026-03-25
> **Implements Pillar**: Core Loop — wave loop temposu ve zorluk ilerlemesi

## 1. Overview

**Wave System**, SOULRIFT'in dusman spawn zamanlamasini, wave yapisini, zorluk ilerlemesini ve boss wave'lerini yoneten sistemdir. Her run 15–20 wave'den olusur. Bir wave icinde dusmanlar sub-wave'ler halinde (3–4 kucuk dalga) spawn olur; tum dusmanlar oldurulunce wave biter ve shop acilir. Her 5. wave boss wave'dir — boss + az sayida normal dusman. Wave System oyunun temposunu belirler: cok hizli spawn = kaotik, cok yavas = sikici. Object Pool uzerinden dusman spawn eder, wave sonunda Shop System'i tetikler ve Run Manager'a ilerleme bilgisi saglar.

---

## 2. Player Fantasy

"Her dalga biraz daha zor, ama ben de biraz daha gucluyum." Oyuncu erken wave'lerde rahat hisseder, ortaya dogru baski artar ve "bu sefer yetisemeyebilirim" gerilimi baslar. Boss wave'leri "bu seferki sinav" hissi yaratir — hazirlaniyor musun? Wave arasi shop "nefes al ve planla" ani. Son wave'ler "ya hep ya hic" doruk noktasi.

---

## 3. Detailed Design

### 3.1 Core Rules

**W1. Run Yapisi**
- Bir run `total_waves` kadar wave'den olusur (varsayilan 18)
- Wave numaralari 1'den baslar
- Her 5. wave (5, 10, 15) boss wave'dir
- Son wave (18) final boss wave'dir
- Wave 18 temizlenince run kazanilir → Victory ekrani

**W2. Wave Akisi**
```
Wave baslar → Sub-wave 1 spawn → Dusmanlar oldurulur →
Sub-wave 2 spawn → ... → Son sub-wave temizlenir →
Wave tamamlandi → Shop acilir → Shop kapatilir → Sonraki wave
```

**W3. Sub-Wave Sistemi**
- Her wave `sub_wave_count` kadar sub-wave iceriyor (varsayilan 3)
- Sub-wave arasi `sub_wave_delay` bekleme suresi (varsayilan 2.0 sn)
- Sonraki sub-wave, onceki sub-wave'deki dusmanlarin %50'si oldurulunce tetiklenir VEYA delay suresi dolunca — hangisi once olursa
- Bu sayede oyuncuya kisa "nefes alma" ani verilirken tempo korunur

**W4. Dusman Spawn**
- Dusmanlar arena kenarlarindan spawn olur (ekran disi, kenardan 1–2 birim iceri)
- Spawn pozisyonu rastgele ama oyuncudan minimum `min_spawn_distance` (varsayilan 8 birim) uzakta
- Her sub-wave icindeki dusman sayisi ve tipleri `WaveData` ScriptableObject'ten okunur
- Object Pool'dan alinir, wave sonunda kullanilmayan dusmanlar pool'a doner

**W5. Wave Tamamlanma**
- Wave'deki tum dusmanlar (tum sub-wave'ler dahil) oldurulunce wave tamamlanir
- `OnWaveCompleted(waveNumber)` event'i tetiklenir
- Shop System bu event'i dinler ve shop'u acar

**W6. Boss Wave'leri**
- Boss wave'lerde normal sub-wave yapisi yerine: boss spawn + 2 sub-wave az sayida normal dusman
- Boss spawn wave basinda tek basina gelir (drama ani)
- Normal dusmanlar boss spawn'dan 3 sn sonra kucuk dalgalar halinde gelir
- Boss olunce wave tamamlanir (kalan normal dusmanlar da yok olur + soul drop verir)

**W7. Zorluk Ilerlemesi**
- Her wave numarasiyla zorluk artar:
  - Dusman sayisi artar
  - Dusman tipleri cesitlenir (gec wave'lerde daha zor tipler eklenir)
  - Dusman HP ve hasar ölceklenir
- Zorluk parametreleri `DifficultyConfig` ScriptableObject'ten okunur

### 3.2 States and Transitions

| State | Kosul | Davranis |
|-------|-------|----------|
| **WaveActive** | Dusmanlar hayatta | Sub-wave'ler spawn oluyor, savas devam |
| **SubWaveDelay** | Onceki sub-wave %50 temiz, sonraki henuz gelmedi | Kisa bekleme, timer calisiyor |
| **WaveComplete** | Tum dusmanlar oldu | OnWaveCompleted tetiklenir |
| **Shopping** | Shop acik | Yeni wave baslamaz, oyuncu item secer |
| **BossIntro** | Boss wave, boss spawn ani | Boss tek basina spawn, 3 sn drama suresi |
| **Victory** | Son wave tamamlandi | Run kazanildi |

| Gecis | Tetikleyici |
|-------|-------------|
| Shopping → WaveActive (veya BossIntro) | Shop kapatildi |
| WaveActive → SubWaveDelay | Sub-wave %50 temiz VE sonraki sub-wave var |
| SubWaveDelay → WaveActive | Delay suresi doldu VEYA onceki sub-wave %50 temiz |
| WaveActive → WaveComplete | Tum dusmanlar oldu |
| WaveComplete → Shopping | Otomatik gecis |
| BossIntro → WaveActive | 3 sn drama suresi doldu, normal dusmanlar spawn baslar |
| WaveComplete → Victory | waveNumber == total_waves |

### 3.3 Interactions with Other Systems

| Sistem | Yon | Veri Akisi | Arayuz |
|--------|-----|-----------|--------|
| **Object Pool** | Upstream | Dusman instance spawn/despawn | `GetFromPool<Enemy>(enemyType)`, `ReturnToPool(enemy)` |
| **Combat System** | Upstream | Dusman olduruldu bilgisi | `OnEnemyKilled(enemyData)` — wave ilerlemesi icin sayim |
| **Shop System** | Downstream | Wave tamamlandi event'i | `OnWaveCompleted(waveNumber)` → shop acar |
| **Run Manager** | Downstream | Wave numarasi, toplam wave, victory | `GetCurrentWave()`, `GetTotalWaves()`, `OnRunVictory` |
| **Enemy AI** | Downstream | Spawn edilen dusman referanslari | Wave System dusmanlari spawn eder, AI sistemi davranislarini yonetir |
| **UI/HUD** | Downstream | Wave numarasi, kalan dusman | `GetCurrentWave()`, `GetRemainingEnemies()`, `OnWaveCompleted` |
| **Soul System** | Indirect | Boss olumunde buyuk soul drop | `enemyData.soul_drop` (boss icin yuksek) |

---

## 4. Formulas

### WF1. Wave Basi Dusman Sayisi
```
base_enemy_count = wave_base_enemies + (wave_number - 1) * enemies_per_wave_increase
sub_wave_enemy_count = ceil(base_enemy_count / sub_wave_count)
```

**Degiskenler:**
- `wave_base_enemies`: int, varsayilan 15 (ilk wave)
- `enemies_per_wave_increase`: int, varsayilan 5 (her wave +5)
- `sub_wave_count`: int, varsayilan 3

| Wave | Toplam Dusman | Sub-Wave Basina |
|------|--------------|----------------|
| 1 | 15 | 5 |
| 5 (boss) | 35 + boss | 12 + boss |
| 10 (boss) | 60 + boss | 20 + boss |
| 15 (boss) | 85 + boss | 28 + boss |
| 18 (final) | 100 + final boss | 33 + final boss |

### WF2. Dusman HP Olceklemesi
```
enemy_hp = base_hp * (1 + (wave_number - 1) * hp_scale_per_wave)
```

- `hp_scale_per_wave`: float, varsayilan 0.08 (wave basina %8 HP artisi)
- Wave 1: base_hp * 1.0
- Wave 10: base_hp * 1.72
- Wave 18: base_hp * 2.36

### WF3. Dusman Hasar Olceklemesi
```
enemy_damage = base_damage * (1 + (wave_number - 1) * damage_scale_per_wave)
```

- `damage_scale_per_wave`: float, varsayilan 0.05 (wave basina %5 hasar artisi)

### WF4. Wave Suresi Tahmini
```
estimated_wave_duration = total_enemies * avg_kill_time + (sub_wave_count - 1) * sub_wave_delay
```

- `avg_kill_time`: ~1.5 sn/dusman (tahmini, balansa bagimli)
- Wave 1: 15 * 1.5 + 2 * 2 = ~26.5 sn
- Wave 10: 60 * 1.5 + 2 * 2 = ~94 sn
- Hedef: ortalama ~90 sn/wave (game concept ile uyumlu)

### WF5. Dusman Tip Dagilimi
```
wave 1-4:   %100 Kucuk
wave 5-8:   %70 Kucuk, %30 Orta
wave 9-12:  %50 Kucuk, %35 Orta, %15 Buyuk
wave 13-18: %30 Kucuk, %35 Orta, %25 Buyuk, %10 Elite
```

---

## 5. Edge Cases

| # | Durum | Ne Olur | Gerekce |
|---|-------|---------|---------|
| WE1 | Oyuncu tum dusmanlari cok hizli oldurur (wave < 30 sn) | Wave biter, shop acilir. Zorluk otomatik artacak (sonraki wave daha fazla dusman). | Skill odulu — hizli oyuncu cezalandirilmaz |
| WE2 | Oyuncu cok yavas (wave > 3 dk) | Sorun yok, tum dusmanlar olene kadar bekler. Timeout yok. | Hollow stratejisi icin zaman gerekebilir |
| WE3 | Boss wave'de boss olur, normal dusmanlar kalir | Kalan dusmanlar aninda yok olur + soul drop verir. Wave tamamlanir. | Boss olumu = wave odulu, kalan temizlik gerekmez |
| WE4 | Oyuncu boss wave'de olur | Normal olum — Run Manager permadeath uygular. Boss wave ozel degil. | Roguelite kurali |
| WE5 | Sub-wave arasi delay'de oyuncu Hollow'da Hunger biriktir | Normal — delay suresi Hunger stack firsati. Kasitli tasarim. | Hunger + wave tempo etkilesimi |
| WE6 | 50+ dusman ayni anda aktif | Object Pool max boyutu asarsa: eski dusmanlar force despawn (pool recycle). Performans butcesi. | 50+ dusman performans siniri |
| WE7 | Son wave (18) tamamlandi | `OnRunVictory` tetiklenir. Shop acilmaz, Victory ekrani gosterilir. | Run sonu |
| WE8 | Spawn pozisyonu oyuncuya cok yakin | `min_spawn_distance` kontrolu. Eger arena cok kucukse ve oyuncu ortadaysa: kenarlarda spawn (her zaman kenardan gelir). | Unfair spawn onlenir |

---

## 6. Dependencies

### Hard Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Object Pool** | Upstream | `GetFromPool<Enemy>(type)`, `ReturnToPool(enemy)` | Dusman spawn/despawn — bu olmadan wave calismaz |

### Soft Dependencies

| Sistem | Yon | Arayuz | Notlar |
|--------|-----|--------|--------|
| **Combat System** | Upstream | `OnEnemyKilled(enemyData)` | Dusman olum sayimi. Olmazsa wave bitmez — test icin manuel kill. |
| **Shop System** | Downstream | `OnWaveCompleted(waveNumber)` | Wave sonunda shop acma |
| **Run Manager** | Downstream | `GetCurrentWave()`, `OnRunVictory` | Ilerleme takibi |
| **Enemy AI** | Downstream | Spawn edilen dusmanlar | Wave spawn eder, AI davranislari yonetir |
| **UI/HUD** | Downstream | `GetCurrentWave()`, `GetRemainingEnemies()` | Wave gostergesi |

---

## 7. Tuning Knobs

Tum degerler `WaveConfig` ve `DifficultyConfig` ScriptableObject'leri uzerinden.

| Parametre | Varsayilan | Guvenli Aralik | Cok Dusukse | Cok Yuksekse | Etkilesim |
|-----------|------------|----------------|-------------|-------------|-----------|
| `total_waves` | 18 | 12–25 | Run cok kisa (<20 dk) | Run cok uzun (>35 dk) | Session suresi hedefi |
| `wave_base_enemies` | 15 | 8–25 | Ilk wave cok kolay | Ilk wave bunaltici | Yeni oyuncu deneyimi |
| `enemies_per_wave_increase` | 5 | 2–10 | Zorluk yavas artar, sıkıcı | Zorluk cok hızlı artar | Gec wave performansi |
| `sub_wave_count` | 3 | 2–5 | Az dalga, buyuk kalabaliklar | Cok dalga, kucuk gruplar | Tempo hissi |
| `sub_wave_delay` | 2.0 sn | 0.5–4.0 | Neredeyse surekliler spawn | Cok uzun bos sure | Nefes alma ani |
| `sub_wave_kill_threshold` | 0.5 | 0.3–0.8 | Sonraki dalga erken gelir | Neredeyse hepsini oldurmen lazim | Baski hissi |
| `min_spawn_distance` | 8 birim | 5–12 | Dusmanlar cok yakin spawn | Dusmanlar cok uzak, yaklasma suresi uzun | Arena boyutu |
| `hp_scale_per_wave` | 0.08 | 0.03–0.15 | Gec wave dusmanlar kolay | Gec wave dusmanlar cok dayanikli | Silah DPS dengesi |
| `damage_scale_per_wave` | 0.05 | 0.02–0.10 | Gec wave hasar dusuk | Gec wave tek vurus olum | Soul loss dengesi |
| `boss_intro_delay` | 3.0 sn | 1.0–5.0 | Boss gelisi dramatik degil | Beklemek sikici | Boss presentation |
| `boss_wave_interval` | 5 | 3–7 | Cok sik boss, ozel hissetmez | Boss cok nadir | Boss olceklemesi |

---

## 8. Acceptance Criteria

### Fonksiyonel Testler

| # | Test | Beklenen Sonuc | Oncelik |
|---|------|----------------|---------|
| WT1 | Wave 1 baslar | 15 dusman, 3 sub-wave halinde spawn | P0 |
| WT2 | Sub-wave gecisi | %50 dusman olunce 2 sn sonra sonraki sub-wave | P0 |
| WT3 | Tum dusmanlar oldu | OnWaveCompleted tetiklenir, shop acilir | P0 |
| WT4 | Wave 5 = boss wave | Boss spawn + kucuk normal dalga | P0 |
| WT5 | Boss olunce kalan dusmanlar | Aninda yok olur + soul drop | P0 |
| WT6 | Wave 18 (final) tamamlandi | OnRunVictory tetiklenir | P0 |
| WT7 | Zorluk olceklemesi wave 10 | Dusman HP ~1.72x, dusman sayisi ~60 | P1 |
| WT8 | Spawn mesafesi | Dusmanlar min 8 birim uzakta spawn | P1 |
| WT9 | Object Pool siniri | 50+ aktif dusman performans kabul edilebilir | P1 |
| WT10 | Sub-wave delay'de Hunger stack | Stack biriktirme calisir | P1 |

### Performans Butcesi

| Metrik | Butce |
|--------|-------|
| Max aktif dusman | 60 (Object Pool limit) |
| Spawn islemi | < 0.5 ms / sub-wave spawn |
| Wave state guncelleme | < 0.1 ms / frame |

### Playtest Kriterleri

| # | Kriter | Olcum |
|---|--------|-------|
| WP1 | Wave temposu dogru hissediyor | Wave suresi ~60-120 sn araliginda mi? |
| WP2 | Sub-wave arasi nefes alma ani yeterli | Oyuncu 2 sn'de durumu degerlendirebildi mi? |
| WP3 | Boss wave ozel hissediyor | Boss girisi drama yaratiyor mu? |
| WP4 | Gec wave'ler zorluyor ama adil | Oyuncu "haksiz" degil "yetersiz kaldim" mi diyor? |

---

## 9. Visual/Audio Requirements

### Gorsel

| Eleman | Detay |
|--------|-------|
| **Wave baslangic** | Ekran ortasinda "WAVE [N]" text (1.5 sn, fade in/out) |
| **Boss wave baslangic** | "BOSS WAVE" buyuk kirmizi text + screen shake |
| **Sub-wave spawn** | Dusmanlar spawn noktasinda kisa flash efekti |
| **Wave tamamlandi** | "WAVE CLEAR" yesil text (1 sn) |
| **Victory** | Ozel victory ekrani (ayri UI) |

### Ses

| Olay | Ses |
|------|-----|
| Wave baslangic | Kisa horn/alarm |
| Sub-wave spawn | Dusuk "whoosh" |
| Boss wave baslangic | Agir, dramatik intro stinger |
| Wave tamamlandi | Tatmin edici completion chime |
| Victory | Zafer fanfari |

---

## 10. UI Requirements

### Wave Gostergesi
- Ekranin ust ortasinda: "WAVE 7 / 18"
- Boss wave'lerde ozel ikon (kafatasi veya benzeri)

### Kalan Dusman Sayaci
- Wave gostergesinin altinda kucuk text: "Remaining: 23"
- Son 5 dusmanda sayi kirmiziya doner

### Sub-Wave Bildirimi
- Yeni sub-wave geldiginde kisa "INCOMING" text (0.5 sn, kucuk)

### Boss HP Bar
- Boss wave'lerde ekranin ustunde buyuk boss HP bari
- Boss ismi gosterilir

---

## 11. Open Questions

| # | Soru | Sahip | Hedef Cozum |
|---|------|-------|-------------|
| WQ1 | Arena boyutu wave'e gore degismeli mi (gec wave = buyuk arena)? | Level Designer | Post-MVP |
| WQ2 | "Endless mode" (wave 18 sonrasi devam) eklenecek mi? | Game Designer | Post-MVP meta content |
| WQ3 | Dusman spawn animasyonu (portal?) veya direkt mi? | Art Director | Hafta 5-6 art pass |
| WQ4 | Wave arasi shop suresi sinirli mi yoksa oyuncu istediginde mi kapatir? | Game Designer | Hafta 3-4 playtest |

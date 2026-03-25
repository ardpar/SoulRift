# SOULRIFT — Sistem Indeksi

Oyunun tum teknik sistemlerinin envanteri, bagimliliklari, oncelikleri ve tasarim durumu.

## Sistem Envanteri

### Foundation (Altyapi) — Bagimliligi Olmayan Sistemler

| # | Sistem | Aciklama | Oncelik | GDD Durumu |
|---|--------|----------|---------|------------|
| 1 | Input System | Unity Input System entegrasyonu, rebindable kontroller | MVP | Baslanmadi |
| 2 | Object Pool | Dusman, mermi, Ruh orbu, partikul pooling (50+ obje/wave) | MVP | Baslanmadi |
| 3 | Data Manager | ScriptableObject tabanli veri yonetimi (item stats, enemy configs, soul thresholds) | MVP | Baslanmadi |

### Core — Foundation'a Bagli Sistemler

| # | Sistem | Bagimliliklari | Oncelik | GDD Durumu |
|---|--------|----------------|---------|------------|
| 4 | Player Controller | Input System | MVP | Baslanmadi |
| 5 | Soul System | Data Manager | MVP | Baslanmadi |
| 6 | Combat / Weapon System | Input System, Object Pool, Data Manager | MVP | Baslanmadi |
| 7 | Enemy System | Object Pool, Data Manager | MVP | Baslanmadi |

### Feature — Core'a Bagli Sistemler

| # | Sistem | Bagimliliklari | Oncelik | GDD Durumu |
|---|--------|----------------|---------|------------|
| 8 | Wave Manager | Enemy System, Soul System | MVP | Baslanmadi |
| 9 | Item System | Soul System, Data Manager | MVP | Baslanmadi |
| 10 | Shop System | Item System, Soul System | MVP | Baslanmadi |
| 11 | Character System | Soul System, Item System | MVP | Baslanmadi |

### Presentation — Gameplay Sistemlerini Saran Katman

| # | Sistem | Bagimliliklari | Oncelik | GDD Durumu |
|---|--------|----------------|---------|------------|
| 12 | Soul VFX System | Soul System | MVP | Baslanmadi |
| 13 | HUD / UI System | Soul System, Item System, Wave Manager | MVP | Baslanmadi |
| 14 | Audio System | Soul System, Wave Manager | Alpha | Baslanmadi |
| 15 | Camera System | Player Controller, Soul System | Alpha | Baslanmadi |

### Polish (Meta) — En Son Tasarlanan Sistemler

| # | Sistem | Bagimliliklari | Oncelik | GDD Durumu |
|---|--------|----------------|---------|------------|
| 16 | Run Manager | Wave Manager, Soul System, Character System | MVP | Baslanmadi |
| 17 | Progression / Unlock | Run Manager, Character System, Item System | Alpha | Baslanmadi |
| 18 | Save System | Progression / Unlock | Alpha | Baslanmadi |

---

## Bagimlilik Haritasi

```
Katman 0 — Foundation (bagimliligi yok):
  Input System, Object Pool, Data Manager

Katman 1 — Core (Foundation'a bagli):
  Player Controller ← Input System
  Soul System ← Data Manager
  Combat/Weapon ← Input System, Object Pool, Data Manager
  Enemy System ← Object Pool, Data Manager

Katman 2 — Feature (Core'a bagli):
  Wave Manager ← Enemy System, Soul System
  Item System ← Soul System, Data Manager
  Shop System ← Item System, Soul System
  Character System ← Soul System, Item System

Katman 3 — Presentation (Feature'a bagli):
  Soul VFX ← Soul System
  HUD/UI ← Soul System, Item System, Wave Manager
  Audio ← Soul System, Wave Manager
  Camera ← Player Controller, Soul System

Katman 4 — Meta:
  Run Manager ← Wave Manager, Soul System, Character System
  Progression ← Run Manager, Character System, Item System
  Save System ← Progression
```

### Yuksek Riskli (Darboğaz) Sistemler

- **Soul System** — 10 sistem buna bagimli. Yanlis tasarlanirsa domino etkisi olusur.
- **Data Manager** — 5 sistem buna bagimli. Veri yapisi erken sabitlenmeli.
- **Object Pool** — Performansin temeli. Hafta 1'de kurulmali.

### Dairesel Bagimlilik

Tespit edilmedi. Graf temiz.

---

## Oncelik Katmanlari

### MVP (Hafta 1-6) — 13 sistem

| Sira | Sistem | Neden MVP? |
|------|--------|------------|
| 1 | Input System | Hareket ve ates icin sart |
| 2 | Object Pool | Performans temeli, wave spawning icin sart |
| 3 | Data Manager | Tum gameplay verileri buradan okunur |
| 4 | Player Controller | Oyuncunun dunyayla etkilesimi |
| 5 | Soul System | Core mechanic — her sey bunun uzerine kurulu |
| 6 | Combat / Weapon | Dusmanlara hasar vermek |
| 7 | Enemy System | Dusmanlar olmadan wave yok |
| 8 | Wave Manager | Wave dongusu — core loop |
| 9 | Item System | 40-50 item, 4 kategori, sinerjiler |
| 10 | Shop System | Wave arasi item satin alma |
| 11 | Character System | 3 karakter, pasif yetenekler |
| 12 | Soul VFX System | Gorsel feedback — Soul hissi icin kritik |
| 13 | HUD / UI System | Soul meter, shop UI, wave sayaci |

### Alpha (Hafta 7-8) — 3 sistem

| Sira | Sistem | Neden Alpha? |
|------|--------|-------------|
| 14 | Audio System | Ses efektleri ve muzik — polish asamasi |
| 15 | Camera System | Distortion, shake — polish asamasi |
| 16 | Run Manager | Run baslat/bitir, skor — game loop tamamlandiktan sonra |

### Full Vision (Hafta 9+) — 2 sistem

| Sira | Sistem | Neden Full Vision? |
|------|--------|-------------------|
| 17 | Progression / Unlock | Meta loop — MVP sonrasi |
| 18 | Save System | Unlock ilerlemesi kaydetme — Progression'a bagli |

---

## Onerilen Tasarim Sirasi

Bagimlilik + oncelik birlesimi. GDD'ler bu sirada yazilmali:

1. Data Manager
2. Input System
3. Object Pool
4. Soul System ← **en kritik, en cok bagimliligi olan**
5. Player Controller
6. Combat / Weapon System
7. Enemy System
8. Wave Manager
9. Item System
10. Shop System
11. Character System
12. Soul VFX System
13. HUD / UI System
14. Run Manager
15. Camera System
16. Audio System
17. Progression / Unlock
18. Save System

---

## Ilerleme Takibi

| # | Sistem | GDD | Implementasyon | Test |
|---|--------|-----|----------------|------|
| 1 | Data Manager | Tasarlandi | — | — |
| 2 | Input System | Tasarlandi | — | — |
| 3 | Object Pool | Tasarlandi | — | — |
| 4 | Soul System | Tasarlandi | — | — |
| 5 | Player Controller | Baslanmadi | — | — |
| 6 | Combat / Weapon | Baslanmadi | — | — |
| 7 | Enemy System | Baslanmadi | — | — |
| 8 | Wave Manager | Baslanmadi | — | — |
| 9 | Item System | Baslanmadi | — | — |
| 10 | Shop System | Baslanmadi | — | — |
| 11 | Character System | Baslanmadi | — | — |
| 12 | Soul VFX | Baslanmadi | — | — |
| 13 | HUD / UI | Baslanmadi | — | — |
| 14 | Run Manager | Baslanmadi | — | — |
| 15 | Camera System | Baslanmadi | — | — |
| 16 | Audio System | Baslanmadi | — | — |
| 17 | Progression | Baslanmadi | — | — |
| 18 | Save System | Baslanmadi | — | — |

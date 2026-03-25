# Systems Index: SOULRIFT

> **Status**: Approved
> **Created**: 2026-03-25
> **Last Updated**: 2026-03-25
> **Source Concept**: design/gdd/game-concept.md

---

## Overview

SOULRIFT, tek bir kaynak (Soul) etrafinda donen bir bullet-heaven roguelite. Tum sistemler Soul System'a bagimli — Soul hem can, hem para, hem guc kaynagi. Core loop: hareket et → ates et → Ruh topla → state degisir → wave bitir → shop'ta item al → tekrarla. 10 haftalik MVP'de 18 MVP sistemi, 4 presentation sistemi ve 2 post-MVP sistemi olmak uzere toplam 24 sistem tanimlanmistir.

---

## Systems Enumeration

| # | System Name | Category | Priority | Status | Design Doc | Depends On |
|---|-------------|----------|----------|--------|------------|------------|
| 1 | Input System | Core | MVP | Not Started | — | — |
| 2 | Object Pool | Core | MVP | Not Started | — | — |
| 3 | Player Movement | Core | MVP | Not Started | — | Input System |
| 4 | Collision/Pickup (inferred) | Core | MVP | Not Started | — | Player Movement |
| 5 | Soul System | Gameplay | MVP | Designed | design/gdd/soul-system.md | Collision/Pickup, Player Movement |
| 6 | Projectile System (inferred) | Core | MVP | Not Started | — | Object Pool, Input System |
| 7 | Combat/Shooting | Gameplay | MVP | Designed | design/gdd/combat-system.md | Projectile System, Player Movement |
| 8 | Wave Spawner | Gameplay | MVP | Designed | design/gdd/wave-system.md | Object Pool |
| 9 | Soul Pickup/Drop (inferred) | Gameplay | MVP | Not Started | — | Soul System, Object Pool, Collision/Pickup |
| 10 | Health/Damage (inferred) | Gameplay | MVP | Designed | design/gdd/health-damage.md | Soul System, Combat/Shooting |
| 11 | Hunger System | Gameplay | MVP | Designed | design/gdd/hunger-system.md | Soul System |
| 12 | Surge Warning | Gameplay | MVP | Designed | design/gdd/surge-warning.md | Soul System |
| 13 | Run Manager (inferred) | Gameplay | MVP | Not Started | — | Soul System, Wave Spawner |
| 14 | Enemy AI | Gameplay | MVP | Not Started | — | Soul System, Wave Spawner |
| 15 | Shop System | Economy | MVP | Designed | design/gdd/shop-system.md | Soul System, Wave Spawner, Run Manager |
| 16 | Item System | Economy | MVP | Not Started | — | Shop System, Soul System |
| 17 | Cursed Item System | Economy | MVP | Not Started | — | Item System, Soul System |
| 18 | Character System | Gameplay | MVP | Not Started | — | Soul System, Hunger System, Item System |
| 19 | VFX/Aura System | Presentation | Vertical Slice | Not Started | — | Soul System, Hunger System, Surge Warning |
| 20 | Camera System (inferred) | Presentation | Vertical Slice | Not Started | — | Soul System, Surge Warning |
| 21 | UI/HUD System | UI | Vertical Slice | Not Started | — | Soul System, Shop System, Wave Spawner, Hunger System |
| 22 | Audio System | Audio | Vertical Slice | Not Started | — | Soul System, Surge Warning, VFX/Aura System |
| 23 | Meta Progression | Progression | Full Vision | Not Started | — | Character System, Item System, Run Manager |
| 24 | Save/Persistence | Persistence | Full Vision | Not Started | — | Meta Progression |

---

## Categories

| Category | Description |
|----------|-------------|
| **Core** | Foundation systems everything depends on — input, movement, pooling, collision, projectiles |
| **Gameplay** | The systems that make SOULRIFT fun — Soul, Hunger, Combat, Waves, Characters, AI |
| **Economy** | Resource creation and consumption — Shop, Items, Cursed Items (all Soul-based) |
| **Presentation** | Visual and camera feedback — VFX/Aura, Camera effects |
| **UI** | Player-facing information — HUD, Soul meter, Shop UI |
| **Audio** | Sound systems — SFX, music, Surge Warning ugultusu |
| **Progression** | Meta-game growth — unlocks, item pool genislemesi |
| **Persistence** | Save state — meta unlock kaydi |

---

## Priority Tiers

| Tier | Definition | Target Milestone | System Count |
|------|------------|------------------|--------------|
| **MVP** | Core loop calisir, "bu eglenceli mi?" test edilebilir | Hafta 1–6 | 18 |
| **Vertical Slice** | Tam deneyim, gorsel/ses polish | Hafta 5–8 | 4 |
| **Full Vision** | Meta, save, post-launch | Post-MVP | 2 |

---

## Dependency Map

### Foundation Layer (no dependencies)

1. **Input System** — Her sistem input'a bagimli, Unity Input System konfigurasyonu
2. **Object Pool** — 50+ dusman/wave performans butcesi, mermi ve pickup havuzu

### Core Foundation Layer (depends on foundation)

3. **Player Movement** — depends on: Input System
4. **Collision/Pickup** — depends on: Player Movement
5. **Projectile System** — depends on: Object Pool, Input System
6. **Wave Spawner** — depends on: Object Pool

### Core Systems Layer (depends on core foundation)

7. **Soul System** — depends on: Collision/Pickup, Player Movement — **DARBOAZ: 15+ sistem buna bagimli**
8. **Combat/Shooting** — depends on: Projectile System, Player Movement

### Soul Subsystems Layer (depends on Soul System)

9. **Soul Pickup/Drop** — depends on: Soul System, Object Pool, Collision/Pickup
10. **Health/Damage** — depends on: Soul System, Combat/Shooting
11. **Hunger System** — depends on: Soul System
12. **Surge Warning** — depends on: Soul System
13. **Run Manager** — depends on: Soul System, Wave Spawner
14. **Enemy AI** — depends on: Soul System, Wave Spawner

### Feature Layer (depends on Soul subsystems)

15. **Shop System** — depends on: Soul System, Wave Spawner, Run Manager
16. **Item System** — depends on: Shop System, Soul System
17. **Cursed Item System** — depends on: Item System, Soul System
18. **Character System** — depends on: Soul System, Hunger System, Item System

### Presentation Layer (depends on gameplay)

19. **VFX/Aura System** — depends on: Soul System, Hunger System, Surge Warning
20. **Camera System** — depends on: Soul System, Surge Warning
21. **UI/HUD System** — depends on: Soul System, Shop System, Wave Spawner, Hunger System
22. **Audio System** — depends on: Soul System, Surge Warning, VFX/Aura System

### Polish Layer (post-MVP)

23. **Meta Progression** — depends on: Character System, Item System, Run Manager
24. **Save/Persistence** — depends on: Meta Progression

---

## Recommended Design Order

| Order | System | Priority | Layer | Est. Effort |
|-------|--------|----------|-------|-------------|
| 1 | Soul System | MVP | Core | L |
| 2 | Hunger System | MVP | Soul Subsystem | M |
| 3 | Surge Warning | MVP | Soul Subsystem | M |
| 4 | Combat/Shooting + Projectile | MVP | Core | M |
| 5 | Wave System | MVP | Core | M |
| 6 | Health/Damage | MVP | Soul Subsystem | S |
| 7 | Shop System | MVP | Feature | M |
| 8 | Item System | MVP | Feature | L |
| 9 | Cursed Item System | MVP | Feature | S |
| 10 | Character System | MVP | Feature | M |
| 11 | Enemy AI | MVP | Soul Subsystem | M |
| 12 | VFX/Aura System | Vertical Slice | Presentation | M |
| 13 | UI/HUD System | Vertical Slice | Presentation | M |
| 14 | Audio System | Vertical Slice | Presentation | S |

*Effort: S = 1 session, M = 2–3 sessions, L = 4+ sessions*

**Not:** Input System, Object Pool, Player Movement, Collision/Pickup, Soul Pickup/Drop ve Run Manager icin ayri GDD yazilmayacak — bunlar teknik implementasyon detayi olup mimari kararlarda (ADR) tanimlanacak.

---

## Circular Dependencies

- Tespit edilmedi. Bagimlilik grafi temiz.

---

## High-Risk Systems

| System | Risk Type | Risk Description | Mitigation |
|--------|-----------|-----------------|------------|
| Soul System | Design | "Hissi" vermeyebilir — 5 state arasi gecis oyuncuya anlamli gelmeyebilir | Hafta 2 playtest. Gorsel feedback (aura + partikuller) erken ekle. |
| Hunger System | Design + Balance | Stack zamanlama ve Ruh carpani cok kolay veya cok zor olabilir | Hafta 2 playtest, tuning knob'lar genis aralikta (2–4 sn, max 3–5) |
| Item System | Scope | 40–50 item sinerji matrisi karmasik, scope kaymasi riski | MVP limiti asma. Sinerji derinligi > item cesitliligi. |
| VFX/Aura System | Technical | 5 farkli aura state + Hunger partikulleri performans butcesini zorlayabilir | Particle pooling, basit shader, Hafta 2'de temel versiyon |

---

## Progress Tracker

| Metric | Count |
|--------|-------|
| Total systems identified | 24 |
| Design docs started | 7 |
| Design docs reviewed | 0 |
| Design docs approved | 0 |
| MVP systems designed | 7/18 |
| Vertical Slice systems designed | 0/4 |

---

## Next Steps

- [ ] Design MVP-tier systems first (use `/design-system [system-name]`)
- [ ] Ilk sistem: Soul System — her seyin temeli
- [ ] Run `/design-review` on each completed GDD
- [ ] Hafta 2 playtest icin minimum: Soul + Hunger + Surge Warning + VFX (temel)
- [ ] Run `/gate-check pre-production` when MVP systems are designed
- [ ] Prototype the highest-risk system early (`/prototype soul-system`)

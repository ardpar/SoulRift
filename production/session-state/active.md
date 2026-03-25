# Session State — SOULRIFT

<!-- STATUS -->
Epic: Pre-Production Setup
Feature: System GDD Authoring
Task: 7/18 MVP systems designed, next: Item System
<!-- /STATUS -->

## Current Task
7 system GDDs complete. Next in design order: Item System.

## Progress
- [x] Game concept converted to Markdown (`design/gdd/game-concept.md`)
- [x] Engine configured as Unity/C# (`.claude/docs/technical-preferences.md`)
- [x] Systems index created — 24 systems (`design/gdd/systems-index.md`)
- [x] Soul System GDD (`design/gdd/soul-system.md`)
- [x] Hunger System GDD (`design/gdd/hunger-system.md`)
- [x] Surge Warning GDD (`design/gdd/surge-warning.md`)
- [x] Combat System GDD (`design/gdd/combat-system.md`)
- [x] Wave System GDD (`design/gdd/wave-system.md`)
- [x] Health/Damage System GDD (`design/gdd/health-damage.md`)
- [x] Shop System GDD (`design/gdd/shop-system.md`)
- [ ] Item System GDD
- [ ] Cursed Item System GDD
- [ ] Character System GDD
- [ ] Enemy AI GDD

## Key Decisions
- Soul: %50 baslangic, manuel orb, hibrit kayip, ISoulProvider arayuzu
- Hunger: stack x bonus her oldurmede, direkt AddSoul, shop'ta timer devam
- Warning: timer sadece gorsel urgency, loop, cift kaynak intensity
- Combat: auto-fire + manual aim, 4 silah (Straight/Spread/Orbital/Beam)
- Wave: 18 wave, sub-wave (%50 kill threshold), her 5 wave boss
- Health: contact damage, iframes 0.5sn, knockback 1.5 birim, 8 dusman tipi
- Shop: 3 item + reroll, skip +5 Ruh, Hollow %30 indirim, Cursed slot Surging+

## Next Step
Design Item System GDD via `/design-system item-system`

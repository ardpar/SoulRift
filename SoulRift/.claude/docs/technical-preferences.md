# Technical Preferences

<!-- Populated by /setup-engine. Updated as the user makes decisions throughout development. -->
<!-- All agents reference this file for project-specific standards and conventions. -->

## Engine & Language

- **Engine**: Unity 6.3 LTS (6000.3.x)
- **Language**: C#
- **Rendering**: URP (Universal Render Pipeline) — 2D pixel art icin ideal
- **Physics**: Unity 2D Physics (Box2D)

## Naming Conventions

- **Classes**: PascalCase (e.g., `PlayerController`, `SoulSystem`)
- **Public Fields/Properties**: PascalCase (e.g., `MoveSpeed`, `SoulLevel`)
- **Private Fields**: _camelCase (e.g., `_moveSpeed`, `_currentSoulState`)
- **Methods**: PascalCase (e.g., `TakeDamage()`, `CollectSoul()`)
- **Events/Delegates**: PascalCase, On prefix (e.g., `OnSoulStateChanged`)
- **Files**: PascalCase matching class (e.g., `PlayerController.cs`)
- **Prefabs**: PascalCase (e.g., `EnemySkeleton`, `SoulOrb`)
- **Constants**: UPPER_SNAKE_CASE (e.g., `MAX_SOUL_LEVEL`, `HOLLOW_THRESHOLD`)

## Performance Budgets

- **Target Framerate**: 60 fps
- **Frame Budget**: 16.6 ms
- **Draw Calls**: [TO BE CONFIGURED — set after first profiling pass]
- **Memory Ceiling**: [TO BE CONFIGURED — set after first profiling pass]

## Testing

- **Framework**: NUnit (Unity Test Framework)
- **Minimum Coverage**: [TO BE CONFIGURED]
- **Required Tests**: Balance formulas, gameplay systems (Soul System, item sinerjileri)

## Forbidden Patterns

<!-- Add patterns that should never appear in this project's codebase -->
- [None configured yet — add as architectural decisions are made]

## Allowed Libraries / Addons

<!-- Add approved third-party dependencies here -->
- [None configured yet — add as dependencies are approved]

## Architecture Decisions Log

<!-- Quick reference linking to full ADRs in docs/architecture/ -->
- [No ADRs yet — use /architecture-decision to create one]

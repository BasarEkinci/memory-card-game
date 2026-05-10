# Card Match

A classic memory/concentration card matching game built with Unity 6. Features a strike bonus system for consecutive matches and penalties for repeated failures, creating a satisfying risk/reward dynamic.

**8 phases, 63 tasks, 72 passing tests (100% code pipeline)**

You can play here: https://basarekinci.itch.io/memory-card-game

---

## About

Card Match is a polished memory game where players flip cards to find matching pairs. The game emphasizes instant play with no menus, no ads, and no distractions.

- **Strike System** — Consecutive matches multiply score (up to 43 points for a perfect game)
- **Progressive Penalties** — 4th/6th/8th consecutive failures incur escalating point deductions
- **Save & Resume** — Leave and continue exactly where you left off
- **Instant Play** — No loading screens, game starts immediately

---

## Performance

| Metric | Target | Notes |
|--------|--------|-------|
| Frame Rate | 60 FPS | Stable on WebGL |
| Draw Calls | < 15 | Sprite atlas batching |
| Initial Load | < 3s | Optimized for web |
| Memory | < 50 MB | Mobile browser friendly |

### Rendering Optimizations

- Single sprite atlas for all 16 cards (1 draw call)
- UI Canvas split by update frequency
- Zero per-frame allocations in gameplay code
- Physics2D raycast only on click (not per-frame)

---

## Architecture

```
Assets/Scripts/
├── Logic/                    # Pure C# (no Unity dependencies)
│   ├── Models/               # CardModel, GameStateModel, GridModel
│   ├── Systems/              # CardSystem, MatchSystem, GridSystem, GameFlowSystem
│   └── Messages/             # MessagePipe event definitions
│
└── Runtime/                  # Unity-dependent code
    ├── Views/                # CardView, GridView, HUDView, InputView, etc.
    ├── Services/             # AudioSystem, SaveSystem
    ├── ScriptableObjects/    # GameConfig, AudioConfig, CardDefinitions
    ├── LifetimeScopes/       # VContainer DI configuration
    └── EntryPoints/          # BootstrapEntryPoint, GameEntryPoint
```

- **Model-View-System (MVS)** — Pure C# logic fully unit-testable without Unity
- **VContainer** — Dependency injection with no service locators or singletons
- **MessagePipe** — Decoupled event-driven communication between systems
- **Assembly Definitions** — Enforced dependency direction at compile time

---

## Tech Stack

**Unity 6** | **C# 9** | **VContainer** | **MessagePipe** | **UniTask** | **LitMotion** | **TextMeshPro** | **New Input System**

---

## Testing

**72 unit tests** across 4 test files (EditMode):

| Category | Tests | Coverage |
|----------|-------|----------|
| MatchSystem | 40 | Scoring, strikes, penalties, edge cases |
| CardSystem | 11 | State transitions, flip logic |
| GameFlowSystem | 14 | Phase management, win conditions |
| GridSystem | 7 | Shuffle algorithm, pair distribution |

All tests run without Unity dependencies — pure C# NUnit tests.

---

## How to Play

1. **Tap a card** to flip it face-up
2. **Tap a second card** to check for a match
3. **Match pairs** to earn points (consecutive matches add bonus points)
4. **Avoid failures** — 4+ consecutive misses incur penalties
5. **Match all 8 pairs** to win

**Perfect game score:** 43 points (all 8 matches consecutive)

---

## Documentation

| Document | Description |
|----------|-------------|
| [Game Design Document](docs/GDD.md) | Complete game mechanics, UI flow, scoring rules |
| [Technical Design Document](docs/TDD.md) | Architecture, systems, data flow, class index |
| [Workflow](docs/WORKFLOW.md) | Phased execution plan with task breakdown |
| [Progress](docs/PROGRESS.md) | Orchestration status and agent activity log |

---

## About the Pipeline

This project was built using an AI-powered game development pipeline. Multiple coordinated Claude Code agents worked through 8 phases:

1. **Foundation** — Assembly definitions, data models, message types
2. **Core Game Logic** — Pure C# systems (Card, Match, Grid, GameFlow)
3. **Unit Tests** — 72 tests with full coverage of game logic
4. **Unity Integration** — ScriptableObjects, Save/Audio systems
5. **Views** — All MonoBehaviour views with LitMotion animations
6. **Wiring** — VContainer LifetimeScopes and dependency injection
7. **Unity Setup** — Scene creation, prefabs, integration testing
8. **2D View Refactor** — SpriteRenderer-based cards with Physics2D input

The pipeline enforces strict rules: pure C# separation, zero allocations on hot paths, no runtime instantiation, mandatory tests, and [FormerlySerializedAs] on all renamed serialized fields.

---

## License

This project is a portfolio piece. Feel free to use it as a reference for Unity architecture patterns.

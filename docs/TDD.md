# Card Match — Technical Design Document

**Version:** 1.1  
**Date:** 2026-05-08  
**Based on:** GDD v1.0  
**Status:** Updated — Requires View Layer Refactor

---

## Changelog

### v1.1 (2026-05-08)
- **BREAKING**: Cards changed from UI Canvas to 2D SpriteRenderer objects
- **BREAKING**: Input changed from IPointerClickHandler to InputView with Physics2D.Raycast
- Added InputView for centralized 2D input handling
- CardView, GridView, DeckView now use Transform + SpriteRenderer instead of RectTransform + Image
- HUD (score, settings, panels) remains in UI Canvas
- Updated Scene Architecture for hybrid 2D + UI setup

---

## 1. Architecture Overview

Card Match follows the **Model-View-System (MVS)** pattern with VContainer for DI, MessagePipe for decoupled communication, UniTask for async operations, and LitMotion for animations.

```
┌─────────────────────────────────────────────────────────────────┐
│                         BOOT SCENE                               │
│  RootLifetimeScope                                               │
│  ├─ AudioSystem (singleton, persists)                           │
│  ├─ SaveSystem (singleton, persists)                            │
│  └─ AudioSettingsModel (singleton, persists)                    │
│                     │                                            │
│                     │ additive load                              │
│                     ▼                                            │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │                      GAME SCENE                              ││
│  │  GameLifetimeScope (child of Root)                          ││
│  │  ├─ Models: GameStateModel, GridModel, CardModel[]          ││
│  │  ├─ Systems: GameFlowSystem, MatchSystem, CardSystem        ││
│  │  ├─ Views: CardView[], GridView, DeckView, HUDView, etc.    ││
│  │  └─ MessagePipe brokers for all game events                 ││
│  └─────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────┘
```

**Key Decisions:**
- **Boot scene for persistence:** RootLifetimeScope holds services that must survive scene reloads (audio, save, settings). Boot scene loads GameScene additively in ~1 frame — invisible to player.
- **Single gameplay scene:** All gameplay happens in GameScene. Reset = clear state + re-deal, not scene reload.
- **No runtime GameObject creation:** All cards exist as pooled prefab instances, activated/deactivated as needed.
- **Pure C# logic:** All game logic in Systems. Views observe Models via ReactiveProperty and call Systems for actions.

---

## 2. Technical Constraints & Standards

Per project CLAUDE.md (non-negotiable):

| Constraint | Implementation |
|------------|----------------|
| Pure C# logic | All scoring, matching, state in Systems. No `using UnityEngine` in Logic assembly. |
| Zero allocation hot paths | Card selection uses cached delegates. No LINQ in gameplay code. |
| No runtime Instantiate | 16 CardViews pre-instantiated, pooled via SetActive. |
| VContainer DI | Constructor injection for Systems, `[Inject] Construct` for Views. No GameContext. |
| MessagePipe events | All cross-system communication via typed messages. |
| UniTask async | All delays via UniTask. No coroutines. |
| LitMotion tweens | All animations. Bound to CancellationToken for cleanup. |
| New Input System | If needed for future input complexity; current implementation uses UI pointer events. |
| ScriptableObjects | All tunable data (timings, thresholds, audio clips). |

---

## 3. System Inventory

| System | Layer | Purpose |
|--------|-------|---------|
| `GameFlowSystem` | Core | Game phase state machine (Loading→Dealing→Playing→Win) |
| `CardSystem` | Core | Individual card state management (FaceDown/FaceUp/Matched) |
| `MatchSystem` | Core | Match detection, scoring, strike/penalty logic |
| `GridSystem` | Core | Card shuffling, position assignment |
| `SaveSystem` | Infra | PlayerPrefs persistence (game state, best score, settings) |
| `AudioSystem` | Infra | Music/SFX playback with volume control |

---

## 4. Bootstrapping & Lifecycle

### 4.1 Initialization Sequence

```
Application Start
    │
    ▼
BootScene loads (set as Build Index 0)
    │
    ▼
RootLifetimeScope.Configure()
    ├─ Register AudioSystem, SaveSystem, AudioSettingsModel
    ├─ Register MessagePipe options
    └─ All singletons created
    │
    ▼
BootstrapEntryPoint.InitializeAsync() [IAsyncStartable]
    ├─ SaveSystem.LoadSettings() → AudioSettingsModel populated
    ├─ AudioSystem.Initialize()
    └─ SceneManager.LoadSceneAsync("GameScene", Additive)
    │
    ▼
GameScene loaded
    │
    ▼
GameLifetimeScope.Configure() [parent = RootLifetimeScope]
    ├─ Register all Models (GameStateModel, GridModel, CardModels)
    ├─ Register all Systems (GameFlowSystem, MatchSystem, CardSystem, GridSystem)
    ├─ Register MessagePipe brokers for game events
    └─ RegisterComponentInHierarchy for all Views
    │
    ▼
GameFlowSystem.StartAsync() [IAsyncStartable]
    ├─ SaveSystem.TryLoadGameState()
    ├─ If saved state exists: restore → enter Playing phase
    └─ If no saved state: GridSystem.Shuffle() → enter Dealing phase
    │
    ▼
Game running
```

### 4.2 Update Order

No manual Update ticks needed. All logic is event-driven:
- Card tap → CardView calls MatchSystem
- Match result → MessagePipe publishes → Views react
- Animations → LitMotion handles timing

### 4.3 Shutdown Sequence

```
OnApplicationPause / OnApplicationQuit
    │
    ▼
SaveSystem.SaveGameState() [if in Playing phase]
SaveSystem.SaveSettings()
    │
    ▼
GameLifetimeScope.Dispose()
    ├─ All Systems disposed (CancellationTokenSources cancelled)
    └─ MessagePipe subscriptions disposed
    │
    ▼
RootLifetimeScope.Dispose()
    └─ Audio stops
```

---

## 5. Assembly Definitions

```
Assemblies/
├─ CardMatch.Logic              ← Pure C# (Models, Systems, Messages)
│   └─ References: None (no Unity dependencies)
│
├─ CardMatch.Logic.Tests        ← NUnit tests for logic
│   └─ References: CardMatch.Logic
│
├─ CardMatch.Runtime            ← Unity code (Views, ScriptableObjects, LifetimeScopes)
│   └─ References: CardMatch.Logic, VContainer, MessagePipe, UniTask, LitMotion
│
├─ CardMatch.Runtime.Tests      ← Integration tests
│   └─ References: CardMatch.Runtime, CardMatch.Logic
│
└─ CardMatch.Editor             ← Editor tools (if needed)
    └─ References: CardMatch.Runtime, UnityEditor
```

**Dependency Rules:**
- Logic → nothing (pure C#)
- Runtime → Logic (one-way)
- Tests → both Logic and Runtime
- Editor → Runtime only

---

## 6. Folder Structure

```
Assets/
├─ Art/
│   ├─ Atlases/
│   │   └─ CardAtlas.spriteatlasv2
│   ├─ Cards/
│   │   ├─ card_back.png
│   │   └─ card_face_0..7.png
│   ├─ UI/
│   │   ├─ background.png
│   │   ├─ button_normal.png
│   │   ├─ panel_9slice.png
│   │   ├─ icon_settings.png
│   │   └─ icon_flame.png
│   └─ Deck/
│       └─ deck_stack.png
│
├─ Audio/
│   ├─ Music/
│   │   └─ bgm_main.mp3
│   └─ SFX/
│       ├─ sfx_flip.wav
│       ├─ sfx_match.wav
│       ├─ sfx_strike.wav
│       ├─ sfx_penalty.wav
│       └─ sfx_win.wav
│
├─ Prefabs/
│   ├─ Card.prefab
│   └─ UI/
│       ├─ SettingsPanel.prefab
│       ├─ WinPanel.prefab
│       └─ ResetConfirmPopup.prefab
│
├─ Scenes/
│   ├─ BootScene.unity
│   └─ GameScene.unity
│
├─ ScriptableObjects/
│   ├─ Config/
│   │   ├─ GameConfig.asset
│   │   └─ AudioConfig.asset
│   └─ Definitions/
│       └─ CardDefinitions.asset
│
├─ Scripts/
│   ├─ Logic/                           ← CardMatch.Logic.asmdef
│   │   ├─ Models/
│   │   │   ├─ CardModel.cs
│   │   │   ├─ GameStateModel.cs
│   │   │   └─ GridModel.cs
│   │   ├─ Systems/
│   │   │   ├─ CardSystem.cs
│   │   │   ├─ MatchSystem.cs
│   │   │   ├─ GridSystem.cs
│   │   │   └─ GameFlowSystem.cs
│   │   └─ Messages/
│   │       └─ GameMessages.cs
│   │
│   ├─ Runtime/                         ← CardMatch.Runtime.asmdef
│   │   ├─ Views/
│   │   │   ├─ CardView.cs              ← 2D SpriteRenderer + BoxCollider2D
│   │   │   ├─ GridView.cs              ← 2D Transform positioning
│   │   │   ├─ DeckView.cs              ← 2D SpriteRenderer
│   │   │   ├─ InputView.cs             ← 2D Raycast input handler (NEW)
│   │   │   ├─ HUDView.cs               ← UI Canvas
│   │   │   ├─ SettingsPanelView.cs     ← UI Canvas
│   │   │   ├─ WinPanelView.cs          ← UI Canvas
│   │   │   └─ ResetConfirmPopupView.cs ← UI Canvas
│   │   ├─ ScriptableObjects/
│   │   │   ├─ GameConfig.cs
│   │   │   ├─ AudioConfig.cs
│   │   │   └─ CardDefinitions.cs
│   │   ├─ Services/
│   │   │   ├─ AudioSystem.cs
│   │   │   └─ SaveSystem.cs
│   │   ├─ LifetimeScopes/
│   │   │   ├─ RootLifetimeScope.cs
│   │   │   └─ GameLifetimeScope.cs
│   │   └─ EntryPoints/
│   │       └─ BootstrapEntryPoint.cs
│   │
│   └─ Editor/                          ← CardMatch.Editor.asmdef (if needed)
│
└─ Tests/
    ├─ Logic/                           ← CardMatch.Logic.Tests.asmdef
    │   ├─ MatchSystemTests.cs
    │   ├─ CardSystemTests.cs
    │   └─ GridSystemTests.cs
    └─ Runtime/                         ← CardMatch.Runtime.Tests.asmdef
        └─ IntegrationTests.cs
```

---

## 7. Core Infrastructure Systems

### 7.1 MessagePipe Event System

**Purpose:** Decoupled communication between Systems and Views.

**Message Definitions** (all `readonly struct`):

| Message | Fields | Publisher | Subscribers |
|---------|--------|-----------|-------------|
| `CardFlippedMessage` | cardIndex, newState | CardSystem | CardView, AudioSystem |
| `MatchResultMessage` | isMatch, card1, card2, scoreDelta, newStrike | MatchSystem | CardView[], HUDView, AudioSystem, SaveSystem |
| `PenaltyAppliedMessage` | penaltyAmount, newScore | MatchSystem | HUDView, AudioSystem |
| `GamePhaseChangedMessage` | newPhase | GameFlowSystem | All Views |
| `GameWonMessage` | finalScore, bestScore, maxStrike | GameFlowSystem | WinPanelView, AudioSystem |
| `SettingsChangedMessage` | musicVolume, sfxVolume | SettingsPanelView | AudioSystem, SaveSystem |

**Registration in LifetimeScope:**
```
var options = builder.RegisterMessagePipe();
builder.RegisterMessageBroker<CardFlippedMessage>(options);
builder.RegisterMessageBroker<MatchResultMessage>(options);
// ... etc
```

### 7.2 Object Pool System

**Not needed as separate system.** With only 16 cards that never get destroyed, pooling is achieved by:
- Pre-instantiating 16 CardView GameObjects in the scene (or via prefab spawning at init)
- Using `SetActive(false)` when matched, `SetActive(true)` on reset
- Cards "returning to deck" = animate to deck position, then deactivate

### 7.3 Configuration System (ScriptableObjects)

**GameConfig.asset:**
- cardCount (16)
- gridColumns (4)
- dealDuration (3.0f)
- dealCardDelay (calculated: dealDuration / cardCount)
- flipDuration (0.3f)
- noMatchRevealTime (2.0f)
- autoCloseTime (10.0f)
- penaltyThresholds (int[]: 4, 6, 8)
- penaltyAmounts (int[]: 1, 2, 3)

**AudioConfig.asset:**
- bgmClip
- flipClip, matchClip, strikeClip, penaltyClip, winClip

**CardDefinitions.asset:**
- cardSprites (Sprite[8] for faces)
- backSprite

### 7.4 Dependency Injection (VContainer)

See Section 4.1 for registration. Key points:
- **No GameContext.** Each class injects only what it needs.
- Models as `Lifetime.Singleton` within their scope
- Systems as `Lifetime.Singleton` with `.AsImplementedInterfaces().AsSelf()`
- Views via `RegisterComponentInHierarchy<T>()`

### 7.5 State Machine Framework

**Not a separate framework.** GameFlowSystem uses a simple enum-based state:

```
enum GamePhase { Loading, Dealing, Playing, Paused, Win }
```

Phase transitions via method calls, published through MessagePipe. No generic state machine abstraction needed for this scope.

### 7.6 Input System (2D Raycast)

**Purpose:** Centralized input handling for 2D card interactions.

**InputView (MonoBehaviour):**
```csharp
public sealed class InputView : MonoBehaviour
{
    private Camera _mainCamera;
    private MatchSystem _matchSystem;
    private GameFlowSystem _gameFlowSystem;
    private GridView _gridView;

    [Inject]
    public void Construct(MatchSystem matchSystem, GameFlowSystem gameFlowSystem, GridView gridView)
    {
        _matchSystem = matchSystem;
        _gameFlowSystem = gameFlowSystem;
        _gridView = gridView;
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!_gameFlowSystem.IsInputAllowed()) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick(Input.mousePosition);
        }
        
        // Touch support for mobile/WebGL
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleClick(Input.GetTouch(0).position);
        }
    }

    private void HandleClick(Vector3 screenPosition)
    {
        Vector2 worldPoint = _mainCamera.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);
        
        if (hit.collider != null && hit.collider.TryGetComponent<CardView>(out var cardView))
        {
            int gridIndex = cardView.GridIndex;
            _matchSystem.SelectCard(gridIndex);
        }
    }
}
```

**Key Points:**
- InputView owns ALL card input detection
- CardView no longer implements IPointerClickHandler
- Uses Physics2D.Raycast with BoxCollider2D on cards
- Checks `GameFlowSystem.IsInputAllowed()` before processing
- Supports both mouse (WebGL desktop) and touch (WebGL mobile)

**Registration:**
```csharp
builder.RegisterComponentInHierarchy<InputView>();
```

---

## 8. Gameplay Systems

### 8.1 GameFlowSystem

**Purpose:** Master controller for game phase transitions.

**MVS Breakdown:**
- **Model:** `GameStateModel.Phase` (ReactiveProperty<GamePhase>)
- **System:** `GameFlowSystem` — owns phase transitions
- **Views:** All views subscribe to `GamePhaseChangedMessage` to enable/disable interaction

**Phase Transitions:**
```
Loading ──[init complete]──► Dealing ──[deal complete]──► Playing
                                                             │
                                    ┌──────────────┬─────────┴────────┐
                                    │              │                  │
                              [settings open]  [all matched]    [reset confirm]
                                    │              │                  │
                                    ▼              ▼                  ▼
                                 Paused          Win              Dealing
                                    │              │              (new game)
                              [settings close]  [new game]
                                    │              │
                                    ▼              │
                                 Playing ◄─────────┘
```

**Key Logic:**
- On `StartAsync`: check SaveSystem for existing state, branch to Dealing or Playing
- On phase change: publish `GamePhaseChangedMessage`
- Paused phase: set flag that MatchSystem checks before processing input

**Events:**
- Publishes: `GamePhaseChangedMessage`, `GameWonMessage`
- Subscribes: `MatchResultMessage` (to check win condition)

### 8.2 CardSystem

**Purpose:** Manage individual card states.

**MVS Breakdown:**
- **Model:** `CardModel[]` (16 instances) — each has `State` (ReactiveProperty<CardState>), `TypeId`, `GridIndex`
- **System:** `CardSystem` — state transitions for cards
- **View:** `CardView` — subscribes to its CardModel.State, plays flip animation

**Card States:**
```
enum CardState { InDeck, FaceDown, FaceUp, Matched }
```

**Key Logic:**
```
FlipCard(cardIndex):
    if card.State != FaceDown: return
    if currentPhase != Playing: return
    card.State = FaceUp
    publish CardFlippedMessage

CloseCard(cardIndex):
    if card.State != FaceUp: return
    card.State = FaceDown
    publish CardFlippedMessage

MarkMatched(card1, card2):
    card1.State = Matched
    card2.State = Matched
    publish CardFlippedMessage for each
```

**Events:**
- Publishes: `CardFlippedMessage`
- Subscribes: none (called by MatchSystem)

### 8.3 MatchSystem

**Purpose:** Core gameplay logic — match detection, scoring, strike/penalty.

**MVS Breakdown:**
- **Model:** `GameStateModel` — score, strikeCount, failCount, maxStrike (all ReactiveProperty)
- **System:** `MatchSystem` — all scoring logic
- **View:** `HUDView` — subscribes to GameStateModel properties

**Key Logic (pseudo code):**
```
OnCardSelected(cardIndex):
    if gamePhase != Playing: return
    if card is already FaceUp or Matched: return
    
    CardSystem.FlipCard(cardIndex)
    
    if firstSelectedCard is null:
        firstSelectedCard = cardIndex
        start autoCloseTimer (10s)
        return
    
    secondSelectedCard = cardIndex
    cancel autoCloseTimer
    
    await EvaluateMatch()

EvaluateMatch():
    isMatch = cards[first].TypeId == cards[second].TypeId
    
    if isMatch:
        scoreDelta = 1 + strikeCount + 1   // base + new strike value
        strikeCount++
        maxStrike = max(maxStrike, strikeCount)
        failCount = 0
        score += scoreDelta
        CardSystem.MarkMatched(first, second)
        publish MatchResultMessage(isMatch=true, scoreDelta, strikeCount)
        
        if all cards matched:
            GameFlowSystem.TriggerWin()
    else:
        strikeCount = 0
        failCount++
        
        penaltyAmount = 0
        if failCount in penaltyThresholds:
            penaltyIndex = indexOf(failCount)
            penaltyAmount = penaltyAmounts[penaltyIndex]
            score = max(0, score - penaltyAmount)
            publish PenaltyAppliedMessage
        
        publish MatchResultMessage(isMatch=false, ...)
        await UniTask.Delay(noMatchRevealTime)
        CardSystem.CloseCard(first)
        CardSystem.CloseCard(second)
    
    firstSelectedCard = null
    secondSelectedCard = null
```

**Events:**
- Publishes: `MatchResultMessage`, `PenaltyAppliedMessage`
- Subscribes: none (called by CardView on tap)

### 8.4 GridSystem

**Purpose:** Card shuffling and position assignment.

**MVS Breakdown:**
- **Model:** `GridModel` — cardIndices[16] mapping grid position to card type
- **System:** `GridSystem` — shuffle algorithm
- **View:** `GridView` — owns card layout positions

**Key Logic:**
```
Shuffle():
    create pairs: [0,0,1,1,2,2,3,3,4,4,5,5,6,6,7,7]
    Fisher-Yates shuffle in-place
    assign to GridModel.cardIndices
    
    for each CardModel:
        cardModel.TypeId = GridModel.cardIndices[cardModel.GridIndex]
        cardModel.State = InDeck
```

**Events:**
- Publishes: none
- Subscribes: none (called by GameFlowSystem)

### 8.5 SaveSystem

**Purpose:** Persist game state and settings to PlayerPrefs.

**Location:** Runtime assembly (needs Unity API for PlayerPrefs)

**Key Logic:**
```
SaveGameState():
    json = serialize GameStateModel + all CardModel states
    PlayerPrefs.SetString("GameState", json)
    
TryLoadGameState() -> bool:
    json = PlayerPrefs.GetString("GameState")
    if empty: return false
    deserialize into models
    return true
    
SaveBestScore(score):
    current = PlayerPrefs.GetInt("BestScore", 0)
    if score > current:
        PlayerPrefs.SetInt("BestScore", score)
        
LoadBestScore() -> int:
    return PlayerPrefs.GetInt("BestScore", 0)
    
SaveSettings(music, sfx):
    PlayerPrefs.SetFloat("MusicVolume", music)
    PlayerPrefs.SetFloat("SFXVolume", sfx)
    
LoadSettings() -> (music, sfx):
    return (PlayerPrefs.GetFloat("MusicVolume", 1f),
            PlayerPrefs.GetFloat("SFXVolume", 1f))

ClearGameState():
    PlayerPrefs.DeleteKey("GameState")
```

**Events:**
- Publishes: none
- Subscribes: `MatchResultMessage` (auto-save), `SettingsChangedMessage` (save settings), `GameWonMessage` (save best score, clear game state)

### 8.6 AudioSystem

**Purpose:** Music and SFX playback.

**Location:** Runtime assembly (needs AudioSource)

**Components:**
- Two AudioSources: one for BGM (loop), one for SFX (one-shot)
- Injected: `AudioConfig`, `AudioSettingsModel`

**Key Logic:**
```
Initialize():
    bgmSource.clip = config.bgmClip
    bgmSource.loop = true
    bgmSource.volume = settingsModel.MusicVolume
    bgmSource.Play()

PlaySFX(clipType):
    clip = config.GetClip(clipType)
    sfxSource.PlayOneShot(clip, settingsModel.SFXVolume)

SetMusicVolume(volume):
    settingsModel.MusicVolume = volume
    bgmSource.volume = volume

SetSFXVolume(volume):
    settingsModel.SFXVolume = volume
```

**Events:**
- Publishes: none
- Subscribes: `CardFlippedMessage` (play flip), `MatchResultMessage` (play match or no sound), `PenaltyAppliedMessage` (play penalty), `GameWonMessage` (play win), `SettingsChangedMessage` (update volumes)

---

## 9. Rendering Architecture

### 9.1 Hybrid Approach: 2D World + UI Canvas

**Cards and Deck:** 2D SpriteRenderer objects in world space  
**HUD and Panels:** uGUI Canvas overlay

**Rationale:**
- Cards benefit from proper 2D rendering (SpriteRenderer batching, sorting layers)
- Pixel-perfect card sprites without UI scaling artifacts
- Easier click detection with BoxCollider2D + Physics2D.Raycast
- HUD stays in UI Canvas for responsive anchoring and safe area handling
- Clear separation: gameplay elements (2D) vs interface elements (UI)

### 9.2 Scene Structure (Hybrid)

```
GameScene
├─ MainCamera (Orthographic, size: 10, for 2D world)
│
├─ World2D (Transform container)
│   ├─ Background (SpriteRenderer, sorting layer: Background)
│   ├─ DeckView (SpriteRenderer + DeckView.cs)
│   │   └─ Sorting Layer: Cards, Order: 0
│   └─ GridContainer (empty transform, anchor for grid)
│       └─ CardViews[16] (SpriteRenderer + BoxCollider2D + CardView.cs)
│           └─ Sorting Layer: Cards, Order: 1-16 (for overlap during animation)
│
├─ InputView (empty GameObject with InputView.cs)
│
├─ Canvas_HUD (Screen Space - Overlay, CanvasScaler: 1080x1920)
│   └─ SafeAreaContainer
│       ├─ SettingsButton (top-right)
│       ├─ ScoreText (top-center)
│       └─ StrikeContainer (below score)
│           ├─ FlameIcon
│           └─ StrikeText
│
├─ Canvas_Overlays (Screen Space - Overlay, disabled by default)
│   ├─ SettingsPanel
│   ├─ WinPanel
│   └─ ResetConfirmPopup
│
├─ AudioSources
│   ├─ BGMSource
│   └─ SFXSource
│
└─ EventSystem (for UI panel interactions)

### 9.3 Sorting Layers

| Layer | Order | Contents |
|-------|-------|----------|
| Background | 0 | Background sprite |
| Cards | 1 | Deck, all cards |
| UI | (Canvas) | HUD, overlays |

**Card Sorting During Animation:**
- Dealing: card being dealt gets Order +16 (on top)
- Flip: no change needed
- Match to deck: card gets Order -1 (behind other cards)

### 9.4 Camera Setup

```
MainCamera:
  - Projection: Orthographic
  - Size: 10 (covers ~20 world units vertically)
  - Position: (0, 0, -10)
  - Clear Flags: Solid Color
  - Background: Game background color
  - Culling Mask: Everything except UI
```

**Grid Positioning (World Space):**
- Grid center: (1.5, 0) — offset right to make room for deck
- Card size: ~2 world units
- Card spacing: 0.2 world units
- Total grid: ~9.2 x 9.2 world units

**Deck Position:** (-3.5, 0) — left side

### 9.5 Screen Management

No complex screen manager needed. Overlays are activated/deactivated:

```
OpenSettings():
    settingsPanel.gameObject.SetActive(true)
    GameFlowSystem.SetPhase(Paused)

CloseSettings():
    settingsPanel.gameObject.SetActive(false)
    GameFlowSystem.SetPhase(Playing)

ShowWinPanel():
    winPanel.gameObject.SetActive(true)
    // No phase change — already in Win phase
```

### 9.6 Data Binding

Views subscribe to Model ReactiveProperties:

```csharp
// In HUDView.Start()
_gameState.Score
    .Subscribe(score => _scoreText.text = score.ToString())
    .AddTo(_disposables);

_gameState.StrikeCount
    .Subscribe(strike => {
        _strikeText.text = $"x{strike}";
        _strikeContainer.SetActive(strike > 0);
    })
    .AddTo(_disposables);
```

### 9.7 Animation Integration

**Card animations (2D world space)** via LitMotion:

```csharp
// Card flip animation (scale X approach in world space)
LMotion.Create(1f, 0f, 0.15f)
    .WithEase(Ease.InQuad)
    .BindToLocalScaleX(_transform)
    .ToUniTask(token);

// Sprite swap at midpoint

LMotion.Create(0f, 1f, 0.15f)
    .WithEase(Ease.OutQuad)
    .BindToLocalScaleX(_transform)
    .ToUniTask(token);

// Card move to deck (world position)
LMotion.Create(_transform.position, deckPosition, 0.4f)
    .WithEase(Ease.InBack)
    .BindToPosition(_transform)
    .ToUniTask(token);
```

**UI animations (HUD)** same as before but using RectTransform.

---

## 10. Data Architecture

### 10.1 Save Data Schema

**PlayerPrefs Keys:**

| Key | Type | Description |
|-----|------|-------------|
| `BestScore` | int | Highest score achieved |
| `MusicVolume` | float | 0.0-1.0 |
| `SFXVolume` | float | 0.0-1.0 |
| `GameState` | string | JSON blob of current game (nullable) |

**GameState JSON:**
```json
{
    "score": 12,
    "strikeCount": 2,
    "failCount": 1,
    "maxStrike": 3,
    "cards": [
        { "gridIndex": 0, "typeId": 3, "state": "FaceDown" },
        { "gridIndex": 1, "typeId": 7, "state": "Matched" },
        ...
    ]
}
```

### 10.2 Runtime Data Flow

```
User Tap/Click
    │
    ▼
InputView.Update() — Physics2D.Raycast
    │
    ▼
InputView.HandleClick() — finds CardView via collider
    │
    ▼
MatchSystem.SelectCard(index)
    │
    ├─► CardSystem.FlipCard() ─► CardModel.State changed
    │                                    │
    │                                    ▼
    │                           CardFlippedMessage published
    │                                    │
    │                           ┌────────┴────────┐
    │                           ▼                 ▼
    │                    CardView.PlayFlip()  AudioSystem.PlayFlip()
    │
    └─► (if second card) EvaluateMatch()
            │
            ├─► GameStateModel.Score/Strike/Fail changed
            │         │
            │         ▼
            │   MatchResultMessage published
            │         │
            │   ┌─────┴─────┬──────────┬──────────┐
            │   ▼           ▼          ▼          ▼
            │ HUDView   CardView[]  AudioSystem SaveSystem
            │ updates   animate     play SFX    auto-save
            │
            └─► (if all matched) GameFlowSystem.TriggerWin()
                      │
                      ▼
                GameWonMessage published
                      │
                ┌─────┴─────┐
                ▼           ▼
           WinPanelView  SaveSystem
           show panel    save best, clear state
```

### 10.3 Serialization Strategy

- **Models:** Plain C# classes with public properties. Serialized via System.Text.Json or Unity's JsonUtility.
- **Enums:** Serialized as strings for readability.
- **No complex migration needed** — if schema changes, old saves are discarded (MVP scope).

---

## 11. Scene Architecture

### 11.1 Scene Hierarchy

**BootScene (Build Index 0):**
```
BootScene
├─ RootLifetimeScope (GameObject with DontDestroyOnLoad)
│   └─ RootLifetimeScope.cs
└─ (empty — scene exists only for initialization)
```

**GameScene (Build Index 1, loaded additively):**
```
GameScene
├─ GameLifetimeScope
│   └─ GameLifetimeScope.cs
│
├─ MainCamera
│   ├─ Orthographic, Size: 10
│   ├─ Position: (0, 0, -10)
│   └─ Background: Game background color
│
├─ World2D (empty container)
│   ├─ Background (SpriteRenderer, Sorting Layer: Background)
│   ├─ DeckView (SpriteRenderer + DeckView.cs)
│   └─ GridContainer (empty transform)
│       └─ CardViews[16] (each: SpriteRenderer + BoxCollider2D + CardView.cs)
│
├─ InputView (empty GameObject with InputView.cs)
│
├─ Canvas_HUD (Screen Space - Overlay)
│   └─ SafeAreaContainer
│       ├─ SettingsButton
│       ├─ ScoreText
│       └─ StrikeContainer
│
├─ Canvas_Overlays (Screen Space - Overlay, disabled by default)
│   ├─ SettingsPanel
│   ├─ WinPanel
│   └─ ResetConfirmPopup
│
├─ AudioSources
│   ├─ BGMSource
│   └─ SFXSource
│
└─ EventSystem (for UI panel interactions)
```

### 11.2 Prefab Inventory

| Prefab | Purpose | Components | Pooled? |
|--------|---------|------------|---------|
| `Card.prefab` | Single card (2D) | SpriteRenderer, BoxCollider2D, CardView | Yes (16 in scene) |
| `SettingsPanel.prefab` | Settings overlay | UI RectTransform, SettingsPanelView | No (1 instance) |
| `WinPanel.prefab` | Win overlay | UI RectTransform, WinPanelView | No (1 instance) |
| `ResetConfirmPopup.prefab` | Confirmation dialog | UI RectTransform, ResetConfirmPopupView | No (1 instance) |

### 11.3 Scene Setup Checklist

**BootScene:**
- [ ] RootLifetimeScope GameObject with script attached
- [ ] Marked as DontDestroyOnLoad in script
- [ ] Build Settings: Index 0

**GameScene — 2D World:**
- [ ] MainCamera: Orthographic, Size 10, Position (0, 0, -10)
- [ ] Sorting Layers created: Background, Cards
- [ ] Background sprite at z=0, Sorting Layer: Background
- [ ] DeckView at position (-3.5, 0), Sorting Layer: Cards
- [ ] GridContainer at position (1.5, 0)
- [ ] 16 CardView children in GridContainer, each with:
  - [ ] SpriteRenderer (Sorting Layer: Cards)
  - [ ] BoxCollider2D (Size: matches card sprite)
  - [ ] CardView.cs attached
- [ ] InputView GameObject with InputView.cs attached

**GameScene — UI:**
- [ ] Canvas_HUD with CanvasScaler (1080x1920, Scale With Screen Size)
- [ ] SafeArea component on container
- [ ] Canvas_Overlays with panels (disabled by default)
- [ ] EventSystem for UI interaction

**GameScene — Other:**
- [ ] GameLifetimeScope with parent reference to RootLifetimeScope
- [ ] AudioSources for BGM and SFX
- [ ] Build Settings: Index 1

---

## 12. Performance Budget

### 12.1 Targets

| Metric | Target | Rationale |
|--------|--------|-----------|
| Frame Rate | 60 FPS stable | WebGL standard |
| Initial Load | < 3 seconds | Fast initial load critical for web |
| Memory | < 50 MB | Mobile browser friendly |
| Draw Calls | < 20 | See Section 13 |

### 12.2 Hot Paths

| Hot Path | Optimization |
|----------|--------------|
| Card tap detection | Physics2D.Raycast on click only (not per-frame) |
| Animation updates | LitMotion handles internally, batched |
| Message publishing | MessagePipe is allocation-free for value types |
| 2D rendering | SpriteRenderer batching via shared atlas + material |

### 12.3 Profiling Checkpoints

- After Dealing animation: verify no GC spikes
- During rapid card flipping: verify frame stability
- Win panel appearance: verify no hitch

---

## 13. Rendering & GPU Strategy

### 13.1 Draw Call Plan

**Target: < 15 draw calls during gameplay.**

| Element | Draw Calls | Strategy |
|---------|------------|----------|
| Background | 1 | Single sprite |
| All Cards (16) | 1 | Single sprite atlas, same material |
| Deck | 1 | Same atlas as cards |
| UI (HUD) | 2-3 | Separate UI atlas, minimal overdraw |
| UI Panels (when open) | 2-3 | Same UI atlas |

**Total: ~8-10 draw calls typical, ~12-15 with panel open.**

### 13.2 Sprite Atlas Plan

**CardAtlas (2048x2048):**
- card_back.png
- card_face_0.png through card_face_7.png
- deck_stack.png

**UIAtlas (1024x1024):**
- background.png (if small enough, else separate)
- button_normal.png, button_pressed.png
- panel_9slice.png
- icon_settings.png
- icon_flame.png

### 13.3 Material Sharing

- All cards use a single shared material referencing CardAtlas
- All UI elements use Unity's default UI material (auto-batched by Canvas)
- **Never access `renderer.material`** — use `sharedMaterial` for read

### 13.4 Batching Approach

- **SRP Batcher:** Enabled (URP default)
- **Sprite Batching:** Automatic via shared atlas + material
- **UI Batching:** Automatic via Canvas batching (same atlas, no material changes)
- **Static Batching:** Not needed (no static 3D meshes)

### 13.5 UI Canvas Split

| Canvas | Contents | Update Frequency |
|--------|----------|------------------|
| Canvas_Main | HUD (score, strike, settings button), Grid, Deck | Score/strike update on match only |
| Canvas_Overlays | Settings, Win, Confirm popups | Rarely (only when opened) |

**Note:** For this simple game, a single Canvas is acceptable. Split only if profiler shows Canvas rebuild issues.

### 13.6 Overdraw Risks

| Risk | Mitigation |
|------|------------|
| Card overlap during deal animation | Cards deal one at a time, minimal overlap |
| Panel over gameplay | Panels are opaque, no transparency stacking |
| Strike flame icon | Small sprite, negligible |

### 13.7 Shader Strategy

- **Sprites:** Default Sprite shader (URP 2D Renderer)
- **UI:** Default UI shader
- **No custom shaders needed** for MVP

### 13.8 Developer Setup Steps (GPU Optimization)

**Step 1: Create Sprite Atlases (BLOCKING — do before implementation)**

1. In Unity: Right-click Project window → Create → 2D → Sprite Atlas
2. Create `Assets/Art/Atlases/CardAtlas.spriteatlasv2`
   - Objects for Packing: Add `Assets/Art/Cards/` folder and `Assets/Art/Deck/` folder
   - Max Texture Size: 2048
   - Enable "Tight Packing"
   - Enable "Allow Rotation": false (cards should stay upright)
3. Create `Assets/Art/Atlases/UIAtlas.spriteatlasv2`
   - Objects for Packing: Add `Assets/Art/UI/` folder
   - Max Texture Size: 1024
4. Click "Pack Preview" on each to verify all sprites fit

**Step 2: Texture Import Settings**

For all sprites in `Assets/Art/`:
- Texture Type: Sprite (2D and UI)
- Compression: ASTC 6x6 (or platform-appropriate)
- Generate Mip Maps: Off (2D game)
- Max Size: 512 for individual sprites (atlas handles final size)

**Step 3: Verify Batching**

After setup, run in Editor with Frame Debugger (Window → Analysis → Frame Debugger):
- Verify all cards render in 1 draw call
- Verify UI renders in 2-3 draw calls max

---

## 14. Testing Strategy

### 14.1 Unit Test Structure (CardMatch.Logic.Tests)

```
Tests/Logic/
├─ MatchSystemTests.cs
│   ├─ MatchDetection_SameTypeId_ReturnsMatch
│   ├─ MatchDetection_DifferentTypeId_ReturnsFail
│   ├─ Scoring_FirstMatch_AddsOnePoint
│   ├─ Scoring_ConsecutiveMatch_AddsStrikeBonus
│   ├─ Strike_ResetOnFail_BecomesZero
│   ├─ Penalty_FourthFail_SubtractsOnePoint
│   ├─ Penalty_ScoreCannotGoBelowZero
│   └─ ... (cover all edge cases from GDD Appendix B)
│
├─ CardSystemTests.cs
│   ├─ FlipCard_FaceDown_BecomesFaceUp
│   ├─ FlipCard_AlreadyFaceUp_NoChange
│   ├─ MarkMatched_SetsMatchedState
│   └─ ...
│
├─ GridSystemTests.cs
│   ├─ Shuffle_Creates8Pairs
│   ├─ Shuffle_AllCardsAssigned
│   ├─ Shuffle_Randomized (statistical test over many runs)
│   └─ ...
│
└─ GameFlowSystemTests.cs
    ├─ Start_NoSavedState_EntersDealing
    ├─ Start_WithSavedState_EntersPlaying
    ├─ AllCardsMatched_TriggersWin
    └─ ...
```

### 14.2 Test Conventions

- **Naming:** `MethodUnderTest_Scenario_ExpectedResult`
- **Structure:** Arrange-Act-Assert, clearly separated
- **One assertion per test** (or closely related assertions)
- **No Unity dependencies** in Logic tests — they must run in any NUnit runner

### 14.3 Integration Tests (CardMatch.Runtime.Tests)

```
Tests/Runtime/
├─ SaveSystemTests.cs
│   ├─ SaveAndLoad_GameState_RoundTrips
│   ├─ Load_NoSavedState_ReturnsFalse
│   └─ ...
│
└─ FullGameFlowTests.cs
    ├─ NewGame_DealsAllCards
    ├─ MatchAllPairs_ShowsWinPanel
    └─ ...
```

### 14.4 Mocking Strategy

- **Pure interfaces** for dependencies where needed
- **No mocking frameworks** — use hand-rolled test doubles
- Systems receive interfaces, tests provide stub implementations

Example:
```csharp
// Production
public interface ISaveService { void SaveGameState(GameStateModel state); }

// Test
public class FakeSaveService : ISaveService {
    public GameStateModel LastSaved;
    public void SaveGameState(GameStateModel state) => LastSaved = state;
}
```

---

## 15. Design Patterns Summary

| System | Patterns | Justification |
|--------|----------|---------------|
| GameFlowSystem | State Pattern (simple enum) | Clear phase transitions, easy to extend |
| MatchSystem | — | Pure logic, no pattern needed beyond MVS |
| CardSystem | — | Pure state management |
| GridSystem | — | Algorithm only |
| SaveSystem | Repository Pattern (light) | Abstracts PlayerPrefs access |
| AudioSystem | — | Service with injected config |
| All Views | Observer (via ReactiveProperty) | Decouple View updates from System logic |
| All Systems | Dependency Injection | Testability, explicit dependencies |
| Cross-System | Publish-Subscribe (MessagePipe) | Decoupled communication |

---

## 16. Class Index

| Class | Assembly | Purpose |
|-------|----------|---------|
| `CardModel` | Logic | Card state data (typeId, state, gridIndex) |
| `GameStateModel` | Logic | Score, strike, fail count, phase |
| `GridModel` | Logic | Card-to-position mapping |
| `CardSystem` | Logic | Card state transitions |
| `MatchSystem` | Logic | Match detection and scoring |
| `GridSystem` | Logic | Shuffle algorithm |
| `GameFlowSystem` | Logic | Phase management |
| `GameMessages` | Logic | All MessagePipe message structs |
| `CardView` | Runtime | 2D card rendering (SpriteRenderer + BoxCollider2D) |
| `GridView` | Runtime | 2D grid layout (Transform positioning) |
| `DeckView` | Runtime | 2D deck visuals (SpriteRenderer) |
| `InputView` | Runtime | 2D input handler (Physics2D.Raycast) — **NEW** |
| `HUDView` | Runtime | Score and strike display (UI Canvas) |
| `SettingsPanelView` | Runtime | Settings overlay (UI Canvas) |
| `WinPanelView` | Runtime | Win screen (UI Canvas) |
| `ResetConfirmPopupView` | Runtime | Confirmation dialog (UI Canvas) |
| `AudioSystem` | Runtime | Audio playback |
| `SaveSystem` | Runtime | PlayerPrefs persistence |
| `GameConfig` | Runtime | ScriptableObject for tunable values |
| `AudioConfig` | Runtime | ScriptableObject for audio clips |
| `CardDefinitions` | Runtime | ScriptableObject for card sprites |
| `RootLifetimeScope` | Runtime | VContainer root scope |
| `GameLifetimeScope` | Runtime | VContainer game scope |
| `BootstrapEntryPoint` | Runtime | Initialization and scene loading |

---

## 17. Open Questions / Risks

| Item | Risk Level | Mitigation |
|------|------------|------------|
| WebGL audio autoplay | Medium | BGM may require first user interaction. AudioSystem should handle this gracefully. |
| PlayerPrefs reliability on WebGL | Low | Standard approach, well-supported. Consider fallback message if storage unavailable. |
| 2D card flip animation | Low | Scale-X approach with sprite swap at midpoint. Proven technique. |
| LitMotion WebGL compatibility | Low | LitMotion is lightweight and WebGL-friendly. Verify in early prototype. |
| Physics2D raycast performance | Low | Only on click/touch, not per-frame. Negligible cost. |

---

## Summary

This architecture delivers a clean, testable, performant memory card game:

- **Hybrid 2D + UI architecture** — cards as 2D SpriteRenderer objects, HUD as UI Canvas overlay
- **Single gameplay scene** (GameScene) with minimal boot scene for service initialization
- **Pure C# logic** fully unit-testable without Unity
- **MVS pattern** throughout with ReactiveProperty binding
- **Centralized input via InputView** — Physics2D.Raycast on click, systems remain input-agnostic
- **MessagePipe** for decoupled event-driven communication
- **< 15 draw calls** via sprite atlasing and SpriteRenderer batching
- **Zero allocation hot paths** — all interactions are event-driven, no per-frame polling

The design is intentionally minimal — no over-engineering for a 16-card memory game. Every system has a clear, single responsibility.

---

**TDD v1.1 Complete.** Run `/plan-workflow` to regenerate the execution plan for View layer refactor.

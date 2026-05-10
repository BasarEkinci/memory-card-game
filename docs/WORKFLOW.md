# Card Match — Execution Workflow Plan

**Version:** 1.1  
**Date:** 2026-05-08  
**Based on:** GDD v1.0, TDD v1.1  
**Status:** Ready for Orchestration (Phase 8 Added)

---

## Changelog

### v1.1 (2026-05-08)
- Added Phase 8: 2D View Refactor (6 tasks)
- Cards changed from UI Canvas to 2D SpriteRenderer objects
- Added InputView for centralized 2D raycast input
- Total tasks: 28 → 34

---

## 1. Overview

| Metric | Value |
|--------|-------|
| Total Phases | 8 |
| Total Tasks | 34 |
| Parallel Efficiency | ~65% (22 of 34 tasks run in parallel groups) |
| Critical Path Length | 12 tasks |
| Recommended Team | 3 coders, 1 tester, 1 reviewer, 1 unity-setup |

**Critical Path:**
```
Asmdefs → Models → CardSystem → MatchSystem → Views → LifetimeScopes → Scene Setup → 2D Refactor → 2D Scene Setup
```

---

## 2. Dependency Graph

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           PHASE 1: FOUNDATION                                │
├─────────────────────────────────────────────────────────────────────────────┤
│  [P1.T1 Asmdefs] ──┬──► [P1.T2 Models] ──┬──► [P1.T3 Messages]              │
│                    │                      │                                  │
└────────────────────┼──────────────────────┼──────────────────────────────────┘
                     │                      │
┌────────────────────┼──────────────────────┼──────────────────────────────────┐
│                    │  PHASE 2: CORE SYSTEMS                                  │
├────────────────────┼──────────────────────┼──────────────────────────────────┤
│                    │                      │                                  │
│  ┌─────────────────▼───────┐  ┌──────────▼────────┐                         │
│  │ [P2.T1 CardSystem]      │  │ [P2.T2 GridSystem]│                         │
│  └─────────────┬───────────┘  └─────────┬─────────┘                         │
│                │                        │                                    │
│                ▼                        ▼                                    │
│  ┌─────────────────────────┐  ┌─────────────────────────┐                   │
│  │ [P2.T3 MatchSystem]     │  │ [P2.T4 GameFlowSystem]  │                   │
│  └─────────────────────────┘  └─────────────────────────┘                   │
│                                                                              │
└──────────────────────────────────────────────────────────────────────────────┘
                     │
┌────────────────────┼─────────────────────────────────────────────────────────┐
│                    │  PHASE 3: UNIT TESTS                                    │
├────────────────────┼─────────────────────────────────────────────────────────┤
│  [P3.T1 CardSystemTests] [P3.T2 GridSystemTests]                             │
│  [P3.T3 MatchSystemTests] [P3.T4 GameFlowSystemTests]                        │
└──────────────────────────────────────────────────────────────────────────────┘
                     │
┌────────────────────┼─────────────────────────────────────────────────────────┐
│                    │  PHASE 4: UNITY INTEGRATION                             │
├────────────────────┼─────────────────────────────────────────────────────────┤
│  [P4.T1 ScriptableObjects] [P4.T2 SaveSystem] [P4.T3 AudioSystem]            │
└──────────────────────────────────────────────────────────────────────────────┘
                     │
┌────────────────────┼─────────────────────────────────────────────────────────┐
│                    │  PHASE 5: VIEWS                                         │
├────────────────────┼─────────────────────────────────────────────────────────┤
│  [P5.T1 CardView] [P5.T2 GridView+DeckView] [P5.T3 HUDView]                  │
│  [P5.T4 SettingsPanelView] [P5.T5 WinPanel+ResetConfirm]                     │
└──────────────────────────────────────────────────────────────────────────────┘
                     │
┌────────────────────┼─────────────────────────────────────────────────────────┐
│                    │  PHASE 6: WIRING                                        │
├────────────────────┼─────────────────────────────────────────────────────────┤
│  [P6.T1 LifetimeScopes] ──► [P6.T2 BootstrapEntryPoint]                      │
└──────────────────────────────────────────────────────────────────────────────┘
                     │
┌────────────────────┼─────────────────────────────────────────────────────────┐
│                    │  PHASE 7: UNITY SETUP & INTEGRATION                     │
├────────────────────┼─────────────────────────────────────────────────────────┤
│  [P7.T1 Scenes] [P7.T2 Prefabs] [P7.T3 SO Assets]                            │
│  [P7.T4 Scene Wiring] [P7.T5 Integration Tests]                              │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Phases

### Phase 1: Foundation
**Goal:** Establish assembly structure, data models, and message contracts.  
**Parallel Capacity:** 3 agents  
**Entry Criteria:** None (first phase)  
**Exit Criteria:** All files compile, assembly references correct

---

#### P1.T1: Assembly Definitions
- **Type:** infrastructure
- **Agent:** coder
- **Inputs:** TDD Section 5 (Assembly Definitions)
- **Outputs:**
  - `Assets/Scripts/Logic/CardMatch.Logic.asmdef`
  - `Assets/Scripts/Runtime/CardMatch.Runtime.asmdef`
  - `Assets/Tests/Logic/CardMatch.Logic.Tests.asmdef`
  - `Assets/Tests/Runtime/CardMatch.Runtime.Tests.asmdef`
- **Description:** Create all four assembly definition files per TDD Section 5. Logic has NO references. Runtime references Logic + VContainer + MessagePipe + UniTask + LitMotion. Test assemblies reference their targets + Unity test framework.
- **Acceptance Criteria:**
  - [ ] CardMatch.Logic.asmdef has no external references
  - [ ] CardMatch.Runtime.asmdef references Logic assembly and third-party packages
  - [ ] Test asmdefs have testOnly: true and reference appropriate assemblies
  - [ ] No circular dependencies between assemblies
- **Complexity:** S
- **Parallel Group:** P1-A

---

#### P1.T2: Data Models
- **Type:** logic
- **Agent:** coder
- **Inputs:** TDD Section 8 (System MVS breakdowns), TDD Section 10.1 (Save Data Schema)
- **Outputs:**
  - `Assets/Scripts/Logic/Models/CardState.cs`
  - `Assets/Scripts/Logic/Models/GamePhase.cs`
  - `Assets/Scripts/Logic/Models/CardModel.cs`
  - `Assets/Scripts/Logic/Models/GameStateModel.cs`
  - `Assets/Scripts/Logic/Models/GridModel.cs`
- **Description:** 
  - Create `CardState` enum: InDeck, FaceDown, FaceUp, Matched
  - Create `GamePhase` enum: Loading, Dealing, Playing, Paused, Win
  - Create `CardModel` with: GridIndex (int), TypeId (int), State (CardState). Pure C# class, mutable.
  - Create `GameStateModel` with: Score, StrikeCount, FailCount, MaxStrike (all int), Phase (GamePhase). Pure C# class.
  - Create `GridModel` with: CardTypeIds (int[16]) mapping grid index to card type.
  - NO Unity dependencies. NO ReactiveProperty yet (added in Runtime layer).
- **Acceptance Criteria:**
  - [ ] All enums define exact states from TDD
  - [ ] CardModel has GridIndex, TypeId, State properties
  - [ ] GameStateModel has Score, StrikeCount, FailCount, MaxStrike, Phase
  - [ ] GridModel has CardTypeIds array of length 16
  - [ ] NO `using UnityEngine` in any file
  - [ ] All classes in CardMatch.Logic namespace
- **Complexity:** S
- **Parallel Group:** P1-A (can start immediately)

---

#### P1.T3: Message Definitions
- **Type:** logic
- **Agent:** coder
- **Inputs:** TDD Section 7.1 (MessagePipe Event System)
- **Outputs:**
  - `Assets/Scripts/Logic/Messages/GameMessages.cs`
- **Description:** Define all MessagePipe messages as readonly structs per TDD Section 7.1:
  - `CardFlippedMessage`: cardIndex (int), newState (CardState)
  - `MatchResultMessage`: isMatch (bool), cardIndex1 (int), cardIndex2 (int), scoreDelta (int), newStrike (int), newFailCount (int)
  - `PenaltyAppliedMessage`: penaltyAmount (int), newScore (int)
  - `GamePhaseChangedMessage`: newPhase (GamePhase)
  - `GameWonMessage`: finalScore (int), bestScore (int), maxStrike (int)
  - `SettingsChangedMessage`: musicVolume (float), sfxVolume (float)
  - `ResetRequestedMessage`: (empty struct for reset button)
- **Acceptance Criteria:**
  - [ ] All 7 message types defined as readonly struct
  - [ ] Each has constructor accepting all fields
  - [ ] All fields are readonly
  - [ ] NO Unity dependencies
- **Complexity:** S
- **Parallel Group:** P1-A

---

### Phase 2: Core Game Logic
**Goal:** Implement all pure C# game systems.  
**Parallel Capacity:** 2-3 agents (dependency ordering)  
**Entry Criteria:** Phase 1 complete  
**Exit Criteria:** All systems compile, logic matches TDD pseudo code

---

#### P2.T1: CardSystem
- **Type:** logic
- **Agent:** coder
- **Inputs:** 
  - `Assets/Scripts/Logic/Models/CardModel.cs`
  - `Assets/Scripts/Logic/Models/CardState.cs`
  - `Assets/Scripts/Logic/Messages/GameMessages.cs`
  - TDD Section 8.2
- **Outputs:**
  - `Assets/Scripts/Logic/Systems/CardSystem.cs`
- **Description:** Implement CardSystem per TDD Section 8.2:
  - Constructor takes CardModel[] (16 cards)
  - `FlipCard(int cardIndex)`: if state is FaceDown → set FaceUp, return true. Else return false.
  - `CloseCard(int cardIndex)`: if state is FaceUp → set FaceDown, return true. Else return false.
  - `MarkMatched(int cardIndex1, int cardIndex2)`: set both to Matched state.
  - `ResetAllCards()`: set all cards to InDeck state.
  - `GetCard(int index)`: return CardModel at index.
  - `GetFaceUpCards()`: return list of indices where state is FaceUp.
  - `GetMatchedCount()`: return count of Matched cards.
  - `AreAllMatched()`: return true if all 16 cards are Matched.
  - System does NOT publish messages directly — caller handles that.
- **Acceptance Criteria:**
  - [ ] FlipCard only works on FaceDown cards
  - [ ] CloseCard only works on FaceUp cards
  - [ ] MarkMatched sets both cards to Matched
  - [ ] ResetAllCards sets all to InDeck
  - [ ] AreAllMatched returns true when all 16 matched
  - [ ] NO Unity dependencies
- **Complexity:** M
- **Parallel Group:** P2-A (can run with P2.T2)

---

#### P2.T2: GridSystem
- **Type:** logic
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Logic/Models/CardModel.cs`
  - `Assets/Scripts/Logic/Models/GridModel.cs`
  - TDD Section 8.4
- **Outputs:**
  - `Assets/Scripts/Logic/Systems/GridSystem.cs`
- **Description:** Implement GridSystem per TDD Section 8.4:
  - Constructor takes GridModel and CardModel[] references
  - `Shuffle(int? seed = null)`: Fisher-Yates shuffle to create random pairing
    - Create array [0,0,1,1,2,2,3,3,4,4,5,5,6,6,7,7]
    - Fisher-Yates in-place shuffle (use System.Random with optional seed for testing)
    - Assign shuffled array to GridModel.CardTypeIds
    - Update each CardModel.TypeId from GridModel
  - `GetCardTypeAt(int gridIndex)`: return type ID at grid position
  - Use System.Random, NOT UnityEngine.Random
- **Acceptance Criteria:**
  - [ ] Shuffle creates exactly 8 pairs (each type 0-7 appears twice)
  - [ ] Fisher-Yates produces uniform distribution
  - [ ] Optional seed parameter enables deterministic testing
  - [ ] Updates both GridModel and CardModel[].TypeId
  - [ ] NO Unity dependencies
- **Complexity:** S
- **Parallel Group:** P2-A (can run with P2.T1)

---

#### P2.T3: MatchSystem
- **Type:** logic
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Logic/Systems/CardSystem.cs`
  - `Assets/Scripts/Logic/Models/GameStateModel.cs`
  - `Assets/Scripts/Logic/Messages/GameMessages.cs`
  - TDD Section 8.3, GDD Section 5 (Scoring/Penalty rules)
- **Outputs:**
  - `Assets/Scripts/Logic/Systems/MatchSystem.cs`
- **Description:** Implement MatchSystem per TDD Section 8.3. Core gameplay logic.
  - Constructor takes CardSystem and GameStateModel
  - State: firstSelectedIndex (int?), secondSelectedIndex (int?)
  - `SelectCard(int cardIndex) → MatchSelectionResult`: 
    - If card not FaceDown, return Ignored
    - Call CardSystem.FlipCard
    - If firstSelected is null: store as first, return WaitingForSecond
    - Else: store as second, return ReadyToEvaluate
  - `CancelSelection()`: if first selected, close it via CardSystem, clear first
  - `EvaluateMatch() → MatchEvaluationResult`: per TDD pseudo code
    - Check if types match
    - If match: calculate score (1 + strikeCount + 1), increment strike, reset fail, mark matched
    - If no match: reset strike to 0, increment fail, check penalty thresholds
    - Return result struct with all deltas and flags
  - `ApplyPenalty(int failCount) → int`: check thresholds [4,6,8], return penalty [1,2,3] or 0
  - `ResetSelection()`: clear first and second
  - `ResetGame()`: reset GameStateModel to initial values
  - Score cannot go below 0.
- **Acceptance Criteria:**
  - [ ] First match scores 1 point (1 base + 0 strike)
  - [ ] Second consecutive match scores 3 points (1 base + 2 strike)
  - [ ] Failed match resets strike to 0
  - [ ] 4th consecutive fail applies -1 penalty
  - [ ] 6th consecutive fail applies -2 penalty
  - [ ] 8th consecutive fail applies -3 penalty
  - [ ] Score never goes below 0
  - [ ] MaxStrike tracks highest strike achieved
  - [ ] NO Unity dependencies
- **Complexity:** L
- **Parallel Group:** P2-B (after CardSystem)

---

#### P2.T4: GameFlowSystem
- **Type:** logic
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Logic/Systems/GridSystem.cs`
  - `Assets/Scripts/Logic/Systems/CardSystem.cs`
  - `Assets/Scripts/Logic/Models/GameStateModel.cs`
  - `Assets/Scripts/Logic/Messages/GameMessages.cs`
  - TDD Section 8.1
- **Outputs:**
  - `Assets/Scripts/Logic/Systems/GameFlowSystem.cs`
- **Description:** Implement GameFlowSystem per TDD Section 8.1. Phase state machine.
  - Constructor takes GameStateModel, GridSystem, CardSystem
  - `SetPhase(GamePhase phase)`: update model, return phase changed message data
  - `StartNewGame()`: shuffle via GridSystem, reset cards via CardSystem, set phase to Dealing
  - `OnDealingComplete()`: set phase to Playing
  - `OnAllCardsMatched(int finalScore, int bestScore) → GameWonMessage`: set phase to Win, return win message
  - `Pause()`: set phase to Paused
  - `Resume()`: set phase to Playing
  - `IsInputAllowed()`: return true only if phase is Playing
  - `RestoreFromSave(GameStateModel savedState)`: restore phase and stats
- **Acceptance Criteria:**
  - [ ] Phase transitions follow TDD state diagram
  - [ ] StartNewGame shuffles and resets
  - [ ] IsInputAllowed returns false during Dealing, Paused, Win
  - [ ] Pause/Resume toggle between Paused and Playing
  - [ ] NO Unity dependencies
- **Complexity:** M
- **Parallel Group:** P2-B (can run with MatchSystem)

---

### Phase 3: Unit Tests
**Goal:** Full test coverage for all pure C# logic.  
**Parallel Capacity:** 2-3 agents  
**Entry Criteria:** Phase 2 complete  
**Exit Criteria:** All tests pass, coverage >90% for logic

---

#### P3.T1: CardSystem Tests
- **Type:** test
- **Agent:** tester
- **Inputs:**
  - `Assets/Scripts/Logic/Systems/CardSystem.cs`
  - TDD Section 14.1
- **Outputs:**
  - `Assets/Tests/Logic/CardSystemTests.cs`
- **Description:** Write unit tests per TDD Section 14.1:
  - `FlipCard_FaceDown_BecomesFaceUp`
  - `FlipCard_AlreadyFaceUp_ReturnsFalse`
  - `FlipCard_AlreadyMatched_ReturnsFalse`
  - `CloseCard_FaceUp_BecomesFaceDown`
  - `CloseCard_NotFaceUp_ReturnsFalse`
  - `MarkMatched_TwoCards_BothBecomeMatched`
  - `ResetAllCards_SetsAllToInDeck`
  - `AreAllMatched_AllMatched_ReturnsTrue`
  - `AreAllMatched_SomeRemaining_ReturnsFalse`
- **Acceptance Criteria:**
  - [ ] All tests follow `MethodUnderTest_Scenario_ExpectedResult` naming
  - [ ] Each test has clear Arrange/Act/Assert sections
  - [ ] Tests run without Unity (pure NUnit)
  - [ ] All edge cases covered
- **Complexity:** M
- **Parallel Group:** P3-A

---

#### P3.T2: GridSystem Tests
- **Type:** test
- **Agent:** tester
- **Inputs:**
  - `Assets/Scripts/Logic/Systems/GridSystem.cs`
  - TDD Section 14.1
- **Outputs:**
  - `Assets/Tests/Logic/GridSystemTests.cs`
- **Description:** Write unit tests:
  - `Shuffle_Creates8Pairs`: verify each type 0-7 appears exactly twice
  - `Shuffle_AllCardsAssigned`: verify all 16 positions have valid types
  - `Shuffle_WithSeed_IsDeterministic`: same seed produces same layout
  - `Shuffle_DifferentSeeds_ProduceDifferentLayouts`
  - `Shuffle_UpdatesCardModelTypes`: verify CardModel[].TypeId matches GridModel
- **Acceptance Criteria:**
  - [ ] Pair count verification is exact
  - [ ] Deterministic seed test uses fixed known output
  - [ ] Tests run without Unity
- **Complexity:** S
- **Parallel Group:** P3-A

---

#### P3.T3: MatchSystem Tests
- **Type:** test
- **Agent:** tester
- **Inputs:**
  - `Assets/Scripts/Logic/Systems/MatchSystem.cs`
  - TDD Section 14.1, GDD Appendix B (Score examples)
- **Outputs:**
  - `Assets/Tests/Logic/MatchSystemTests.cs`
- **Description:** Write comprehensive tests covering GDD scoring rules:
  - `SelectCard_FirstCard_ReturnsWaitingForSecond`
  - `SelectCard_SecondCard_ReturnsReadyToEvaluate`
  - `SelectCard_AlreadyFaceUp_ReturnsIgnored`
  - `EvaluateMatch_SameType_IsMatch`
  - `EvaluateMatch_DifferentType_NotMatch`
  - `Scoring_FirstMatch_AddsOnePoint`
  - `Scoring_SecondConsecutiveMatch_AddsThreePoints`
  - `Scoring_ThirdConsecutiveMatch_AddsFourPoints`
  - `Strike_ResetOnFail_BecomesZero`
  - `Penalty_FourthFail_SubtractsOnePoint`
  - `Penalty_SixthFail_SubtractsTwoPoints`
  - `Penalty_EighthFail_SubtractsThreePoints`
  - `Penalty_ScoreCannotGoBelowZero`
  - `MaxStrike_TracksHighestStreak`
  - `PerfectGame_Scores43Points` (per GDD Appendix B)
- **Acceptance Criteria:**
  - [ ] All GDD Appendix B examples verified
  - [ ] Penalty thresholds exact
  - [ ] Score floor at 0 verified
  - [ ] MaxStrike tracking verified
- **Complexity:** L
- **Parallel Group:** P3-B (after P3-A or in parallel if agent available)

---

#### P3.T4: GameFlowSystem Tests
- **Type:** test
- **Agent:** tester
- **Inputs:**
  - `Assets/Scripts/Logic/Systems/GameFlowSystem.cs`
  - TDD Section 14.1
- **Outputs:**
  - `Assets/Tests/Logic/GameFlowSystemTests.cs`
- **Description:** Write unit tests:
  - `StartNewGame_SetsPhaseToDealing`
  - `OnDealingComplete_SetsPhaseToPlaying`
  - `Pause_SetsPhaseTopaused`
  - `Resume_SetsPhaseToPlaying`
  - `IsInputAllowed_DuringPlaying_ReturnsTrue`
  - `IsInputAllowed_DuringDealing_ReturnsFalse`
  - `IsInputAllowed_DuringPaused_ReturnsFalse`
  - `OnAllCardsMatched_SetsPhaseToWin`
- **Acceptance Criteria:**
  - [ ] All phase transitions tested
  - [ ] Input blocking verified for non-Playing phases
- **Complexity:** S
- **Parallel Group:** P3-B

---

### Phase 4: Unity Integration Layer
**Goal:** Create ScriptableObject definitions and Unity-dependent services.  
**Parallel Capacity:** 3 agents  
**Entry Criteria:** Phase 3 tests pass  
**Exit Criteria:** All scripts compile in Unity

---

#### P4.T1: ScriptableObject Definitions
- **Type:** integration
- **Agent:** coder
- **Inputs:** TDD Section 7.3 (Configuration System)
- **Outputs:**
  - `Assets/Scripts/Runtime/ScriptableObjects/GameConfig.cs`
  - `Assets/Scripts/Runtime/ScriptableObjects/AudioConfig.cs`
  - `Assets/Scripts/Runtime/ScriptableObjects/CardDefinitions.cs`
- **Description:** Create SO definition scripts:
  - **GameConfig**: cardCount (16), gridColumns (4), dealDuration (3f), flipDuration (0.3f), noMatchRevealTime (2f), autoCloseTime (10f), penaltyThresholds (int[]), penaltyAmounts (int[])
  - **AudioConfig**: bgmClip, flipClip, matchClip, strikeClip, penaltyClip, winClip (all AudioClip)
  - **CardDefinitions**: faceSprites (Sprite[8]), backSprite (Sprite)
  - All with [CreateAssetMenu] attribute for easy asset creation.
- **Acceptance Criteria:**
  - [ ] All fields use [SerializeField]
  - [ ] CreateAssetMenu paths are logical (e.g., "CardMatch/GameConfig")
  - [ ] Default values set where sensible
- **Complexity:** S
- **Parallel Group:** P4-A

---

#### P4.T2: SaveSystem
- **Type:** integration
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Logic/Models/*.cs`
  - TDD Section 8.5, TDD Section 10.1
- **Outputs:**
  - `Assets/Scripts/Runtime/Services/SaveSystem.cs`
  - `Assets/Scripts/Runtime/Services/SaveData.cs`
- **Description:** Implement SaveSystem per TDD Section 8.5:
  - **SaveData class**: serializable class matching TDD Section 10.1 JSON schema
  - **SaveSystem**: 
    - `SaveGameState(GameStateModel, CardModel[])`: serialize to JSON, store in PlayerPrefs
    - `TryLoadGameState(out SaveData)`: load from PlayerPrefs, return success
    - `ClearGameState()`: delete PlayerPrefs key
    - `SaveBestScore(int)`: save if higher than current
    - `LoadBestScore() → int`: return 0 if not found
    - `SaveSettings(float music, float sfx)`
    - `LoadSettings() → (float, float)`: return (1f, 1f) defaults
  - Use UnityEngine.JsonUtility for serialization.
- **Acceptance Criteria:**
  - [ ] Round-trip save/load produces identical data
  - [ ] Best score only updates if higher
  - [ ] Missing data returns sensible defaults
  - [ ] Uses PlayerPrefs (WebGL compatible)
- **Complexity:** M
- **Parallel Group:** P4-A

---

#### P4.T3: AudioSystem
- **Type:** integration
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Runtime/ScriptableObjects/AudioConfig.cs`
  - TDD Section 8.6
- **Outputs:**
  - `Assets/Scripts/Runtime/Services/AudioSystem.cs`
  - `Assets/Scripts/Runtime/Services/AudioSettingsModel.cs`
- **Description:** Implement AudioSystem per TDD Section 8.6:
  - **AudioSettingsModel**: simple class with MusicVolume (float), SFXVolume (float)
  - **AudioSystem**:
    - Constructor/Inject: AudioConfig, AudioSettingsModel, two AudioSource refs (BGM, SFX)
    - `Initialize()`: setup BGM source (loop, play)
    - `PlayFlip()`, `PlayMatch()`, `PlayStrike()`, `PlayPenalty()`, `PlayWin()`: one-shot SFX
    - `SetMusicVolume(float)`, `SetSFXVolume(float)`: update model and source
    - Implements IDisposable for cleanup
- **Acceptance Criteria:**
  - [ ] BGM loops continuously
  - [ ] SFX play at current volume
  - [ ] Volume changes apply immediately
  - [ ] Strike sound plays alongside match sound (not replacing)
- **Complexity:** M
- **Parallel Group:** P4-A

---

### Phase 5: Views
**Goal:** Implement all MonoBehaviour views with animations.  
**Parallel Capacity:** 3 agents  
**Entry Criteria:** Phase 4 complete  
**Exit Criteria:** All views compile, animations defined

---

#### P5.T1: CardView
- **Type:** integration
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Logic/Models/CardModel.cs`
  - `Assets/Scripts/Runtime/ScriptableObjects/CardDefinitions.cs`
  - TDD Section 8.2 (Card States), TDD Section 9.5 (Animation)
- **Outputs:**
  - `Assets/Scripts/Runtime/Views/CardView.cs`
- **Description:** Core card visual component:
  - Serialized fields: Image component ref, CardDefinitions ref
  - Inject: CardModel (assigned at runtime), MatchSystem
  - `Initialize(CardModel, int index)`: bind to model
  - `OnPointerClick(PointerEventData)`: call MatchSystem.SelectCard
  - `PlayFlipAnimation(CardState newState)`: LitMotion scale X flip (1→0→1 with sprite swap at midpoint)
  - `PlayMatchedAnimation()`: animate to deck position then deactivate
  - `SetCardFace(int typeId)`: set sprite from CardDefinitions
  - `SetFaceDown()`: show back sprite
  - Uses UniTask + CancellationToken for async animations
  - Implements IPointerClickHandler
- **Acceptance Criteria:**
  - [ ] Click triggers card selection
  - [ ] Flip animation has scale-X approach (not rotation)
  - [ ] Sprite swaps at animation midpoint
  - [ ] Matched animation moves card toward deck
  - [ ] Proper cancellation on destroy
- **Complexity:** L
- **Parallel Group:** P5-A

---

#### P5.T2: GridView and DeckView
- **Type:** integration
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Logic/Models/GridModel.cs`
  - `Assets/Scripts/Runtime/Views/CardView.cs`
  - TDD Section 9.2 (Canvas Structure)
- **Outputs:**
  - `Assets/Scripts/Runtime/Views/GridView.cs`
  - `Assets/Scripts/Runtime/Views/DeckView.cs`
- **Description:**
  - **GridView**: 
    - Holds references to 16 CardView children
    - `GetCardPosition(int index) → Vector2`: return world position for grid slot
    - `GetCardViews() → CardView[]`: return all card views
    - Layout handled by Unity's GridLayoutGroup (configured in scene)
  - **DeckView**:
    - Visual representation of card deck
    - `GetDeckPosition() → Vector2`: return position for animations
    - `PlayShuffleAnimation()`: subtle visual feedback on reset
- **Acceptance Criteria:**
  - [ ] GridView provides position lookup for 4x4 grid
  - [ ] DeckView provides target position for matched card animations
- **Complexity:** M
- **Parallel Group:** P5-A

---

#### P5.T3: HUDView
- **Type:** integration
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Logic/Models/GameStateModel.cs`
  - TDD Section 9.4 (Data Binding)
- **Outputs:**
  - `Assets/Scripts/Runtime/Views/HUDView.cs`
- **Description:** Score and strike display:
  - Serialized fields: TMP_Text for score, TMP_Text for strike, GameObject strikeContainer, Button settingsButton
  - Subscribe to GameStateModel changes (score, strike)
  - `UpdateScore(int score)`: update text, play flash animation on change
  - `UpdateStrike(int strike)`: update text, show/hide container, play pulse on increase
  - Settings button click opens settings panel (via injected reference or message)
  - Use LitMotion for pulse/flash animations
- **Acceptance Criteria:**
  - [ ] Score updates immediately on change
  - [ ] Strike counter shows "x{N}" format
  - [ ] Strike container hidden when strike is 0
  - [ ] Pulse animation on strike increase
  - [ ] Red flash on penalty
- **Complexity:** M
- **Parallel Group:** P5-A

---

#### P5.T4: SettingsPanelView
- **Type:** integration
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Runtime/Services/AudioSystem.cs`
  - TDD Section 7.4 (UI Layout)
- **Outputs:**
  - `Assets/Scripts/Runtime/Views/SettingsPanelView.cs`
- **Description:** Settings overlay:
  - Serialized fields: Slider musicSlider, Slider sfxSlider, Button resetButton, Button closeButton
  - Inject: AudioSystem, GameFlowSystem
  - `Open()`: show panel, pause game
  - `Close()`: hide panel, resume game
  - Slider callbacks update AudioSystem volumes
  - Reset button shows ResetConfirmPopupView
- **Acceptance Criteria:**
  - [ ] Opens as overlay (does not destroy game state)
  - [ ] Volume sliders update audio in real-time
  - [ ] Close resumes game
  - [ ] Reset shows confirmation first
- **Complexity:** M
- **Parallel Group:** P5-B

---

#### P5.T5: WinPanelView and ResetConfirmPopupView
- **Type:** integration
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Logic/Models/GameStateModel.cs`
  - TDD Section 7.5 (Win Panel)
- **Outputs:**
  - `Assets/Scripts/Runtime/Views/WinPanelView.cs`
  - `Assets/Scripts/Runtime/Views/ResetConfirmPopupView.cs`
- **Description:**
  - **WinPanelView**:
    - Serialized fields: TMP_Text for score, best, maxStrike; Button newGameButton
    - `Show(int score, int bestScore, int maxStrike)`: populate and display
    - New game button triggers GameFlowSystem.StartNewGame
  - **ResetConfirmPopupView**:
    - Turkish text: "Emin misiniz?" with "EVET" / "HAYIR" buttons
    - Evet: close all panels, start new game
    - Hayır: close popup only
- **Acceptance Criteria:**
  - [ ] Win panel shows all three stats
  - [ ] Best score updates if current is higher
  - [ ] Reset confirm has Turkish text
  - [ ] Evet starts new game, Hayır cancels
- **Complexity:** M
- **Parallel Group:** P5-B

---

### Phase 6: Wiring (VContainer)
**Goal:** Create DI containers and bootstrap logic.  
**Parallel Capacity:** 1-2 agents (sequential dependency)  
**Entry Criteria:** Phase 5 complete  
**Exit Criteria:** All registrations compile, scope hierarchy correct

---

#### P6.T1: LifetimeScopes
- **Type:** integration
- **Agent:** coder
- **Inputs:**
  - All Systems and Views
  - TDD Section 4.1 (Initialization Sequence), TDD Section 7.4 (VContainer)
- **Outputs:**
  - `Assets/Scripts/Runtime/LifetimeScopes/RootLifetimeScope.cs`
  - `Assets/Scripts/Runtime/LifetimeScopes/GameLifetimeScope.cs`
- **Description:**
  - **RootLifetimeScope**:
    - Serialized: AudioConfig, GameConfig
    - Register: AudioSettingsModel (Singleton), SaveSystem (Singleton), AudioSystem (Singleton)
    - Mark as DontDestroyOnLoad in Awake
    - Register MessagePipe options
  - **GameLifetimeScope**:
    - Parent: RootLifetimeScope (set via serialized field or autoInject)
    - Register all Models as Singleton
    - Register all Logic Systems as Singleton with AsSelf().AsImplementedInterfaces()
    - Register all Views via RegisterComponentInHierarchy
    - Register MessagePipe brokers for all message types
- **Acceptance Criteria:**
  - [ ] RootLifetimeScope survives scene loads
  - [ ] GameLifetimeScope inherits from Root
  - [ ] All Systems receive correct dependencies
  - [ ] All Views are injected properly
  - [ ] MessagePipe brokers registered for all 7 message types
- **Complexity:** L
- **Parallel Group:** P6-A

---

#### P6.T2: BootstrapEntryPoint
- **Type:** integration
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Runtime/LifetimeScopes/RootLifetimeScope.cs`
  - `Assets/Scripts/Runtime/Services/SaveSystem.cs`
  - TDD Section 4.1
- **Outputs:**
  - `Assets/Scripts/Runtime/EntryPoints/BootstrapEntryPoint.cs`
- **Description:** Application entry point using VContainer's IAsyncStartable:
  - `InitializeAsync(CancellationToken)`:
    1. Load settings from SaveSystem
    2. Initialize AudioSystem (start BGM)
    3. Load GameScene additively
  - Registered in RootLifetimeScope as EntryPoint
- **Acceptance Criteria:**
  - [ ] Implements IAsyncStartable
  - [ ] Loads settings before audio init
  - [ ] Loads GameScene additively (not replacing BootScene)
  - [ ] Uses UniTask for async scene load
- **Complexity:** S
- **Parallel Group:** P6-B (after P6.T1)

---

### Phase 7: Unity Setup & Integration
**Goal:** Create scenes, prefabs, assets, and final wiring via Unity MCP.  
**Parallel Capacity:** 2 agents (Unity MCP + tester)  
**Entry Criteria:** Phase 6 complete, code compiles  
**Exit Criteria:** Game runs end-to-end

---

#### P7.T1: Scene Creation
- **Type:** unity-setup
- **Agent:** unity-setup
- **Inputs:** TDD Section 11.1 (Scene Hierarchy)
- **Outputs:**
  - `Assets/Scenes/BootScene.unity`
  - `Assets/Scenes/GameScene.unity`
- **Description:** Create scenes via Unity MCP:
  - **BootScene**: 
    - Empty scene with single GameObject "RootLifetimeScope"
    - Attach RootLifetimeScope.cs
  - **GameScene**:
    - MainCamera (Orthographic)
    - Canvas (Screen Space Overlay, CanvasScaler 1080x1920)
    - EventSystem
    - GameLifetimeScope GameObject
    - Audio GameObject with BGMSource and SFXSource AudioSources
- **Acceptance Criteria:**
  - [ ] BootScene is minimal (just scope object)
  - [ ] GameScene has camera, canvas, event system
  - [ ] Build settings: Boot=0, Game=1
- **Complexity:** M
- **Parallel Group:** P7-A

---

#### P7.T2: Prefab Creation
- **Type:** unity-setup
- **Agent:** unity-setup
- **Inputs:** 
  - TDD Section 11.2 (Prefab Inventory)
  - All View scripts
- **Outputs:**
  - `Assets/Prefabs/Card.prefab`
  - `Assets/Prefabs/UI/SettingsPanel.prefab`
  - `Assets/Prefabs/UI/WinPanel.prefab`
  - `Assets/Prefabs/UI/ResetConfirmPopup.prefab`
- **Description:** Create prefabs via Unity MCP:
  - **Card.prefab**: Image component, CardView.cs, Button or IPointerClickHandler
  - **SettingsPanel.prefab**: Panel with sliders, buttons, SettingsPanelView.cs
  - **WinPanel.prefab**: Panel with stats texts, button, WinPanelView.cs
  - **ResetConfirmPopup.prefab**: Small popup with Turkish text, two buttons
- **Acceptance Criteria:**
  - [ ] Card prefab has Image and CardView components
  - [ ] UI prefabs match GDD mockups
  - [ ] All text is TextMeshPro
- **Complexity:** M
- **Parallel Group:** P7-A

---

#### P7.T3: ScriptableObject Assets
- **Type:** unity-setup
- **Agent:** unity-setup
- **Inputs:** 
  - `Assets/Scripts/Runtime/ScriptableObjects/*.cs`
  - TDD Section 7.3
- **Outputs:**
  - `Assets/ScriptableObjects/Config/GameConfig.asset`
  - `Assets/ScriptableObjects/Config/AudioConfig.asset`
  - `Assets/ScriptableObjects/Definitions/CardDefinitions.asset`
- **Description:** Create SO asset instances via Unity MCP:
  - **GameConfig.asset**: Set default values from TDD (cardCount=16, etc.)
  - **AudioConfig.asset**: Leave clip fields empty (assign later)
  - **CardDefinitions.asset**: Leave sprite fields empty (assign later)
- **Acceptance Criteria:**
  - [ ] All assets created in correct folders
  - [ ] GameConfig has sensible defaults
- **Complexity:** S
- **Parallel Group:** P7-A

---

#### P7.T4: Scene Wiring
- **Type:** unity-setup
- **Agent:** unity-setup
- **Inputs:**
  - All prefabs and SO assets
  - TDD Section 9.2 (Canvas Structure), Section 11.3 (Scene Setup Checklist)
- **Outputs:**
  - Updated `Assets/Scenes/GameScene.unity`
- **Description:** Final scene assembly via Unity MCP:
  - Instantiate 16 Card prefabs in GridView area (4x4 GridLayoutGroup)
  - Add SafeArea handler to canvas container
  - Wire up LifetimeScope serialized references to SO assets
  - Set up canvas hierarchy per TDD Section 9.2
  - Configure GridLayoutGroup for 4x4 card layout
- **Acceptance Criteria:**
  - [ ] 16 cards in grid layout
  - [ ] All panels in Overlays container (disabled by default)
  - [ ] LifetimeScope references assigned
  - [ ] Safe area handling enabled
- **Complexity:** L
- **Parallel Group:** P7-B (after P7.T1-T3)

---

#### P7.T5: Integration Tests
- **Type:** test
- **Agent:** tester
- **Inputs:**
  - Complete game setup
  - TDD Section 14.3
- **Outputs:**
  - `Assets/Tests/Runtime/SaveSystemTests.cs`
  - `Assets/Tests/Runtime/FullGameFlowTests.cs`
- **Description:** Write integration tests that run in Unity:
  - **SaveSystemTests**:
    - `SaveAndLoad_GameState_RoundTrips`
    - `Load_NoSavedState_ReturnsFalse`
    - `BestScore_OnlyUpdatesIfHigher`
  - **FullGameFlowTests**:
    - `NewGame_StartsInDealingPhase`
    - `MatchAllPairs_EntersWinPhase`
- **Acceptance Criteria:**
  - [ ] Tests use Unity Test Framework (Play Mode)
  - [ ] Save/load round-trips correctly
  - [ ] Full game flow can be simulated
- **Complexity:** M
- **Parallel Group:** P7-B

---

## 4. Agent Team Configuration

| Agent | Phases Active | Primary Responsibilities |
|-------|---------------|-------------------------|
| Coder 1 | P1, P2, P4, P5, P6 | Infrastructure, CardSystem, ScriptableObjects, CardView, LifetimeScopes |
| Coder 2 | P1, P2, P4, P5 | Models/Messages, GridSystem, Services, GridView/DeckView |
| Coder 3 | P2, P4, P5, P6 | MatchSystem, GameFlowSystem, HUDView, UI Panels, EntryPoint |
| Tester | P3, P7 | All unit tests, integration tests |
| Unity-Setup | P7 | Scenes, prefabs, SO assets, final wiring |
| Reviewer | All phases | Review checkpoints after each phase |

---

## 5. Review Checkpoints

| Checkpoint | Trigger | Focus Areas |
|------------|---------|-------------|
| RC1 | After Phase 1 | Assembly refs correct, models match TDD, messages complete |
| RC2 | After Phase 2 | Systems implement TDD pseudo code, no Unity deps in Logic |
| RC3 | After Phase 3 | All tests pass, scoring rules verified per GDD |
| RC4 | After Phase 4 | SO definitions match TDD, services implement spec |
| RC5 | After Phase 5 | Views compile, animations defined, UI matches GDD mockups |
| RC6 | After Phase 6 | VContainer wiring correct, scope hierarchy valid |
| RC7 | After Phase 7 | Game runs end-to-end, all GDD features work |

---

## 6. Risk Register

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| MatchSystem complexity | High | Medium | Detailed tests for all scoring edge cases |
| VContainer wiring errors | High | Medium | Careful review at RC6, test in isolation first |
| LitMotion animation timing | Medium | Low | Test flip animation early in P5.T1 |
| Unity MCP scene creation | Medium | Medium | Have fallback manual setup instructions |
| Merge conflicts in LifetimeScope | High | Low | Single agent owns P6 |
| Card flip visual not working | Medium | Low | Test scale-X approach in prototype |

---

## 7. Merge Strategy

### File Ownership
Each task owns specific files. No two tasks produce the same file.

| Folder | Primary Owner |
|--------|---------------|
| Scripts/Logic/Models | P1.T2 |
| Scripts/Logic/Messages | P1.T3 |
| Scripts/Logic/Systems | P2.T1-T4 (one file each) |
| Scripts/Runtime/ScriptableObjects | P4.T1 |
| Scripts/Runtime/Services | P4.T2-T3 |
| Scripts/Runtime/Views | P5.T1-T5 |
| Scripts/Runtime/LifetimeScopes | P6.T1 |
| Scripts/Runtime/EntryPoints | P6.T2 |
| Tests/Logic | P3.T1-T4 |
| Tests/Runtime | P7.T5 |

### Conflict Resolution
1. If two tasks need to modify the same file: task with lower ID takes priority, second task waits
2. LifetimeScopes may need updates as views are added: P6.T1 creates initial version, agent updates as needed
3. Review checkpoint catches integration issues before next phase

### Integration Verification
After each phase, run:
1. `dotnet build` (or Unity compile) to verify no syntax errors
2. Run all existing tests
3. Reviewer validates against TDD

---

## 8. Task Summary by Phase

| Phase | Tasks | Parallel Groups | Duration Estimate |
|-------|-------|-----------------|-------------------|
| P1: Foundation | 3 | 1 group (all parallel) | 30 min |
| P2: Core Logic | 4 | 2 groups (2+2) | 1 hour |
| P3: Unit Tests | 4 | 2 groups (2+2) | 45 min |
| P4: Unity Integration | 3 | 1 group (all parallel) | 45 min |
| P5: Views | 5 | 2 groups (3+2) | 1.5 hours |
| P6: Wiring | 2 | 2 groups (sequential) | 30 min |
| P7: Unity Setup | 5 | 2 groups (3+2) | 1 hour |
| **P8: 2D Refactor** | **5** | **2 groups (3+2)** | **1.5 hours** |

**Total Estimated Time:** ~7.5 hours with 3 parallel coders

---

## Phase 8: 2D View Refactor (NEW)

**Goal:** Convert cards from UI Canvas to 2D SpriteRenderer objects with centralized input handling.  
**Parallel Capacity:** 3 agents  
**Entry Criteria:** Phase 6 complete (wiring exists)  
**Exit Criteria:** Cards render as 2D objects, input works via Physics2D.Raycast

**Rationale:** TDD v1.1 changed architecture from UI-based cards to 2D SpriteRenderer objects for better rendering control and proper 2D game feel.

---

#### P8.T1: InputView (NEW)
- **Type:** integration
- **Agent:** coder
- **Inputs:**
  - TDD Section 7.6 (Input System)
  - `Assets/Scripts/Logic/Systems/MatchSystem.cs`
  - `Assets/Scripts/Logic/Systems/GameFlowSystem.cs`
- **Outputs:**
  - `Assets/Scripts/Runtime/Views/InputView.cs`
- **Description:** Create centralized 2D input handler:
  - MonoBehaviour with VContainer injection
  - In Update: check `GameFlowSystem.IsInputAllowed()`
  - On mouse click or touch: `Physics2D.Raycast` from screen position
  - If hit has CardView component: call `MatchSystem.SelectCard(gridIndex)`
  - Support both Input.GetMouseButtonDown and Input.GetTouch for WebGL
- **Acceptance Criteria:**
  - [ ] Click on card triggers MatchSystem.SelectCard
  - [ ] Click on empty space does nothing
  - [ ] Input blocked during non-Playing phases
  - [ ] Touch input works for mobile WebGL
- **Complexity:** M
- **Parallel Group:** P8-A

---

#### P8.T2: CardView 2D Refactor
- **Type:** refactor
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Runtime/Views/CardView.cs` (existing)
  - TDD Section 9 (Rendering Architecture)
- **Outputs:**
  - `Assets/Scripts/Runtime/Views/CardView.cs` (modified)
- **Description:** Convert CardView from UI to 2D:
  - Remove: `IPointerClickHandler`, `Image`, `RectTransform`, `UnityEngine.EventSystems`
  - Add: `SpriteRenderer` serialized field, `BoxCollider2D` (set via Inspector)
  - Change: `_rectTransform` → `_transform` (Transform)
  - Change: `anchoredPosition` animations → `position` animations
  - Add: `public int GridIndex { get; private set; }` property for InputView
  - Keep: flip animation logic (scale X approach works with Transform)
  - Update: `PlayMatchedAnimation` to use `Vector3` world position
- **Acceptance Criteria:**
  - [ ] No UI dependencies (Image, RectTransform, IPointerClickHandler removed)
  - [ ] SpriteRenderer displays card sprites
  - [ ] BoxCollider2D exists for raycast detection
  - [ ] Flip animation works with Transform.localScale
  - [ ] Match animation moves to world position
- **Complexity:** L
- **Parallel Group:** P8-A

---

#### P8.T3: GridView 2D Refactor
- **Type:** refactor
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Runtime/Views/GridView.cs` (existing)
  - TDD Section 9.4 (Camera Setup)
- **Outputs:**
  - `Assets/Scripts/Runtime/Views/GridView.cs` (modified)
- **Description:** Convert GridView from UI to 2D:
  - Remove: `RectTransform` references
  - Change: `GetCardPosition(int) → Vector3` (world position)
  - Add: Grid calculation logic (4x4 grid in world space)
    - Grid center: configurable (default: 1.5, 0)
    - Card spacing: configurable (default: 2.2 world units)
  - Add: `CalculateGridPosition(int gridIndex) → Vector3` helper
  - Keep: `GetCardViews()`, `GetCardView(int)`, `CardCount`
- **Acceptance Criteria:**
  - [ ] Returns Vector3 world positions
  - [ ] Grid positions form 4x4 layout
  - [ ] Positions are centered around grid anchor
- **Complexity:** M
- **Parallel Group:** P8-A

---

#### P8.T4: DeckView 2D Refactor
- **Type:** refactor
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Runtime/Views/DeckView.cs` (existing)
  - TDD Section 9.4 (Camera Setup)
- **Outputs:**
  - `Assets/Scripts/Runtime/Views/DeckView.cs` (modified)
- **Description:** Convert DeckView from UI to 2D:
  - Remove: `RectTransform` references
  - Add: `SpriteRenderer` for deck visual (optional)
  - Change: `GetDeckPosition() → Vector3` (world position)
  - Change: Shuffle animation to use `Transform.position`
- **Acceptance Criteria:**
  - [ ] Returns Vector3 world position
  - [ ] Shuffle animation works with Transform
- **Complexity:** S
- **Parallel Group:** P8-A

---

#### P8.T5: GameLifetimeScope Update
- **Type:** refactor
- **Agent:** coder
- **Inputs:**
  - `Assets/Scripts/Runtime/LifetimeScopes/GameLifetimeScope.cs` (existing)
  - `Assets/Scripts/Runtime/Views/InputView.cs` (from P8.T1)
- **Outputs:**
  - `Assets/Scripts/Runtime/LifetimeScopes/GameLifetimeScope.cs` (modified)
- **Description:** Register InputView in VContainer:
  - Add: `builder.RegisterComponentInHierarchy<InputView>();`
- **Acceptance Criteria:**
  - [ ] InputView is registered and receives injected dependencies
- **Complexity:** S
- **Parallel Group:** P8-B (after P8.T1)

---

#### P8.T6: Unity 2D Scene Setup
- **Type:** unity-setup
- **Agent:** unity-setup
- **Inputs:**
  - TDD Section 9.2 (Scene Structure), Section 11.3 (Scene Setup Checklist)
  - All refactored View scripts
- **Outputs:**
  - Updated `Assets/Scenes/GameScene.unity`
- **Description:** Reconfigure GameScene for 2D:
  - **Camera:** Orthographic, Size 10, Position (0, 0, -10)
  - **Sorting Layers:** Create Background, Cards layers
  - **World2D container:** Create empty GameObject as parent
  - **GridContainer:** Position at (1.5, 0)
  - **16 CardViews:** Each with SpriteRenderer + BoxCollider2D, positioned in 4x4 grid
  - **DeckView:** Position at (-3.5, 0), SpriteRenderer
  - **InputView:** Empty GameObject with InputView.cs
  - **Canvas_HUD:** Keep existing HUD, rename Canvas
  - **Canvas_Overlays:** Keep existing panels
- **Acceptance Criteria:**
  - [ ] Cards visible as 2D sprites in world space
  - [ ] Cards clickable via BoxCollider2D
  - [ ] HUD displays correctly over 2D scene
  - [ ] Camera shows full grid + deck
- **Complexity:** L
- **Parallel Group:** P8-B (after code refactors)

---

## P8 Review Checkpoint

| Checkpoint | Trigger | Focus Areas |
|------------|---------|-------------|
| RC8 | After Phase 8 | Cards render as 2D, input works, no UI dependencies in card code, scene hybrid structure correct |

---

**Workflow Plan Complete.** Run `/orchestrate` to begin automated execution.

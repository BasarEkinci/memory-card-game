# Card Match — Game Design Document

**Version:** 1.0  
**Date:** 2026-05-08  
**Status:** Complete — Ready for Architecture Phase

---

## 1. Executive Summary

Card Match is a classic memory/concentration card matching game with a modern twist. Players flip cards to find matching pairs, building streaks for bonus points while avoiding consecutive failures that incur penalties. The game features a single-scene architecture with no main menu, immediate gameplay, and persistent progress saving.

**Core Appeal:** Simple, satisfying gameplay loop with risk/reward mechanics through the strike system. Casual enough for all ages, engaging enough for score chasers.

---

## 2. Core Concept

### Genre & Sub-genre
- **Genre:** Puzzle / Casual
- **Sub-genre:** Memory / Concentration

### Core Fantasy
The satisfaction of remembering card positions and building winning streaks. The tension of deciding whether to play it safe or push for higher strikes.

### Unique Selling Points
1. **Strike System:** Consecutive matches multiply score, rewarding memory and confidence
2. **Progressive Penalty:** 4/6/8 consecutive failures incur escalating point deductions
3. **Instant Play:** No menus, no loading — game starts immediately
4. **Save & Resume:** Players can leave and continue exactly where they left off

### Reference Games & Differentiation
- **Classic Concentration/Memory:** Base mechanic
- **Differentiation:** Strike bonus system + penalty system creates risk/reward dynamic absent in traditional memory games

---

## 3. Target Audience & Platform

### Demographics
- **Age:** All ages (casual, family-friendly)
- **Player Type:** Casual players, puzzle enthusiasts, score chasers
- **Session Length:** 2-5 minutes per round

### Platform & Technical Targets
- **Primary Platform:** WebGL (cross-browser: Chrome, Firefox, Safari, Edge)
- **Orientation:** Portrait only
- **Target FPS:** 60 FPS
- **Responsive UI:** Anchored positions, safe area support for notch devices
- **Build Size:** Optimized for fast initial load

---

## 4. Core Gameplay Loop

```
┌─────────────────────────────────────────────────────────────┐
│                      GAME START                              │
│                  (or Resume from Save)                       │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                   DEAL PHASE                                 │
│  - Cards deal from deck to 4x4 grid (one by one, ~3 sec)    │
│  - Input blocked during animation                            │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────┐
│                   PLAY PHASE                                 │
│  - Player taps card to flip (face up)                       │
│  - Can tap open card to cancel (close it)                   │
│  - Single open card auto-closes after 10 sec (neutral)      │
└─────────────────────┬───────────────────────────────────────┘
                      │
          ┌───────────┴───────────┐
          │ Second card selected  │
          └───────────┬───────────┘
                      │
        ┌─────────────┴─────────────┐
        │                           │
        ▼                           ▼
┌───────────────┐           ┌───────────────┐
│    MATCH      │           │   NO MATCH    │
├───────────────┤           ├───────────────┤
│ +1 base point │           │ Strike → 0    │
│ +N strike pts │           │ Fail counter++│
│ Strike++      │           │ Cards shown   │
│ Fail ctr → 0  │           │ 2 sec, then   │
│ Cards → deck  │           │ flip back     │
│ (animation)   │           │               │
└───────┬───────┘           └───────┬───────┘
        │                           │
        │    ┌──────────────────────┘
        │    │
        │    ▼
        │   ┌─────────────────────────────────────┐
        │   │ Check Penalty (4th/6th/8th fail)    │
        │   │ 4th: -1 pt | 6th: -2 pt | 8th: -3 pt│
        │   └─────────────────────────────────────┘
        │                           │
        └───────────┬───────────────┘
                    │
                    ▼
        ┌───────────────────────┐
        │ All pairs matched?    │
        └───────────┬───────────┘
                    │
          ┌────────┴────────┐
          │ NO              │ YES
          ▼                 ▼
    [Back to PLAY]    ┌─────────────────┐
                      │   WIN PANEL     │
                      │ - Current Score │
                      │ - Best Score    │
                      │ - Max Strike    │
                      │ - New Game btn  │
                      └────────┬────────┘
                               │
                               ▼
                      [New Game → DEAL PHASE]
```

---

## 5. Game Mechanics

### 5.1 Card Dealing

| Property | Value |
|----------|-------|
| Total cards | 16 (8 pairs) |
| Grid layout | 4x4 |
| Deal direction | Deck → Grid positions (sequential) |
| Deal duration | ~3 seconds total |
| Deal animation | Cards move one by one from deck to grid position |
| Initial state | All cards face-down |
| Input during deal | **Blocked** |

**Card Shuffle:** Cards are randomly assigned to grid positions at deal start. Shuffle happens before animation begins.

### 5.2 Card Flipping

| Property | Value |
|----------|-------|
| Animation type | 3D flip (Y-axis rotation) |
| Flip duration | ~0.3 seconds (tunable) |
| Flip sound | Single SFX for both open and close |

**Player Actions:**
- **Tap face-down card:** Flip to face-up
- **Tap face-up card (own selection):** Flip back to face-down (cancel current attempt)
- **Tap while two cards revealed:** Ignored (input locked)

### 5.3 Matching Logic

| Scenario | Result |
|----------|--------|
| Two cards match | Both cards animate back to deck, removed from play |
| Two cards don't match | Cards remain face-up for 2 seconds, then flip back |
| Single card open 10+ sec | Auto-closes (neutral — no penalty, no strike reset) |
| Player closes own card | Cancel — no penalty, no strike reset |

### 5.4 Scoring System

**Base Scoring:**
- Each successful match: **+1 point**

**Strike Bonus:**
- Consecutive matches without failure add strike bonus
- Strike bonus = current strike count
- Strike increments AFTER each successful match

| Match # (consecutive) | Base | Strike Bonus | Total Points | Running Total |
|-----------------------|------|--------------|--------------|---------------|
| 1st | 1 | 0 | 1 | 1 |
| 2nd | 1 | 2 | 3 | 4 |
| 3rd | 1 | 3 | 4 | 8 |
| 4th | 1 | 4 | 5 | 13 |
| 5th | 1 | 5 | 6 | 19 |
| 6th | 1 | 6 | 7 | 26 |
| 7th | 1 | 7 | 8 | 34 |
| 8th | 1 | 8 | 9 | 43 |

**Maximum possible score (perfect game):** 43 points

**Strike Reset:** Any failed match resets strike counter to 0.

### 5.5 Penalty System

Consecutive failed matches (without a successful match in between) trigger penalties:

| Consecutive Fails | Penalty |
|-------------------|---------|
| 4th | -1 point |
| 6th | -2 points |
| 8th | -3 points |

**Rules:**
- Score cannot go below 0
- Successful match resets fail counter to 0
- Cancel/timeout do NOT increment fail counter
- Penalties continue pattern if player keeps failing (theoretical 10th, 12th, etc. — unlikely in 8-pair game)

### 5.6 Win Condition

- All 8 pairs successfully matched
- Win panel displays immediately upon final match animation completing

---

## 6. Game Systems

### 6.1 Card System

**Purpose:** Manages individual card state and visuals

**Card States:**
```
┌──────────┐      tap       ┌──────────┐
│ FaceDown │ ─────────────► │ FaceUp   │
└──────────┘                └──────────┘
     ▲                           │
     │         timeout/          │ matched
     │         cancel/           │
     │         no-match          ▼
     │                      ┌──────────┐
     └───────────────────── │ Matched  │ ──► (removed)
                            └──────────┘
```

**Data Requirements:**
- Card ID (for pairing)
- Card type/image index (0-7 for 8 unique types)
- Current state (FaceDown, FaceUp, Matched)
- Grid position (row, column)

### 6.2 Match System

**Purpose:** Handles match detection and scoring logic

**Inputs:**
- First selected card
- Second selected card

**Outputs:**
- Match result (success/failure)
- Score delta
- Strike delta
- Penalty (if applicable)

**State:**
- Current score
- Current strike count
- Consecutive fail count
- Max strike achieved (for win screen)

### 6.3 Game Flow System

**Purpose:** Controls overall game state and phase transitions

**States:**
```
┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐
│ Loading │ ──► │ Dealing │ ──► │ Playing │ ──► │   Win   │
└─────────┘     └─────────┘     └─────────┘     └─────────┘
                                     │               │
                                     ▼               │
                                ┌─────────┐          │
                                │ Paused  │          │
                                └─────────┘          │
                                     │               │
                                     └───────────────┘
                                            │
                                            ▼
                                      [New Game]
```

**Pause Behavior:**
- Triggered by Settings panel open
- Cards become non-interactive
- Does NOT use Time.timeScale (animations can still complete)

### 6.4 Save System

**Purpose:** Persist game state and high scores

**Persisted Data (PlayerPrefs):**

| Key | Type | Description |
|-----|------|-------------|
| `BestScore` | int | Highest score achieved |
| `GameState` | string (JSON) | Current game state for resume |

**GameState JSON Structure:**
```json
{
  "cardPositions": [
    {"gridX": 0, "gridY": 0, "typeId": 3, "isMatched": false},
    ...
  ],
  "currentScore": 12,
  "strikeCount": 2,
  "failCount": 1,
  "maxStrike": 3
}
```

**Save Triggers:**
- After each match attempt (success or failure)
- On application pause/quit

**Load Behavior:**
- On game start, check for saved GameState
- If exists and valid: resume from saved state (skip dealing, restore card states)
- If not exists: start new game with dealing animation

**Clear Behavior:**
- New Game after win: clear GameState, keep BestScore
- Reset button: clear GameState, start new game

### 6.5 Audio System

**Purpose:** Manage music and sound effects with volume control

**Audio Assets:**

| ID | Type | Trigger | Loop |
|----|------|---------|------|
| `bgm_main` | Music | Game start | Yes |
| `sfx_flip` | SFX | Card flip (open or close) | No |
| `sfx_match` | SFX | Successful match | No |
| `sfx_strike` | SFX | Match with active strike (>0) | No |
| `sfx_penalty` | SFX | Point deduction | No |
| `sfx_win` | SFX | Win condition met | No |

**Volume Control:**
- Music volume: 0-100% slider
- SFX volume: 0-100% slider
- Persisted in PlayerPrefs

**Playback Rules:**
- `sfx_strike` plays IN ADDITION to `sfx_match` when strike > 0
- Music continues through win panel
- Music pauses when browser tab loses focus (WebGL behavior)

### 6.6 Animation System

**Purpose:** Handle all visual animations via tweening

**Animations:**

| Animation | Duration | Easing | Description |
|-----------|----------|--------|-------------|
| Card Deal | 0.15s per card | EaseOutQuad | Card moves from deck to grid position |
| Card Flip | 0.3s | EaseInOutSine | Y-axis 3D rotation (0° → 90° → 180°) |
| Card to Deck | 0.4s | EaseInQuad | Matched cards return to deck |
| Strike Pulse | 0.2s | EaseOutElastic | Scale up then back on strike counter |
| Score Flash | 0.3s | Linear | Red tint on penalty |
| Score Color | 0.2s | Linear | Color shift on strike |
| Deck Shuffle | 0.5s | - | Visual shuffle effect on reset |

**Animation Library:** LitMotion (per project setup)

---

## 7. UI/UX Flow

### 7.1 Screen Inventory

| Screen | Type | Description |
|--------|------|-------------|
| Game Screen | Main | Only gameplay screen |
| Settings Panel | Overlay | Music/SFX sliders + Reset |
| Reset Confirm | Popup | "Emin misiniz?" + Evet/Hayır |
| Win Panel | Overlay | Score summary + New Game |

### 7.2 Game Screen Layout

```
┌─────────────────────────────────────────┐
│            [Safe Area Top]              │
├─────────────────────────────────────────┤
│                                    [⚙]  │  ← Settings button (top-right)
│                                         │
│              ┌─────────┐                │
│              │  SCORE  │                │  ← Score (top-center)
│              │   24    │                │
│              └─────────┘                │
│              ┌─────────┐                │
│              │ 🔥 x3   │                │  ← Strike counter (below score)
│              └─────────┘                │
│                                         │
│   ┌───┐   ┌───┬───┬───┬───┐            │
│   │   │   │ ? │ ? │ ? │ ? │            │
│   │ D │   ├───┼───┼───┼───┤            │  ← Deck (left) + Grid (center-right)
│   │ E │   │ ? │ ? │ ? │ ? │            │
│   │ C │   ├───┼───┼───┼───┤            │
│   │ K │   │ ? │ ? │ ? │ ? │            │
│   │   │   ├───┼───┼───┼───┤            │
│   └───┘   │ ? │ ? │ ? │ ? │            │
│           └───┴───┴───┴───┘            │
│                                         │
│            [Safe Area Bottom]           │
└─────────────────────────────────────────┘
```

**Layout Rules:**
- All UI uses RectTransform with anchors (responsive)
- Safe area margins for notch/home indicator
- Deck offset from left edge (not touching)
- Grid centered horizontally with deck offset considered
- Score/Strike centered at top

### 7.3 Settings Panel

```
┌─────────────────────────────────┐
│           SETTINGS              │
├─────────────────────────────────┤
│                                 │
│  Music      ────●────────       │  ← Slider
│                                 │
│  SFX        ──────────●──       │  ← Slider
│                                 │
│         [ RESET ]               │  ← Reset button
│                                 │
│           [ X ]                 │  ← Close button
└─────────────────────────────────┘
```

**Behavior:**
- Opens as centered popup with semi-transparent background
- Game pauses (cards non-interactive) but animations complete
- Close via X button or tap outside

### 7.4 Reset Confirmation Popup

```
┌─────────────────────────────────┐
│                                 │
│       Emin misiniz?             │
│                                 │
│    [ EVET ]     [ HAYIR ]       │
│                                 │
└─────────────────────────────────┘
```

**Behavior:**
- EVET: Close settings, cards return to deck with shuffle animation, new deal
- HAYIR: Close popup, return to settings

### 7.5 Win Panel

```
┌─────────────────────────────────┐
│                                 │
│         🎉 KAZANDIN! 🎉         │
│                                 │
│     Skor:          24           │
│     En İyi:        43           │
│     Max Strike:     5           │
│                                 │
│        [ YENİ OYUN ]            │
│                                 │
└─────────────────────────────────┘
```

**Behavior:**
- Displays after final match animation completes
- Victory sound plays once
- Music continues
- New Game: clears game state, keeps best score, starts dealing

---

## 8. Art Direction

### 8.1 Visual Style
- **Style:** Clean, modern, casual
- **Color Palette:** Vibrant but not overwhelming (TBD by artist)
- **Card Design:** Placeholder initially, final art later

### 8.2 Asset Requirements

| Asset | Count | Format | Notes |
|-------|-------|--------|-------|
| Card Back | 1 | Sprite | Uniform back design |
| Card Faces | 8 | Sprite | Unique icons/images for each pair |
| Deck Visual | 1 | Sprite | Stacked cards appearance |
| Flame Icon | 1 | Sprite | For strike counter |
| Settings Icon | 1 | Sprite | Gear/cog |
| Background | 1 | Sprite | Game screen background |
| UI Panel | 1-2 | Sprite/9-slice | For popups |
| Button | 2-3 | Sprite | Normal/hover/pressed states |

### 8.3 Animation Specifications

| Element | Animation Type | Notes |
|---------|---------------|-------|
| Card Flip | 3D Y-rotation | Scale X: 1→0→1 with sprite swap at midpoint |
| Card Deal | Position tween | Deck → grid with slight arc |
| Card Match | Position tween | Grid → deck |
| Strike Pulse | Scale tween | 1.0 → 1.3 → 1.0 |
| Score Flash | Color tween | Normal → Red → Normal |

---

## 9. Audio Design

### 9.1 Music
- **Track Count:** 1
- **Style:** Casual, upbeat, loopable
- **Duration:** 60-120 seconds loop
- **Format:** MP3 or WAV

### 9.2 Sound Effects

| SFX | Description | Trigger |
|-----|-------------|---------|
| Card Flip | Soft card flip/whoosh | Any card flip (open or close) |
| Match | Positive chime/ding | Successful match |
| Strike | Energetic whoosh/fire | Match while on strike (plays with Match) |
| Penalty | Negative buzz/thud | Points deducted |
| Victory | Celebratory jingle | Win condition |

### 9.3 Implementation Notes
- All SFX should be short (<1 second)
- No spatial audio needed (2D game)
- Simultaneous SFX supported (match + strike)

---

## 10. Economy & Progression

### 10.1 Scoring Economy

| Action | Points |
|--------|--------|
| Match (base) | +1 |
| Strike bonus | +[current strike] |
| Penalty (4th fail) | -1 |
| Penalty (6th fail) | -2 |
| Penalty (8th fail) | -3 |

### 10.2 Progression
- **Single session:** Beat your high score
- **No unlocks:** All content available from start
- **No currency:** Pure score-chasing

### 10.3 Balance Levers (Tunable Parameters)

| Parameter | Default | Description |
|-----------|---------|-------------|
| `cardCount` | 16 | Total cards (must be even) |
| `pairCount` | 8 | Number of unique pairs |
| `dealDuration` | 3.0s | Total dealing animation time |
| `flipDuration` | 0.3s | Single card flip time |
| `noMatchRevealTime` | 2.0s | How long non-matching cards stay visible |
| `autoCloseTime` | 10.0s | Single open card auto-close timeout |
| `penaltyThresholds` | [4,6,8] | Consecutive fails for penalties |
| `penaltyAmounts` | [1,2,3] | Points deducted at each threshold |

---

## 11. Technical Requirements

### 11.1 Platform Targets
- **Primary:** WebGL
- **Browsers:** Chrome, Firefox, Safari, Edge (latest 2 versions)
- **Orientation:** Portrait locked

### 11.2 Performance Targets
- **Frame Rate:** 60 FPS stable
- **Initial Load:** < 5 seconds on average connection
- **Memory:** Minimize footprint for mobile browsers

### 11.3 Architectural Patterns
Per project CLAUDE.md:
- **MVS Pattern:** Model-View-System separation
- **VContainer:** Dependency injection
- **MessagePipe:** Event communication
- **UniTask:** Async operations
- **LitMotion:** Animations/tweening

### 11.4 Optimization Requirements
- **Sprite Atlas:** All card sprites in single atlas
- **Draw Calls:** Minimize via batching
- **Object Pooling:** Cards should be pooled (not instantiated/destroyed)
- **No runtime Instantiate:** Prefab references only

### 11.5 Third-Party Integrations
- None (portfolio project)

---

## 12. Content Scope (MVP)

### 12.1 Must Have (v1.0)
- [x] 4x4 grid with 8 pairs
- [x] Card dealing animation
- [x] Card flip animation (3D)
- [x] Match detection
- [x] Scoring with strike system
- [x] Penalty system
- [x] Win panel with stats
- [x] Settings panel (volume sliders + reset)
- [x] Save/resume functionality
- [x] Best score tracking
- [x] All audio (1 music + 5 SFX)
- [x] Responsive UI

### 12.2 Should Have (if time permits)
- [ ] Card face art (currently placeholder)
- [ ] Visual polish (particles, juice)

### 12.3 Won't Have (out of scope)
- Main menu
- Multiple difficulty levels
- Multiple game modes
- Leaderboards
- Achievements
- Monetization
- Analytics

---

## 13. Monetization

Not applicable — free portfolio project.

---

## 14. Accessibility

### 14.1 Implemented
- Volume controls for audio
- No time pressure (no countdown timer)
- Clear visual feedback for all actions

### 14.2 Not Implemented (Out of Scope)
- Colorblind modes
- Screen reader support
- Input remapping

---

## 15. Analytics & KPIs

Not implemented — portfolio project. Future consideration:
- Games played
- Average score
- Perfect game rate
- Session length

---

## 16. Glossary

| Term | Definition |
|------|------------|
| **Pair** | Two cards with matching face images |
| **Match** | Successfully revealing two cards of the same pair |
| **Strike** | Counter for consecutive successful matches |
| **Strike Bonus** | Additional points equal to current strike count |
| **Fail Counter** | Tracks consecutive failed match attempts |
| **Penalty** | Point deduction at 4th/6th/8th consecutive fails |
| **Deal** | Animation of cards moving from deck to grid |
| **Flip** | Animation of card rotating to reveal/hide face |
| **Cancel** | Player tapping open card to close it (neutral action) |
| **Auto-close** | Single open card closing after 10 second timeout |

---

## Appendix A: State Diagram — Match Phase

```
                    ┌──────────────────┐
                    │  IDLE            │
                    │  (0 cards open)  │
                    └────────┬─────────┘
                             │ tap card
                             ▼
                    ┌──────────────────┐
              ┌─────│  ONE_OPEN        │─────┐
              │     │  (1 card open)   │     │
              │     └────────┬─────────┘     │
              │              │               │
        tap same      tap different    10s timeout
        (cancel)           card          (auto-close)
              │              │               │
              │              ▼               │
              │     ┌──────────────────┐     │
              │     │  TWO_OPEN        │     │
              │     │  (checking...)   │     │
              │     └────────┬─────────┘     │
              │              │               │
              │    ┌─────────┴─────────┐     │
              │    │                   │     │
              │  match              no match │
              │    │                   │     │
              │    ▼                   ▼     │
              │ ┌────────┐      ┌──────────┐ │
              │ │MATCHED │      │ REVEALED │ │
              │ │→ deck  │      │ (2 sec)  │ │
              │ └───┬────┘      └────┬─────┘ │
              │     │                │       │
              │     │           flip back    │
              │     │                │       │
              └─────┴────────────────┴───────┘
                             │
                             ▼
                    [Back to IDLE]
```

---

## Appendix B: Score Calculation Examples

**Example 1: Perfect Game**
```
Match 1: 1 + 0 = 1  (strike becomes 1)
Match 2: 1 + 2 = 3  (strike becomes 2)
Match 3: 1 + 3 = 4  (strike becomes 3)
Match 4: 1 + 4 = 5  (strike becomes 4)
Match 5: 1 + 5 = 6  (strike becomes 5)
Match 6: 1 + 6 = 7  (strike becomes 6)
Match 7: 1 + 7 = 8  (strike becomes 7)
Match 8: 1 + 8 = 9  (strike becomes 8)
Total: 43 points
```

**Example 2: One Mistake Mid-Game**
```
Match 1: 1 + 0 = 1  (strike: 1)
Match 2: 1 + 2 = 3  (strike: 2)
Match 3: 1 + 3 = 4  (strike: 3)
FAIL    → strike: 0, fail: 1
Match 4: 1 + 0 = 1  (strike: 1)
Match 5: 1 + 2 = 3  (strike: 2)
Match 6: 1 + 3 = 4  (strike: 3)
Match 7: 1 + 4 = 5  (strike: 4)
Match 8: 1 + 5 = 6  (strike: 5)
Total: 27 points
```

**Example 3: Multiple Failures with Penalty**
```
Match 1: 1 + 0 = 1  (strike: 1)
FAIL 1  → strike: 0, fail: 1
FAIL 2  → fail: 2
FAIL 3  → fail: 3
FAIL 4  → fail: 4, PENALTY -1 (score: 0)
Match 2: 1 + 0 = 1  (strike: 1, fail: 0)
... continues
```

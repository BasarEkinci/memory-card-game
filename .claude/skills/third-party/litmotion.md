# LitMotion — Zero Allocation Tween Library

## Overview

LitMotion is a high-performance, zero-allocation tween library for Unity. It uses DOTS (Data-Oriented Technology Stack) for extremely fast tween creation and updates — 2-20x faster than DOTween with zero GC allocation.

**Requirements:** Unity 2021.3+, Burst 1.6.0+, Collections 1.5.1+, Mathematics 1.0.0+

## Installation

```json
// Packages/manifest.json
{
    "dependencies": {
        "com.annulusgames.lit-motion": "https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion"
    }
}
```

## Core API

### Basic Motion Creation

```csharp
using LitMotion;
using LitMotion.Extensions;

// Basic value tween
LMotion.Create(0f, 10f, 2f)           // from, to, duration
    .Bind(x => value = x);             // bind to variable

// Transform position
LMotion.Create(Vector3.zero, Vector3.one, 1f)
    .BindToPosition(transform);

// Single axis
LMotion.Create(0f, 5f, 1f)
    .BindToPositionX(transform);

// Color
LMotion.Create(Color.white, Color.red, 0.5f)
    .BindToColor(spriteRenderer);
```

### Supported Types

- Primitives: `float`, `double`, `int`, `long`
- Unity: `Vector2`, `Vector3`, `Vector4`, `Quaternion`, `Color`, `Rect`
- Strings: `FixedString32Bytes` through `FixedString4096Bytes` (zero-alloc text)

## Configuration (Builder Pattern)

```csharp
LMotion.Create(0f, 1f, 2f)
    .WithEase(Ease.OutQuad)              // Easing function
    .WithDelay(0.5f)                      // Start delay
    .WithLoops(3, LoopType.Yoyo)          // Loop count and type
    .WithScheduler(MotionScheduler.FixedUpdate)  // Update timing
    .WithOnComplete(() => Debug.Log("Done"))     // Completion callback
    .WithOnCancel(() => Debug.Log("Cancelled"))  // Cancel callback
    .WithCancelOnError()                  // Cancel if bind throws
    .Bind(x => value = x);
```

### Easing Functions

```csharp
Ease.Linear
Ease.InSine, Ease.OutSine, Ease.InOutSine
Ease.InQuad, Ease.OutQuad, Ease.InOutQuad
Ease.InCubic, Ease.OutCubic, Ease.InOutCubic
Ease.InQuart, Ease.OutQuart, Ease.InOutQuart
Ease.InQuint, Ease.OutQuint, Ease.InOutQuint
Ease.InExpo, Ease.OutExpo, Ease.InOutExpo
Ease.InCirc, Ease.OutCirc, Ease.InOutCirc
Ease.InElastic, Ease.OutElastic, Ease.InOutElastic
Ease.InBack, Ease.OutBack, Ease.InOutBack
Ease.InBounce, Ease.OutBounce, Ease.InOutBounce

// Custom curve
.WithEase(animationCurve)
```

### Loop Types

```csharp
LoopType.Restart   // Reset to start value each loop
LoopType.Yoyo      // Ping-pong between start and end
LoopType.Increment // Add to end value each loop
```

### Delay Types

```csharp
.WithDelay(0.5f, DelayType.FirstLoop)  // Delay only on first loop (default)
.WithDelay(0.5f, DelayType.EveryLoop)  // Delay on every loop
```

## Motion Control (MotionHandle)

```csharp
// Get handle when creating motion
MotionHandle handle = LMotion.Create(0f, 1f, 2f)
    .BindToPositionX(transform);

// Check state
if (handle.IsActive()) { }
if (handle.IsPlaying()) { }

// Control
handle.Complete();        // Jump to end, trigger OnComplete
handle.Cancel();          // Stop immediately, trigger OnCancel
handle.TryComplete();     // Returns false if already inactive
handle.TryCancel();

// Properties
handle.PlaybackSpeed = 0.5f;  // Slow motion
handle.PlaybackSpeed = 0f;    // Pause
handle.PlaybackSpeed = -1f;   // Reverse
double time = handle.Time;
float duration = handle.Duration;
int loops = handle.CompletedLoops;

// Link to GameObject (auto-cancel on destroy)
handle.AddTo(gameObject);
handle.AddTo(this);  // MonoBehaviour

// Convert to IDisposable
using var disposable = handle.ToDisposable();
```

## Extension Methods (LitMotion.Extensions)

### Transform

```csharp
.BindToPosition(transform)           // Vector3 → position
.BindToPositionX/Y/Z(transform)      // float → position.x/y/z
.BindToPositionXY/XZ/YZ(transform)   // Vector2 → position axes
.BindToLocalPosition(transform)      // Vector3 → localPosition
.BindToLocalPositionX/Y/Z(transform)
.BindToRotation(transform)           // Quaternion → rotation
.BindToLocalRotation(transform)
.BindToEulerAngles(transform)        // Vector3 → eulerAngles
.BindToEulerAnglesX/Y/Z(transform)
.BindToLocalEulerAngles(transform)
.BindToLocalScale(transform)         // Vector3 → localScale
.BindToLocalScaleX/Y/Z(transform)
.BindToLocalScaleXYZ(transform)      // float → uniform scale
```

### RectTransform (UI)

```csharp
.BindToAnchoredPosition(rectTransform)      // Vector2
.BindToAnchoredPositionX/Y(rectTransform)   // float
.BindToAnchoredPosition3D(rectTransform)    // Vector3
.BindToSizeDelta(rectTransform)             // Vector2
.BindToSizeDeltaX/Y(rectTransform)          // float
.BindToPivot(rectTransform)                 // Vector2
.BindToAnchorMin/Max(rectTransform)         // Vector2
```

### uGUI Components

```csharp
.BindToColor(graphic)          // Image, Text, etc.
.BindToColorA(graphic)         // Alpha only
.BindToFillAmount(image)       // Image.fillAmount
.BindToAlpha(canvasGroup)      // CanvasGroup.alpha
.BindToFontSize(text)          // Text.fontSize (int)
.BindToText(text)              // Text.text (FixedString)
```

### SpriteRenderer

```csharp
.BindToColor(spriteRenderer)
.BindToColorA(spriteRenderer)
```

### Material

```csharp
.BindToColor(material)
.BindToFloat(material, propertyId)
```

## Punch & Shake

```csharp
// Punch — damped oscillation returning to start
LMotion.Punch.Create(0f, 5f, 0.5f)      // startValue, strength, duration
    .WithFrequency(10)                   // Oscillation count
    .WithDampingRatio(1f)                // 0 = no damping, 1 = critical
    .BindToPositionX(transform);

LMotion.Punch.Create(Vector3.zero, Vector3.up * 30f, 0.5f)
    .BindToPosition(transform);

// Shake — random oscillation
LMotion.Shake.Create(0f, 5f, 0.5f)
    .WithFrequency(10)
    .WithDampingRatio(1f)
    .WithRandomSeed(42)                  // Reproducible shake
    .BindToPositionX(transform);
```

## Sequences

```csharp
// Combine multiple motions
LSequence.Create()
    .Append(LMotion.Create(0f, 1f, 1f).BindToPositionX(transform))  // First
    .Append(LMotion.Create(0f, 1f, 1f).BindToPositionY(transform))  // After first
    .Join(LMotion.Create(0f, 1f, 1f).BindToPositionZ(transform))    // Parallel with previous
    .Insert(0.5f, LMotion.Create(0f, 1f, 0.5f).BindToLocalScaleXYZ(transform))  // At specific time
    .AppendInterval(0.5f)                                            // Wait
    .Run();                                                          // Start sequence

// Configure the sequence motion
LSequence.Create()
    .Append(...)
    .Run(builder => builder.WithLoops(2).WithOnComplete(() => {}));
```

## Schedulers (Update Timing)

```csharp
MotionScheduler.Update                // Default, uses Time.deltaTime
MotionScheduler.FixedUpdate           // Physics, uses Time.fixedDeltaTime
MotionScheduler.LateUpdate            // After all Updates
MotionScheduler.UpdateIgnoreTimeScale // Unaffected by Time.timeScale
MotionScheduler.Manual                // Manual update via ManualMotionDispatcher

// Change default
MotionScheduler.DefaultScheduler = MotionScheduler.FixedUpdate;

// Use specific scheduler
.WithScheduler(MotionScheduler.FixedUpdate)
```

## Zero-Allocation Patterns

### Avoid Closure Allocations

```csharp
// BAD — closure allocates
LMotion.Create(0f, 1f, 1f)
    .Bind(x => myObject.Value = x);  // Captures myObject

// GOOD — pass state explicitly
LMotion.Create(0f, 1f, 1f)
    .Bind(myObject, static (x, obj) => obj.Value = x);  // No closure
```

### Pre-allocate Storage

```csharp
// At startup, reserve capacity for expected motion counts
MotionDispatcher.EnsureStorageCapacity<float, NoOptions, FloatMotionAdapter>(500);
MotionDispatcher.EnsureStorageCapacity<Vector3, NoOptions, Vector3MotionAdapter>(200);
```

### Zero-Allocation Text Animation

```csharp
// Use FixedString variants (no string allocation)
LMotion.String.Create128Bytes("", "Hello World!", 2f)
    .WithRichText()                          // Support <color>, <b>, etc.
    .WithScrambleChars(ScrambleMode.All)     // Fill with random chars
    .BindToText(tmpText);
```

## Async/Await Integration

### Native C# (ValueTask/Awaitable)

```csharp
// Direct await
await LMotion.Create(0f, 1f, 1f).BindToPositionX(transform);

// With cancellation
var cts = new CancellationTokenSource();
await handle.ToValueTask(cts.Token);

// Unity 2023.1+ Awaitable
await handle.ToAwaitable(cancellationToken);
```

### UniTask Integration

```csharp
// Auto-enabled if UniTask is installed via Package Manager
// Otherwise add LITMOTION_SUPPORT_UNITASK to Scripting Define Symbols

await LMotion.Create(0f, 1f, 1f)
    .BindToPositionX(transform)
    .ToUniTask(cancellationToken);
```

### Coroutine

```csharp
yield return handle.ToYieldInstruction();
```

## DOTween Migration Cheatsheet

| DOTween | LitMotion |
|---------|-----------|
| `transform.DOMove(end, dur)` | `LMotion.Create(start, end, dur).BindToPosition(transform)` |
| `transform.DOMoveX(end, dur)` | `LMotion.Create(start, end, dur).BindToPositionX(transform)` |
| `DOTween.To(() => v, x => v = x, end, dur)` | `LMotion.Create(v, end, dur).Bind(x => v = x)` |
| `.SetEase(Ease.OutQuad)` | `.WithEase(Ease.OutQuad)` |
| `.SetLoops(2, LoopType.Yoyo)` | `.WithLoops(2, LoopType.Yoyo)` |
| `.SetDelay(0.5f)` | `.WithDelay(0.5f)` |
| `.SetUpdate(UpdateType.Fixed)` | `.WithScheduler(MotionScheduler.FixedUpdate)` |
| `.SetLink(gameObject)` | `.AddTo(gameObject)` (on handle) |
| `tween.Kill()` | `handle.Cancel()` |
| `tween.Complete()` | `handle.Complete()` |
| `tween.Pause()` | `handle.PlaybackSpeed = 0f` |
| `DOTween.Sequence()` | `LSequence.Create()` |
| `.Append()/.Join()/.Insert()` | Same methods |
| `yield return tween.WaitForCompletion()` | `yield return handle.ToYieldInstruction()` |
| `await tween.AsyncWaitForCompletion()` | `await handle` or `await handle.ToUniTask()` |

## MVS Architecture Integration

### In View Layer (MonoBehaviour)

```csharp
public sealed class CardView : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;
    
    private MotionHandle _flipHandle;
    private readonly CompositeDisposable _disposables = new();
    
    public void PlayFlipAnimation(Action onMidpoint)
    {
        _flipHandle.TryCancel();
        
        _flipHandle = LSequence.Create()
            .Append(LMotion.Create(0f, 90f, 0.15f)
                .WithEase(Ease.InQuad)
                .WithOnComplete(onMidpoint)
                .BindToLocalEulerAnglesY(_rectTransform))
            .Append(LMotion.Create(90f, 0f, 0.15f)
                .WithEase(Ease.OutQuad)
                .BindToLocalEulerAnglesY(_rectTransform))
            .Run();
        
        _flipHandle.AddTo(this);
    }
    
    public void PlayPunchScale()
    {
        LMotion.Punch.Create(Vector3.one, Vector3.one * 0.2f, 0.3f)
            .WithFrequency(6)
            .BindToLocalScale(_rectTransform)
            .AddTo(this);
    }
    
    private void OnDestroy()
    {
        _flipHandle.TryCancel();
    }
}
```

### Reusable Animation Settings (ScriptableObject)

```csharp
[CreateAssetMenu(menuName = "Config/Card Animation Settings")]
public sealed class CardAnimationSettings : ScriptableObject
{
    [field: SerializeField] public float FlipDuration { get; private set; } = 0.3f;
    [field: SerializeField] public Ease FlipEaseIn { get; private set; } = Ease.InQuad;
    [field: SerializeField] public Ease FlipEaseOut { get; private set; } = Ease.OutQuad;
    [field: SerializeField] public float PunchStrength { get; private set; } = 0.2f;
    [field: SerializeField] public int PunchFrequency { get; private set; } = 6;
}
```

### Cancel Previous Animations Pattern

```csharp
private MotionHandle _currentMotion;

public void Animate()
{
    _currentMotion.TryCancel();  // Safe — returns false if already inactive
    _currentMotion = LMotion.Create(...)
        .BindTo...
        .AddTo(this);
}
```

## Debugging

```csharp
// Enable debugger (development builds only)
MotionDebugger.Enabled = true;

// Name motions for debugger
.WithDebugName("CardFlip")

// Open debugger window: Window > LitMotion Debugger

// Handle exceptions
MotionDispatcher.RegisterUnhandledExceptionHandler(ex => Debug.LogWarning(ex));
```

## Key Differences from DOTween

1. **No From()** — Always specify start and end in `Create(from, to, duration)`
2. **No SetSpeedBased()** — Calculate duration based on distance manually
3. **No DoPath()** — Use Unity Splines package for path animation
4. **No DelayedCall()** — Use UniTask.Delay or coroutines
5. **Sequence callbacks** — Not supported; use async/await for complex flows
6. **Pause** — Use `PlaybackSpeed = 0f` instead of dedicated Pause method

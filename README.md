# Unity Optimization Patterns

> Production-ready Unity design patterns and optimization techniques — Singleton, Delegate, Object Pooling, Networking, Post-Processing, and more.

![Unity](https://img.shields.io/badge/Unity-2021.3+-blue?logo=unity)
![C#](https://img.shields.io/badge/Language-C%23-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

---

## Overview

A collection of **battle-tested Unity design patterns** and optimization techniques extracted from real game development projects. Each module is self-contained and can be integrated independently into any Unity project.

---

## Patterns

### Singleton

Thread-safe Singleton pattern for Unity MonoBehaviour managers.

- **DontDestroyOnLoad** persistent managers
- **Lazy initialization** with null checking
- Scene transition safety
- Use case: GameManager, AudioManager, UIManager

### Delegate & Event System

Decoupled communication between game systems using C# delegates and events.

- **Action/Func delegates** for flexible callbacks
- **Event-driven architecture** reducing direct dependencies
- Publisher-Subscriber pattern
- Use case: UI updates, game state changes, achievement triggers

### ScriptableObject

Data-driven design using Unity ScriptableObjects.

- **Runtime data containers** without MonoBehaviour overhead
- Shared configuration across scenes
- Inspector-friendly data editing
- Use case: Item databases, character stats, game configuration

### Ballistic (Projectile Physics)

Physics-based projectile trajectory calculation and simulation.

- **Parabolic trajectory** computation
- Gravity-affected projectile motion
- Launch angle and velocity calculation
- Use case: Artillery, throwing mechanics, trajectory preview

### Photon Networking (PUN)

Multiplayer networking patterns using Photon PUN.

- **Room management** and matchmaking
- State synchronization strategies
- RPC (Remote Procedure Call) patterns
- Use case: Real-time multiplayer games, lobby systems

### Post-Processing

Visual enhancement pipeline and rendering optimization.

- **URP/Built-in** post-processing setup
- Bloom, color grading, vignette configuration
- Performance-optimized effect stacking
- Use case: Visual polish, mood setting, screenshot mode

### Utility Scripts

Reusable helper classes and extension methods.

- **Object pooling** for performance optimization
- Collection extensions and helper methods
- Common math utilities for game development
- Use case: Frequently spawned objects (bullets, effects, particles)

---

## Tech Stack

| Component | Details |
|-----------|---------|
| **Engine** | Unity 2021.3+ |
| **Language** | C# |
| **Networking** | Photon PUN2 |
| **Rendering** | URP / Built-in Pipeline |
| **Physics** | Unity 2D/3D Physics |

---

## Usage

Each pattern is organized in its own directory. To use a pattern:

1. Copy the desired pattern folder into your Unity project's `Assets/Scripts/`
2. Adjust namespaces if needed
3. Follow the comments in each script for integration guidance

### Example: Singleton

```csharp
public class GameManager : Singleton<GameManager>
{
    // Access anywhere: GameManager.Instance.DoSomething()
    public void DoSomething() { }
}
```

### Example: Object Pool

```csharp
// Pre-warm pool
ObjectPool.Instance.CreatePool(bulletPrefab, 50);

// Get from pool (instead of Instantiate)
var bullet = ObjectPool.Instance.GetObject(bulletPrefab);

// Return to pool (instead of Destroy)
ObjectPool.Instance.ReturnObject(bullet);
```

---

## Why These Patterns?

| Pattern | Problem Solved |
|---------|---------------|
| **Singleton** | Global access without Find() calls; persistent across scenes |
| **Delegate/Event** | Eliminates tight coupling between systems |
| **ScriptableObject** | Separates data from logic; designer-friendly editing |
| **Object Pool** | Eliminates GC spikes from frequent Instantiate/Destroy |
| **Ballistic** | Accurate physics simulation without per-frame raycasts |
| **Photon** | Reliable real-time networking with minimal boilerplate |
| **Post-Processing** | Visual quality without performance degradation |

---

## License

MIT License

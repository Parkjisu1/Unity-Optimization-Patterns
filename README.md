# Unity Optimization Patterns | Unity 최적화 패턴

> 실전에서 검증된 Unity 디자인 패턴과 최적화 기법 모음 — Singleton, Delegate, Object Pooling, 네트워킹, 포스트 프로세싱 등
>
> Production-ready Unity design patterns and optimization techniques — Singleton, Delegate, Object Pooling, Networking, Post-Processing, and more.

![Unity](https://img.shields.io/badge/Unity-2021.3+-blue?logo=unity)
![C#](https://img.shields.io/badge/Language-C%23-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

---

## 개요 | Overview

실제 게임 개발 프로젝트에서 추출한 **실전 검증 Unity 디자인 패턴** 및 최적화 기법 모음입니다. 각 모듈은 독립적으로 구성되어 있어 어떤 Unity 프로젝트에도 개별 통합이 가능합니다.

A collection of **battle-tested Unity design patterns** and optimization techniques extracted from real game development projects. Each module is self-contained and can be integrated independently into any Unity project.

---

## 프로젝트 구조 | Project Structure

```
Unity-Optimization-Patterns/
├── README.md
├── Singleton/
│   ├── Singleton.cs            # Generic Singleton<T> base class
│   └── README.md
├── Delegate/
│   ├── DelegateExample.cs      # Delegate, Action, Func examples
│   └── README.md
├── Ballistic/
│   ├── FormulaManager.cs       # Projectile trajectory + wind simulation
│   └── README.md
├── Photon/
│   ├── PunManager.cs           # Photon PUN networking manager
│   └── README.md
├── PostProcess/
│   └── README.md               # Post-processing setup guide
├── ScriptableObject/
│   ├── ItemData.cs             # ScriptableObject example
│   └── README.md
├── SnowFight/
│   ├── SnowFightController.cs  # Multiplayer snowball fight mini-game
│   └── README.md
├── Util/
│   ├── MathHelper.cs           # Angle calculation, ref keyword
│   ├── PlayerMovement.cs       # Smooth keyboard movement
│   └── README.md
└── Portfolio/
    ├── README.md               # AOS game portfolio overview
    └── Scripts/
        ├── GameManager.cs
        ├── UIManager.cs
        ├── PunManager.cs
        ├── CameraManager.cs
        ├── CameraManager_CineCam.cs
        ├── MapManager.cs
        ├── ShopManager.cs
        ├── Player/
        │   ├── NPlayer.cs
        │   ├── NPlayerMove.cs
        │   └── NPlayerRPC.cs
        ├── Data/
        │   ├── DataTableManager.cs
        │   ├── ItemInfo.cs
        │   ├── MonsterInfo.cs
        │   ├── PlayerInfo.cs
        │   └── EnumDataList.cs
        └── Effects/
            └── DissolveShader.shader
```

---

## 패턴 목록 | Patterns

### [Singleton](./Singleton/) | 싱글톤

Unity MonoBehaviour 매니저를 위한 제네릭 싱글톤 패턴. 지연 초기화 및 전역 접근 지원.

Generic Singleton base class for Unity MonoBehaviour managers with lazy initialization.

```csharp
public class GameManager : Singleton<GameManager>
{
    public void DoSomething() { }
}
```

### [Delegate & Event](./Delegate/) | 델리게이트 & 이벤트

C# 델리게이트와 Action/Func를 활용한 시스템 간 비결합 통신.

Decoupled inter-system communication using C# Delegates, Action, and Func.

### [Ballistic](./Ballistic/) | 탄도 물리

물리 기반 투사체 궤도 계산 및 바람 저항 시뮬레이션.

Physics-based projectile trajectory calculation with wind resistance simulation.

### [Photon Networking](./Photon/) | 포톤 네트워킹

Photon PUN 기반 멀티플레이어 네트워킹 — 룸 관리, RPC 통신, UDP/TCP 비교.

Photon PUN multiplayer networking — room management, RPC communication, protocol comparison.

### [Post-Processing](./PostProcess/) | 포스트 프로세싱

URP/Built-in 파이프라인 포스트 프로세싱 설정 가이드.

Post-processing setup guide for URP and Built-in render pipelines.

### [ScriptableObject](./ScriptableObject/) | 스크립터블 오브젝트

Unity ScriptableObject를 활용한 데이터 주도 설계.

Data-driven design using Unity ScriptableObjects as lightweight data containers.

### [SnowFight](./SnowFight/) | 눈싸움 미니게임

Photon PUN 기반 실시간 멀티플레이어 눈싸움 배틀로얄 미니게임.

Real-time multiplayer snowball fight battle royale mini-game system.

### [Utility](./Util/) | 유틸리티

재사용 가능한 수학 헬퍼 및 플레이어 이동 클래스.

Reusable math helpers and smooth player movement implementation.

### [Portfolio](./Portfolio/) | AOS 게임 포트폴리오

AOS류 게임 전체 구현 — 플레이어/몬스터/NPC 시스템, UI, 데이터 관리, Photon 네트워킹.

Complete AOS-style game implementation — player/monster/NPC systems, UI, data management, Photon networking.

---

## 왜 이 패턴인가? | Why These Patterns?

| 패턴 | 해결하는 문제 |
|------|---------------|
| **Singleton** | Find() 호출 없이 전역 접근; 씬 간 영속성 |
| **Delegate/Event** | 시스템 간 강결합 제거 |
| **ScriptableObject** | 데이터와 로직 분리; 기획자 친화적 편집 |
| **Object Pool** | Instantiate/Destroy로 인한 GC 스파이크 제거 |
| **Ballistic** | 프레임별 Raycast 없이 정확한 물리 시뮬레이션 |
| **Photon** | 최소한의 보일러플레이트로 안정적 실시간 네트워킹 |
| **Post-Processing** | 성능 저하 없는 비주얼 품질 향상 |

---

## 기술 스택 | Tech Stack

| 구성요소 | 상세 |
|----------|------|
| **엔진** | Unity 2021.3+ |
| **언어** | C# |
| **네트워킹** | Photon PUN2 |
| **렌더링** | URP / Built-in Pipeline |
| **물리** | Unity 2D/3D Physics |

---

## 사용법 | Usage

각 패턴은 개별 디렉토리에 정리되어 있습니다:

1. 원하는 패턴 폴더를 Unity 프로젝트의 `Assets/Scripts/`에 복사
2. 필요시 네임스페이스 조정
3. 각 스크립트의 주석 및 README를 참고하여 통합

---

## 라이선스 | License

MIT License

# Unity 최적화 패턴 | Unity Optimization Patterns

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

## 패턴 목록 | Patterns

### Singleton | 싱글톤

Unity MonoBehaviour 매니저를 위한 스레드 안전 싱글톤 패턴.

- **DontDestroyOnLoad** — 씬 전환에도 유지되는 영속 매니저
- **지연 초기화** — null 체크 기반 Lazy initialization
- 씬 전환 안전성 보장
- 적용: GameManager, AudioManager, UIManager

```csharp
public class GameManager : Singleton<GameManager>
{
    // GameManager.Instance.DoSomething() 으로 어디서든 접근
    public void DoSomething() { }
}
```

### Delegate & Event | 델리게이트 & 이벤트

C# 델리게이트와 이벤트를 활용한 시스템 간 비결합 통신.

- **Action/Func 델리게이트** — 유연한 콜백 처리
- **이벤트 기반 아키텍처** — 직접 의존성 최소화
- Publisher-Subscriber 패턴 구현
- 적용: UI 업데이트, 게임 상태 변경, 업적 트리거

### ScriptableObject | 스크립터블 오브젝트

Unity ScriptableObject를 활용한 데이터 주도 설계.

- **런타임 데이터 컨테이너** — MonoBehaviour 오버헤드 없음
- 씬 간 공유 설정 관리
- Inspector 친화적 데이터 편집
- 적용: 아이템 DB, 캐릭터 스탯, 게임 설정

### Ballistic | 탄도 물리

물리 기반 투사체 궤도 계산 및 시뮬레이션.

- **포물선 궤도** 계산
- 중력 영향 투사체 모션
- 발사 각도 및 속도 산출
- 적용: 포격, 투척 메커닉, 궤도 프리뷰

### Photon Networking | 포톤 네트워킹

Photon PUN 기반 멀티플레이어 네트워킹 패턴.

- **룸 관리** 및 매치메이킹
- 상태 동기화 전략
- RPC (Remote Procedure Call) 패턴
- 적용: 실시간 멀티플레이어 게임, 로비 시스템

### Post-Processing | 포스트 프로세싱

비주얼 향상 파이프라인 및 렌더링 최적화.

- **URP/Built-in** 포스트 프로세싱 설정
- Bloom, Color Grading, Vignette 설정
- 성능 최적화된 이펙트 스태킹
- 적용: 비주얼 폴리싱, 분위기 연출, 스크린샷 모드

### Utility | 유틸리티

재사용 가능한 헬퍼 클래스 및 확장 메서드.

- **오브젝트 풀링** — Instantiate/Destroy 대신 재활용으로 GC 스파이크 제거
- 컬렉션 확장 및 헬퍼 메서드
- 게임 개발용 수학 유틸리티
- 적용: 빈번하게 생성되는 오브젝트 (총알, 이펙트, 파티클)

```csharp
// 풀 사전 생성
ObjectPool.Instance.CreatePool(bulletPrefab, 50);

// 풀에서 가져오기 (Instantiate 대신)
var bullet = ObjectPool.Instance.GetObject(bulletPrefab);

// 풀에 반환 (Destroy 대신)
ObjectPool.Instance.ReturnObject(bullet);
```

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
3. 각 스크립트의 주석을 따라 통합

---

## 라이선스 | License

MIT License

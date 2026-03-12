# AOS Game Portfolio | AOS 게임 포트폴리오

## 개요 | Overview

AOS(Aeon of Strife)류 게임 제작에 관한 기능 구현 및 최적화 포트폴리오입니다. Photon PUN 기반 멀티플레이어 게임으로, 플레이어/몬스터/NPC 시스템, UI 관리, 데이터 저장/로드, 아이템 시스템 등을 포함합니다.

An AOS-style game development portfolio covering feature implementation and optimization. Built on Photon PUN for multiplayer, featuring player/monster/NPC systems, UI management, data persistence, and item systems.

## 주요 기능 | Key Features

### 1. 움직임 구현 | Movement System

- **플레이어** — NavMeshAgent 기반 클릭 이동 + 키보드 이동
- **몬스터** — NavMeshAgent로 플레이어 추적 (감지/추적/공격 상태 머신)
- **NPC** — 기본 Animation 재생

몬스터 상태는 EnumData로 관리: `IDLE -> TRACE -> ATTACK -> DEAD`

Monster states managed via EnumData: `IDLE -> TRACE -> ATTACK -> DEAD`

### 2. 몬스터 소환 | Monster Spawning (Object Pooling)

몬스터를 반복적으로 Instantiate/Destroy하면 GC 스파이크가 발생합니다. 이를 방지하기 위해:

To avoid GC spikes from repeated Instantiate/Destroy:

- 몬스터를 일정량 한번에 소환하여 풀 생성
- 사망 시 `Destroy` 대신 `SetActive(false)`로 비활성화
- 일정 시간 후 다시 `SetActive(true)`로 활성화

### 3. UI 기능 | UI System

- **미니맵** — CameraManager + CineCam을 통한 실시간 미니맵 렌더링
- **인벤토리** — Dictionary<CostumeParts, ItemData> 기반 장비 관리, JSON 저장
- **플레이어 정보** — World Canvas로 HP/MP 표시

### 4. 데이터 저장/로드 | Save/Load System

외부 서버 없이 클라이언트 로컬에 JSON으로 저장:

Client-side JSON persistence without external server:

```csharp
string ToJson = JsonUtility.ToJson(playerInfoData);
File.WriteAllText(filePath, ToJson);
```

### 5. Photon 네트워킹 | Photon Networking

- PunManager로 서버 연결 및 룸 관리
- `SendRPC` / `ReceiveRPC` 패턴으로 통신
- RPCType enum으로 타입별 분기 처리
- 각 오브젝트가 이벤트 발생 시 자신의 Photon으로 직접 RPC 전송 (마스터 과부하 방지)

## 프로젝트 구조 | Project Structure

```
Portfolio/
├── README.md
└── Scripts/
    ├── GameManager.cs          # 게임 관리, 저장/로드, 튜토리얼, 몬스터 풀
    ├── UIManager.cs            # UI 스택 관리, Addressable 에셋 로딩
    ├── PunManager.cs           # Photon 서버 연결, 룸/플레이어/몬스터 관리
    ├── CameraManager.cs        # 미니맵 카메라 관리
    ├── CameraManager_CineCam.cs # 시네 카메라 (팔로우, 회전, 흔들림)
    ├── MapManager.cs           # 맵 관리 (placeholder)
    ├── ShopManager.cs          # 상점 관리 (placeholder)
    ├── Player/
    │   ├── NPlayer.cs          # 플레이어/몬스터/NPC 공통 엔티티
    │   ├── NPlayerMove.cs      # 이동, 점프, 대시, 구르기
    │   └── NPlayerRPC.cs       # RPC 송수신 및 타입별 처리
    ├── Data/
    │   ├── DataTableManager.cs # 데이터 테이블 관리 (레벨, 몬스터, 아이템)
    │   ├── ItemInfo.cs         # 아이템 데이터 구조
    │   ├── MonsterInfo.cs      # 몬스터 데이터 구조
    │   ├── PlayerInfo.cs       # 플레이어 데이터 구조
    │   └── EnumDataList.cs     # 게임 전역 Enum 정의
    └── Effects/
        └── DissolveShader.shader # Dissolve 쉐이더 (URP)
```

## 기술 스택 | Tech Stack

- **Unity 2021.3+**
- **Photon PUN2** — 멀티플레이어 네트워킹
- **Addressables** — 런타임 에셋 로딩
- **Newtonsoft.Json** — 데이터 직렬화
- **NavMeshAgent** — AI 길찾기
- **CodeStage Anti-Cheat** — 메모리 보호

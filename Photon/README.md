# Photon Networking | 포톤 네트워킹

## 개요 | Overview

Photon PUN 기반 멀티플레이어 네트워킹 패턴입니다. 룸 관리, 매치메이킹, RPC 통신 등을 다룹니다.

Photon PUN-based multiplayer networking patterns covering room management, matchmaking, and RPC communication.

## Photon Room 개념 | Room Concept

포톤에서 Room은 플레이어들이 만나서 어떤 행위를 할 수 있게 해주는 케이지(공간)입니다.

In Photon, a Room is a container where players can meet and interact.

### 게임 참여 플로우 | Game Join Flow

1. **서버 세팅 (Server Setting)** — `PhotonNetwork.ConnectUsingSettings()`
2. **로비 입장 (Join Lobby)** — 방 목록 확인
3. **방 입장 (Join Room)** — 선택한 방에 입장

방에 참여해야만 RPC(Remote Procedure Call), 즉 플레이어 간 신호를 주고받을 수 있습니다.

Players must join a room to send/receive RPCs (Remote Procedure Calls).

## 게임 유형별 RPC 전략 | RPC Strategy by Game Type

### 1. 상태 기반 게임 (State-Based / Master-Controlled)

방장(마스터 클라이언트)이 상태를 변환시키고, 다른 플레이어들은 받은 RPC를 통해 자신의 상태만 변경합니다.

The master client controls state transitions. Other players only modify their local state based on received RPCs.

- 주요 상태 변환은 마스터가 담당
- 결과도 마스터가 정리하여 전파
- 적용: 스테이지 기반 게임, 보드 게임

### 2. RPG 스타일 (Individual RPC)

각 플레이어가 `RPCTarget.AllBuffered`로 개인의 RPC 정보를 방 전체에 전송합니다.

Each player sends their own RPCs to all players using `RPCTarget.AllBuffered`.

- 마스터 클라이언트에 트래픽 과부하 방지
- 각 플레이어가 자신의 신호를 직접 전송
- 적용: MMORPG, 오픈월드

## 프로토콜 | Protocol (UDP vs TCP)

### UDP (User Datagram Protocol)
- 비연결형 프로토콜 — 전송 속도가 빠름
- 1:1, 1:N, N:N 통신 가능
- 신뢰성이 낮음 — 패킷 순서 뒤바뀜 가능 (예: 1,2,3 -> 1,3,2)
- 게임 네트워킹에 주로 사용

### TCP (Transmission Control Protocol)
- 연결형 프로토콜 — 높은 신뢰성
- 전송 데이터 크기 무제한
- 순서 보장 — (예: 1,2,3 -> 1,2,3)
- 스트리밍 서비스에는 부적합 (재전송 요청으로 인한 지연)

## Photon Room Flow | 룸 플로우

`PhotonNetwork.CreateRoom()` 또는 `PhotonNetwork.JoinRoom()` 호출 시 `OnJoinedRoom()` 콜백이 발생합니다.

When calling `CreateRoom()` or `JoinRoom()`, the `OnJoinedRoom()` callback is triggered.

로비에서 방을 만들고 선택하여 입장하려면:
- `CreateRoom`에서 `OnJoinedRoom` 콜백이 바로 발생하는 것을 제어해야 합니다
- 또는 로비 자체를 하나의 방으로 처리할 수 있지만, `UpdateRoomList` 콜백을 받을 수 없는 제약이 있습니다

## 파일 | Files

- `PunManager.cs` — Photon PUN 매니저 (서버 연결, 룸 관리, 플레이어 스폰, RPC 처리)

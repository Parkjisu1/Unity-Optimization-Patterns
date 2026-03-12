# SnowFight Mini-Game | 눈싸움 미니게임

## 개요 | Overview

Photon PUN 기반 실시간 멀티플레이어 눈싸움(배틀로얄) 미니게임 시스템입니다.

A real-time multiplayer snowball fight (battle royale) mini-game system built on Photon PUN.

## 주요 기능 | Key Features

- **멀티플레이어 매칭** — 최소/최대 플레이어 수 관리, 대기 타이머
- **아이템 시스템** — 눈덩이, 힐, 쉴드, 스피드업, 머신건, 파워 불릿
- **HP 관리** — 마스터 클라이언트 기반 데미지/힐 처리
- **버프 시스템** — 지속시간 기반 버프 관리 (쉴드, 머신건, 파워불릿, 스피드)
- **리스폰 시스템** — 사망 후 무적시간 포함 리스폰
- **게임 스테이트 머신** — 대기 -> 카운트다운 -> 플레이 -> 종료 -> 결과

## 게임 플로우 | Game Flow

1. `BR_PLAYER_WAIT` — 플레이어 대기 (최소 인원 충족 또는 타이머 만료)
2. `BRS_SELECT_GAME` — 게임 선택
3. `BRS_FIELD_LOAD` — 맵 로드 및 스폰 위치 배정
4. `BRS_FIELD_MAKE` — 필드 생성
5. `READY_COUNT_DOWN` — 카운트다운
6. `GAME_START` / `PLAY` — 게임 진행
7. `FINISH` — 시간 초과
8. `RESULT` — 결과 표시 (킬/데스/히트 수)

## 아이템 종류 | Item Types

| 아이템 | 효과 |
|--------|------|
| ITEM_SNOW_BULLET | 눈덩이 탄약 보충 |
| ITEM_HEAL | HP 회복 |
| ITEM_SPEEDUP | 이동 속도 증가 (지속시간) |
| ITEM_SHIELD | 피해 1회 방어 (지속시간) |
| ITEM_MACHINEGUN | 빠른 연사 (지속시간) |
| ITEM_POWER_BULLET | 강화 탄환 (지속시간) |

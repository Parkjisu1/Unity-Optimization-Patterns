# Ballistic / Formula Manager | 탄도 물리 계산

## 개요 | Overview

물리 기반 투사체 궤도 계산 및 바람 저항 시뮬레이션을 위한 매니저 클래스입니다.

A physics-based projectile trajectory calculator with wind resistance simulation for ballistic mechanics.

## 주요 기능 | Key Features

- **포물선 궤도 계산 (Parabolic Trajectory)** — 발사 각도와 속도에 따른 X, Y, Z 위치 계산
- **바람 저항 시뮬레이션 (Wind Resistance)** — 동/서/남/북/상/하 방향의 바람 영향 반영
- **Inspector 연동** — Range 슬라이더로 실시간 각도/속도 조절 가능

## 수학 공식 | Math Formulas

- **수평 위치 (X)**: `V * sin(angle_H) * t`
- **수직 위치 (Y)**: `V * sin(angle_V) * t - g * t^2 / 2`
- **전방 위치 (Z)**: `V * cos(angle_V) * t`
- **바람 영향**: 방향 벡터 * 바람 세기 * 시간

## 사용법 | Usage

1. `FormulaManager`를 빈 GameObject에 부착
2. Shooter, Bullet 프리팹 할당
3. Inspector에서 속도/각도/바람 설정 조절
4. 마우스 클릭으로 발사

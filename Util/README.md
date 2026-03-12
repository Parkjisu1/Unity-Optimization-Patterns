# Utility | 유틸리티

## 개요 | Overview

게임 개발에서 자주 사용하는 재사용 가능한 헬퍼 클래스 모음입니다.

A collection of reusable helper classes commonly used in game development.

## 파일 | Files

### MathHelper.cs

#### 각도 계산 (CalculateAngle)

마우스 클릭을 통한 이동에서 이동 방향의 각도를 계산합니다. 각도가 크게 벌어지면 미끄러짐을 방지합니다.

Calculates the direction angle for click-to-move mechanics. Prevents sliding when the angle difference is too large.

**과정 | Process:**
1. `Vector3.Dot` — 현재 forward 벡터와 목표 방향의 내적 계산
2. `Mathf.Acos` — 내적 값을 라디안으로 변환
3. `Radian * Mathf.Rad2Deg` — 라디안을 각도(degree)로 변환
4. 각도가 임계값 초과 시 NavMesh 경로 리셋

#### Ref 키워드 (Ref Keyword)

`ref`는 매개변수 한정자입니다. 매개변수로 전달한 값의 원본이 변경됩니다.

`ref` is a parameter modifier that allows the method to modify the original variable.

```csharp
int _temp = 10;
SetData(ref _temp);
// _temp is now 20 (원본 값이 변경됨)
```

### PlayerMovement.cs

키보드 입력 기반 부드러운 플레이어 이동 구현입니다.

Smooth keyboard-based player movement with camera-relative direction.

**주요 요소 | Key Components:**
- `CharacterController` — 물리 기반 이동
- 카메라-플레이어 상대적 방향 계산
- `Mathf.Lerp` — 부드러운 가속/감속
- `Vector3.Slerp` — 부드러운 회전
- Blend Tree 애니메이션 연동 가능

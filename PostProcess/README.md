# Post-Processing | 포스트 프로세싱

## 개요 | Overview

PostProcess는 후처리 기능 중 하나입니다. 일반적으로 유니티에서 게임 창을 보면 일반 카메라에서 찍혀지는 장면을 송출합니다. PostProcess의 기능은 카메라에 보이는 모습에 하나의 보기 좋은 막을 덧씌워서 보여주는 것이라고 생각하면 편합니다.

Post-Processing is a rendering technique that applies visual effects on top of the camera's output. Think of it as placing a visually-enhancing filter over what the camera sees.

## 설정 방법 | Setup

### URP (Universal Render Pipeline)

1. **Volume 추가** — Scene에 Global Volume 오브젝트 생성
2. **Profile 생성** — Volume 컴포넌트에서 New Profile 생성
3. **Override 추가** — 원하는 효과(Bloom, Color Grading, Vignette 등) 추가
4. **카메라 설정** — Camera 컴포넌트에서 Post Processing 체크

### Built-in Pipeline

1. **패키지 설치** — Package Manager에서 Post Processing 설치
2. **Post Process Layer** — 카메라에 Post Process Layer 컴포넌트 추가
3. **Post Process Volume** — Scene에 Post Process Volume 추가
4. **Profile 설정** — 원하는 효과 추가 및 조정

## 주요 효과 | Common Effects

| 효과 | 설명 |
|------|------|
| **Bloom** | 밝은 영역에서 빛 번짐 효과 |
| **Color Grading** | 색감/톤 조정 |
| **Vignette** | 화면 가장자리 어둡게 처리 |
| **Depth of Field** | 피사계 심도 (배경 흐림) |
| **Ambient Occlusion** | 구석/틈새 그림자 강화 |

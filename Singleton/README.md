# Singleton Pattern | 싱글톤 패턴

## 개요 | Overview

Unity MonoBehaviour 매니저를 위한 스레드 안전 싱글톤 패턴입니다.

A generic Singleton base class for Unity MonoBehaviour managers, providing lazy initialization and global access.

## 설명 | Description

보통 다른 스크립트를 변수처럼 다른 CS 파일에서 사용하기 위해 `static`을 선언하여 사용합니다:

Typically, to use one script from another like a variable, we declare a static instance:

```csharp
public static ClassName Instance;

private void Awake()
{
    Instance = this;
}
```

이 방법도 동작하지만 스크립트의 낭비라고 할 수 있습니다. 그래서 Singleton 선언을 하여 스크립트를 변수처럼 사용할 수 있는 제네릭 클래스를 만들었습니다.

While this works, it can be considered redundant boilerplate. Instead, this generic Singleton class lets any MonoBehaviour be used as a globally-accessible instance automatically.

## 사용법 | Usage

```csharp
public class GameManager : Singleton<GameManager>
{
    // Access via GameManager.Instance.DoSomething()
    public void DoSomething() { }
}
```

## 주요 기능 | Key Features

- **지연 초기화 (Lazy Initialization)** — `FindObjectOfType`으로 먼저 찾고, 없으면 자동 생성
- **전역 접근 (Global Access)** — `T.Instance`로 어디서든 접근 가능
- **DontDestroyOnLoad** 지원 가능 — 필요시 `Awake()`에서 추가

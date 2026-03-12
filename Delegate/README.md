# Delegate & Event | 델리게이트 & 이벤트

## 개요 | Overview

C# 델리게이트와 Action/Func를 활용한 시스템 간 비결합 통신 패턴입니다.

C# Delegates, Action, and Func patterns for decoupled inter-system communication in Unity.

## 델리게이트란? | What is a Delegate?

Delegate는 함수를 매개변수처럼 보내서 사용하는 것을 말합니다. 예를 들어 A라는 CS에서 B라는 CS로 Delegate를 사용하여 보내고, 임시로 저장하여 B에서 특수 이벤트에 구동시킵니다.

A Delegate allows you to pass functions as parameters. For example, script A can send a delegate to script B, which stores it and invokes it on a special event.

### 방어 코드 | Null Guard

```csharp
if (delegateInstance == null)
    return;
```

### 사용 예시 | Usage Example

```csharp
private DelegateDel _tempDel;

public void GetDelegate(DelegateDel _delegateDel)
{
    if (_delegateDel == null) return;
    _tempDel = _delegateDel;
}
```

UI 기능 구현에서 자주 사용됩니다. Commonly used in UI callback implementations.

## Action & Func

Delegate와 비슷한 역할을 하지만 미리 선언하지 않고 바로 사용할 수 있습니다.

Similar to delegates but without requiring a separate declaration.

- **Action** — 반환값이 없는 델리게이트 (void delegate)
- **Func** — 반환값이 있는 델리게이트 (delegate with return value)

```csharp
// Using System.Linq required
Action result = () => Debug.Log("Result");
result();

Func<int> value = () => 1 + 2;
Debug.Log($"value: {value()}");
```

큰 차이는 없지만, 별도의 delegate 선언 없이 사용할 수 있다는 편의성이 있습니다.

The main convenience is that you don't need a separate delegate type declaration.

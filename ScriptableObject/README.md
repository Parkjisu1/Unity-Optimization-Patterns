# ScriptableObject | 스크립터블 오브젝트

## 개요 | Overview

Unity ScriptableObject를 활용한 데이터 주도 설계 패턴입니다.

Data-driven design using Unity ScriptableObjects as lightweight data containers.

## ScriptableObject란? | What is ScriptableObject?

유니티 내에서 지원하는 데이터를 가지는 형식입니다. 대용량이 아닌 적은 용량의 데이터(몬스터 정보, 인디게임에서 사용되는 소량의 데이터)를 관리하고 사용하기에 충분히 용이합니다.

A Unity-native data container format. Suitable for managing small-to-medium data sets like monster info or indie game configurations.

## 데이터 로드 | Data Loading

제작해 놓은 UIManager에서 `LoadAsset`을 통해 `<T>`를 ScriptableObj로 하여 Addressable로 불러옵니다.

Load via UIManager's `LoadAsset<T>` using Addressables.

## 대용량 데이터 | Large Data Sets

대용량 데이터 관리에는 기능이 충분히 용이하지 않기 때문에, 레벨 데이터 혹은 플레이어의 레벨별 기본 정보와 같은 대용량 데이터의 경우 Google Sheet를 사용하여 관리하는 것이 더 좋습니다.

For large data sets (level data, player stats per level), using Google Sheets or external databases is more practical.

## 사용 예시 | Usage Example

```csharp
[CreateAssetMenu(fileName = "ServerSetting", menuName = "ScriptableSetting/ServerSetting")]
public class ScriptableServerSetting : ScriptableObject
{
    public string AppIdRealtime = "******-****-****-****-************";
    public bool UseNameServer = true;
    public string AppVersion = "1.0.0";
    public string FixedRegion = "kr";
}
```

`[CreateAssetMenu]` 어트리뷰트를 사용하면 Unity 메뉴에서 편리하게 ScriptableObject 에셋을 생성할 수 있습니다.

The `[CreateAssetMenu]` attribute adds a menu entry for easy asset creation in Unity Editor.

## 데이터 필드 예시 | Data Field Examples

```csharp
[SerializeField] private int Level;
[SerializeField] private float HP;
[SerializeField] private float MP;
[SerializeField] private float EXP;
[SerializeField] private Vector3 OriPos;
```

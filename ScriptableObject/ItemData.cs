using UnityEngine;

/// <summary>
/// Example ScriptableObject for server/game settings.
/// ScriptableObjects serve as lightweight data containers without MonoBehaviour overhead.
/// </summary>
[CreateAssetMenu(fileName = "ServerSetting", menuName = "ScriptableSetting/ServerSetting")]
public class ScriptableServerSetting : ScriptableObject
{
    public string AppIdRealtime = "******-****-****-****-************";

    public bool UseNameServer = true;
    public bool EnableProtocolFallback = true;

    public string Server = string.Empty;
    public int Port = 0;

    public string AppVersion = "1.0.0";
    public string FixedRegion = "kr";
}

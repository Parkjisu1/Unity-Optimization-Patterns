using System;
using UnityEngine;

/// <summary>
/// Examples of C# Delegate, Action, and Func usage in Unity.
/// </summary>
public class DelegateExample : MonoBehaviour
{
    // --- Delegate Declaration ---
    public delegate void DelegateDel();

    private DelegateDel _tempDel;

    /// <summary>
    /// Stores a delegate for later invocation (e.g., UI callback).
    /// </summary>
    public void GetDelegate(DelegateDel _delegateDel)
    {
        if (_delegateDel == null)
            return;

        _tempDel = _delegateDel;
    }

    /// <summary>
    /// Invokes the stored delegate if available.
    /// </summary>
    public void InvokeDelegate()
    {
        if (_tempDel == null)
            return;

        _tempDel();
    }

    // --- Action / Func Examples ---

    private void Start()
    {
        // Action: delegate with no return value
        Action result = () => Debug.Log("<color=red>Result</color>");
        result();

        // Func: delegate with a return value
        Func<int> value = () => 1 + 2;
        Debug.Log($"<color=blue>value: {value()}</color>");
    }
}

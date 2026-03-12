using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Math helper utilities for angle calculation and ref keyword examples.
/// </summary>
public class MathHelper : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private Vector3 destination;
    private bool IsMove;

    /// <summary>
    /// Calculates the angle between the forward direction and the destination,
    /// then sets the NavMeshAgent destination. Prevents sliding when angle is too large.
    /// 
    /// 마우스 클릭 이동 시 방향 각도를 계산하여 미끄러짐을 방지합니다.
    /// </summary>
    public void CalculateAngle(Vector3 dest)
    {
        // Dot product between forward vector and normalized direction to destination
        float Dot = Vector3.Dot(transform.forward, (dest - transform.position).normalized);

        // Convert dot product to radian using Acos
        float Radian = Mathf.Acos(Dot);

        // Convert radian to degree (radian * Rad2Deg)
        float Degree = Radian * Mathf.Rad2Deg;

        // If angle is too large, reset path to prevent sliding
        if (Degree > 0.5f)
        {
            navMeshAgent.nextPosition = transform.position;
            navMeshAgent.ResetPath();
        }

        navMeshAgent.SetDestination(dest);
        destination = dest;
        IsMove = true;
    }

    /// <summary>
    /// Demonstrates the ref keyword: modifies the original variable passed as parameter.
    /// ref 키워드: 매개변수로 전달한 값의 원본이 변경됩니다.
    /// 
    /// Example output:
    ///   Before SetData: _temp = 10
    ///   After SetData:  _temp = 20
    /// </summary>
    public void RefExample()
    {
        int _temp = 10;
        Debug.Log($"<color=red>_temp:{_temp}</color>"); // Output: 10

        SetData(ref _temp);
        Debug.Log($"<color=blue>_temp:{_temp}</color>"); // Output: 20
    }

    private void SetData(ref int temp)
    {
        temp = 20;
    }
}

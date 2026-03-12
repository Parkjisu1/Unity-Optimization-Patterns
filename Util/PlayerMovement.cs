using UnityEngine;

/// <summary>
/// Smooth player movement using keyboard input, camera-relative direction,
/// and CharacterController for physics-based motion.
/// 
/// 키보드 입력 기반 부드러운 플레이어 이동 구현.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("[Movement Settings]")]
    public float MoveSpeed = 6f;
    public float RunScale = 1.2f;
    public float Interpolation = 10f;

    private float currentV = 0;
    private float currentH = 0;
    private Vector3 currentDirection = Vector3.zero;
    private Vector3 moveDirection;

    private CharacterController charController;
    private Transform cameraTransform;

    private void Start()
    {
        charController = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    /// <summary>
    /// Call in Update() for smooth keyboard-based movement.
    /// Update에 넣으면 움직임 구동 가능.
    /// </summary>
    void DefaultMove()
    {
        float _v = Input.GetAxis("Vertical");
        float _h = Input.GetAxis("Horizontal");

        // Sprint with Left Shift
        if (Input.GetKey(KeyCode.LeftShift))
        {
            _v *= RunScale;
            _h *= RunScale;
        }

        // Smooth interpolation for fluid movement
        currentV = Mathf.Lerp(currentV, _v, Time.deltaTime * Interpolation);
        currentH = Mathf.Lerp(currentH, _h, Time.deltaTime * Interpolation);

        // Camera-relative direction
        Vector3 _direction = cameraTransform.forward * currentV + cameraTransform.right * currentH;

        float _directionLength = _direction.magnitude;
        _direction.y = 0;
        _direction = _direction.normalized * _directionLength;

        if (_direction != Vector3.zero)
        {
            currentDirection = Vector3.Slerp(currentDirection, _direction, Time.deltaTime * Interpolation);

            transform.rotation = Quaternion.LookRotation(currentDirection);
            moveDirection = new Vector3(currentDirection.x, moveDirection.y, currentDirection.z);
        }

        // Animation blend tree parameter: use direction magnitude
        // animator.SetFloat("MoveSpeed", direction.magnitude);

        if (charController.enabled)
        {
            charController.Move(moveDirection * MoveSpeed * Time.deltaTime);
        }
    }

    private void Update()
    {
        DefaultMove();
    }
}

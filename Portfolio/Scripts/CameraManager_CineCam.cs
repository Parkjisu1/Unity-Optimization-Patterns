using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CineCam : Singletone<CineCam>
{
    public CamTargetType CamTargetType { get; private set; }
    private float rotLerpTime;

    private Vector3 OriPos = new Vector3(0,-0.6f,1.1f);


    private Vector3 oriRotPos;
    private Vector3 newRotPos;
    private Vector3 posNormalize;

    private Vector3 rotTarget;
    private Quaternion quaternionTarget;
    private Vector3 cameraRayTarget;
    private RaycastHit cameraRayHitInfo;

    private bool isRotTouch;

    private float rotSensitive;
    [SerializeField]
    public float CamRotSensitive
    {
        get { return rotSensitive; }
        set
        {
            rotSensitive = value;
        }
    }

    [Header("[Camera]")]
    public Transform CineCamTr;
    public Camera CineCamera;

    [Header("[Audio]")]
    public AudioListener AudionListener;

    [Header("[UI Ray]")]
    public GraphicRaycaster graphicRaycaster;
    private PointerEventData pointEvtData;
    private List<RaycastResult> rayResults = new List<RaycastResult>();

    [Header("[Cam Shake]")]
    public float shakeTime = 1.0f;
    public float shakeSpeed = 3.0f;
    public float shakeAmount = 1.5f;

    void Awake()
    {
        pointEvtData = new PointerEventData(null);
    }

    public void Init()
    {
        CamEnable(true);
        CamTargetType = CamTargetType.PLAYER;
        rotLerpTime = 0.4f;
    }

    public void CamEnable(bool _enable)
    {
        CineCamera.gameObject.SetActive(_enable);
    }

    public void FollowTarget(Transform _target)
    {
        transform.position = _target.position;

        cameraRayTarget = CineCamera.transform.position - _target.position;
        CineCamera.transform.localPosition = new Vector3(OriPos.x, OriPos.y, OriPos.z);
        Physics.Raycast(_target.position , cameraRayTarget, out cameraRayHitInfo, Vector3.Distance(CineCamera.transform.position, _target.position));
        Debug.DrawLine(_target.position , CineCamera.transform.position, Color.magenta);

        CamaraManager.Instance.RefreshMiniMap(_target);


        //TODO 테스트 기능 구현을 위해 나중에 변경
        //if (cameraRayHitInfo.collider == null)
        //{
        //    CineCamera.transform.localPosition = Vector3.Lerp(CineCamera.transform.localPosition, Vector3.zero, Time.deltaTime * 5);


        //}
        //else if (cameraRayHitInfo.collider != null)
        //{
        //    if (cameraRayHitInfo.collider.tag == "Untagged")
        //        return;

        //    CineCamera.transform.position = Vector3.Lerp(CineCamera.transform.position, cameraRayHitInfo.point, Time.deltaTime * 5);
        //}
        //TODO 테스트 기능 구현을 위해 나중에 변경
        //CineCamera.transform.localPosition = Vector3.Lerp(CineCamera.transform.position, cameraRayTarget, Time.deltaTime);
    }

    void RotateSet()
    {
        //#if UNITY_EDITOR
        //        if (Main.Instance.TestSetting.RotSensitive > 0)
        //            CamRotSensitive = Main.Instance.TestSetting.RotSensitive;
        //#endif
        CamRotSensitive = 10f;
        isRotTouch = false;

        pointEvtData.position = Input.mousePosition;
        rayResults.Clear();
        graphicRaycaster.Raycast(pointEvtData, rayResults);

        if (rayResults.Count <= 0)
        {
            if (Input.GetMouseButtonDown(1))
            {
                oriRotPos = Input.mousePosition;
            }
            else if (Input.GetMouseButton(1))
            {
                if (oriRotPos != Input.mousePosition)
                {
                    newRotPos = Input.mousePosition;
                    posNormalize = newRotPos - oriRotPos;
                    //                        posNormalize.Normalize();
                    isRotTouch = true;
                }
            }
        }

        if (isRotTouch)
        {
                // 좌, 우, 상, 하 회전
                rotTarget = transform.eulerAngles + (new Vector3(-posNormalize.y, posNormalize.x, 0) * CamRotSensitive * Time.deltaTime);

                if (rotTarget.x > 180)
                    rotTarget.x = Mathf.Clamp(rotTarget.x, 341.7f, 380f);
                else if (rotTarget.x <= 180)
                    rotTarget.x = Mathf.Clamp(rotTarget.x, -20, 30f);

                SetQueaternionTarget();
            

            oriRotPos = newRotPos;
        }
    }

    void SetQueaternionTarget()
    {
        rotTarget.z = 0;

        if (rotTarget.y > 360)
            rotTarget.y -= 360f;
        else if (rotTarget.y < 0)
            rotTarget.y += 360f;

        quaternionTarget = Quaternion.Euler(rotTarget);
    }
    void Rotate()
    {
        if (transform.rotation.eulerAngles.magnitude != rotTarget.magnitude)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, quaternionTarget, rotLerpTime);
        }
    }

    public void ShakeCam()
    {
        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        Vector3 originPos = CineCamTr.localPosition;
        float _temp = 0.0f;

        while(_temp < shakeTime)
        {
//#if UNITY_EDITOR
//            //shakeAmount = Main.Instance.TestSetting.shakeAmount;
//            //shakeSpeed = Main.Instance.TestSetting.shakeSpeed;

//#endif
            Vector3 randomPoint = originPos + Random.insideUnitSphere * shakeAmount;
            CineCamTr.localPosition = Vector3.Lerp(CineCamTr.localPosition, randomPoint, Time.deltaTime * shakeSpeed);

            yield return null;

            _temp += Time.deltaTime;

        }

        CineCamTr.localPosition = originPos;
    }

    void Update()
    {
        RotateSet();
        Rotate();
    }
}

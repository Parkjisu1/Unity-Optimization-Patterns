using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singletone<CameraManager>
{
    [Header("[CAMERA]")]
    //public CineCam CinecamRot;
    public Transform CineCamTr;
    public Camera UICamera;
    public GameObject MiniMapCamera;
   

    [Header("[Camera Speed]")]
    public float TestCameraAutoZoomSpeed = 0;



    public void RefreshMiniMap(Transform _transform)
    {
        if (MiniMapCamera == null)
            return;

        MiniMapCamera.transform.localPosition=new Vector3(_transform.localPosition.x,MiniMapCamera.transform.localPosition.y,_transform.localPosition.z);
    }

}

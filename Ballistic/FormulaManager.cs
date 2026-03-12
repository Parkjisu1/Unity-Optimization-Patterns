using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public enum WindKind
{ 
  NONE=0,
  EAST,
  WEST,
  SOUTH,
  NORTH,
}

public enum WIndDir
{
  NONE =0,
  UP,
  DOWN,
}

public class FormulaManager : MonoBehaviour
{
    [Header("[Shooter]")]
    public GameObject Shooter;
    public Vector3 StartPos;
    public bool IsShooted = false;


    [Header("[Target]")]
    public Vector3 TargetPos;

    [Header("[Bullet]")]
    [Range(0,1000)]
    public float Bullet_Velocity;
    [Range(-45,45)]
    public double Bullet_Angle_H;
    
    [Range(-25, 25)]
    public double Bullet_Angle_V;
    private double Bullet_Radian_V;

    private float G=9.81f;
    private float time;

    [Header("[Bullet]")]
    public GameObject Bullet;
    private Ball _bullet;

    [Header("[Wind]")]
    public WindKind WindKind;
    public WIndDir WindUPDOWN;
    [Range(0,10)]
    public float WindStrength;

    private void Update()
    {
        SetLauncher();
    }
    #region Test1
    private void SetLauncher()
    {
        if(Input.GetKeyDown(KeyCode.Mouse0))
        {
            if(!IsShooted)
            {
                IsShooted = true;
                StartCoroutine(ChangeIsShoot());
                GameObject _temp = Instantiate(Bullet, StartPos, Shooter.gameObject.transform.rotation);
                _bullet = _temp.GetComponent<Ball>();
            }
           
        }

        Shooter.transform.rotation = Quaternion.Euler((float)Bullet_Angle_V, (float)Bullet_Angle_H, 0);

        if (IsShooted)
            time += Time.deltaTime;

        if(IsShooted && time <5)
        {
            Vector3 _bullPos = new Vector3((float)GetPosX(Bullet_Angle_H), (float)GetPosY(Bullet_Angle_V) + StartPos.y, (float)GetPosZ(Bullet_Angle_V)) + GetWindResist(WindKind, WindUPDOWN, WindStrength);

            _bullet.SetPosition(_bullPos, WindKind.NONE);
 
            Debug.Log($"<color=yellow>Y:{GetPosY(Bullet_Angle_V)+ StartPos.y}</color>\n<color=red>X:{GetPosZ(Bullet_Angle_V)}</color>\n<color=blue>{GetPosX(Bullet_Angle_H)}</color>");
        }
    }
    private IEnumerator ChangeIsShoot()
    {
        yield return new WaitForSeconds(5f);
        IsShooted = false;
        time = 0;
        Destroy(_bullet.gameObject);
    }

    private double GetBulletAngleV(double _bulletAngleV)
    {
        double radian = _bulletAngleV * Math.PI / 180;
        return radian;
    }

    private double GetPosX(double _bulletAngleH)
    {
        double radian = _bulletAngleH * Math.PI / 180;
        double PosX = Bullet_Velocity*(Math.Sin(radian))*time;
        return PosX;
    }

    private double GetPosY(double _bulletAngleV)
    {
        double PosY = (Bullet_Velocity * Math.Sin(GetBulletAngleV(-_bulletAngleV))) * time - G * Math.Pow(time, 2) / 2;

        return PosY;
    }

    private double GetPosZ(double _bulletAngleV)
    {
        double PosZ = (Bullet_Velocity * Math.Cos(GetBulletAngleV(_bulletAngleV))) * time;

        return PosZ;
    }

    private Vector3 windDir;
    private Vector3 GetWindResist(WindKind _windKind,WIndDir _windUpDown,float _windStrength)
    {
        switch (_windKind)
        {
            case WindKind.NONE:
                windDir = new Vector3(0, 0, 0);
                break;
            case WindKind.EAST:
                windDir = new Vector3(1, 0, 0);
                break;
            case WindKind.WEST:
                windDir = new Vector3(-1, 0, 0);
                break;
            case WindKind.SOUTH:
                windDir = new Vector3(0, 0, -1);
                break;
            case WindKind.NORTH:
                windDir = new Vector3(0, 0, 1);
                break;
        }

        switch (_windUpDown)
        {
            case WIndDir.NONE:break;
            case WIndDir.UP:
                windDir += new Vector3(0, 1, 0);
                break;
            case WIndDir.DOWN:
                windDir += new Vector3(0, -1, 0);
                break;
        }
        float windLength = windDir.magnitude;
        Vector3 _temp = windDir.normalized;
        _temp = _temp * windLength;

        Debug.Log($"<color=cyan>{_temp * _windStrength * time}</color>");
        return _temp*_windStrength * time;
    }

    #endregion
   
}

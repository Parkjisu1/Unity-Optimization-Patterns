using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using CodeStage.AntiCheat.ObscuredTypes;
using System.Threading;
using Photon.Pun;

public class NplayerMove : MonoBehaviourPun
{

    public Nplayer nPlayer;

    private Animator animator;

    private Transform CamTrans;
    private Transform possibleMoveAreaCenter;
    private ObscuredFloat playerMoveDisMax;

    private bool IsNpc { get; set; }
    private int npcID;

    public bool IsSeaSwimming { get; set; }

    private bool IsMove;
    private Vector3 destination;

    private bool wasGround;

    private float interpolation = 1000;
    private readonly float RunScale = 1.2f;
    private float currentV = 0;
    private float currentH = 0;

    private Vector3 currentDirection = Vector3.zero;
    private Vector3 moveValue;

    private float moveSpeed = 6f;//1.8F
    private float jumpForce = 3f;

    public bool isJump = false;
    public JumpSt JumpSt { get; private set; }


    private float groundChkDis = 0.5f;
    [SerializeField]
    private float seaChkDis = 0.5f;
    private int groundChkResultCnt;
    private List<Vector3> groundChkPosList = new List<Vector3>()
    {
        Vector3.up * 0.2f
        ,(Vector3.up * 0.2f) + (Vector3.left * 0.16f)
        ,(Vector3.up * 0.2f) + (Vector3.right * 0.16f)
    };

    private RaycastHit[] seaChkRaycastHit;
    private bool isInSea;
    private bool isInMap;

    public int UseChairNo { get; private set; }

    private float gravity = -6.18f;
    private Vector3 moveDirection;

    //TODO 테스트 마우스 클릭
    private Vector3 movePoint;
    private Vector3 OriDirection;

    #region Monster
    public MonsterState monstate { get; set; }
    private bool IsMonster { get; set; }
    private float TraceDist = 15.0f;
    private float AttackDist = 3.2f;
    #endregion
    #region NPC
    public NpcKind npckind { get; set; }
    #endregion
    [SerializeField]
    private CharacterController charController;

    //[SerializeField]
    //private CapsuleCollider capsuleCollider;

    [SerializeField]
    private NavMeshAgent navMeshAgent;

    [SerializeField]
    private Rigidbody rigidbody;
    #region SturnSystem
    public bool IsSturn { get; private set; }
    private float SturnTime = 0;
    #endregion

    #region Roll
    private bool IsRun { get; set; }

    public GameObject RollDicrection;
    public RollSt RollSt { get; private set; }
    public double RollCool { get; private set; }
    public bool CanRoll { get; private set; }
    #endregion
    #region Dash
    private bool CanDash { get; set; }
    #endregion

    public void Init(object[] _instantiateData)
    {
        int _kind = JsonDecode.ToInt(_instantiateData[0]);
        Debug.Log($"<color=white>Kind :{_kind}, PlayerKind : {(PlayerKind)_kind}</color>");


        if (_kind == (int)PlayerKind.NPC)
        {
            IsNpc = true;
            //capsuleCollider.enabled = true;
            npckind = (NpcKind)JsonDecode.ToInt(_instantiateData[1]);
        }
            
        if(_kind ==(int)PlayerKind.MONSTER)
        {
            IsMonster = true;
            //capsuleCollider.enabled = true;
            monstate = (MonsterState)JsonDecode.ToInt(_instantiateData[1]);

            //StartCoroutine(CheckState());
            //StartCoroutine(CheckStateForAction());
        }

        animator = this.GetComponentInChildren<Animator>();
        if (IsNpc)
           return;

        if (IsMonster)
            return;

        //TODO 테스트 마우스클릭
        navMeshAgent.updateRotation = false;

        JumpSt = JumpSt.START;

        CamTrans = Camera.main.transform;

        //TODO 테스트
        RollSt = RollSt.NONE;
        CanRoll = true;
        RollCool = 0;
        IsRun = false;
        CanDash = true;

        if (photonView.IsMine)
        {
            PunManager.MyRpc.SendTypeRPC((int)RPCType.ANI_MOVEMENT, RpcTarget.AllBuffered);
            Debug.Log($"<color=white>Player Is SendingTypeRPC</color>");
        }
            
    }

    #region Action

    void DefaultMove()
    {
        if (IsNpc)
        {
            animator.Play("Action1");
            charController.Move(moveDirection * moveSpeed * Time.deltaTime);
            return;
        }
          
        if(IsMonster)
            return;
        

        //TODO 테스트
        if (IsSturn)
        {
            SturnTime -= Time.deltaTime;
            if (SturnTime <= 0)
                IsSturn = false;
            return;
        }

        if(Input.GetMouseButtonDown(1))
        {
            Ray ray = CineCam.Instance.CineCamera.ScreenPointToRay(Input.mousePosition);

            if(Physics.Raycast(ray, out RaycastHit hit ))
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.updatePosition = false;
                navMeshAgent.updateRotation = false;
                OriDirection = (hit.point - transform.position).normalized;

                //if(Mathf.Abs(transform.eulerAngles.y-OriDirection.y)>90)
                //{
                //    Debug.Log($"<color=cyan>{Mathf.Abs(transform.eulerAngles.y - OriDirection.y)}</color>");
                //}
                //TODO 테스트 
                //float Dot = Vector3.Dot(transform.rotation);

                SetDestination(hit.point);
            }
        }

        float _v = 0;
        float _h = 0;

        _v = destination.z - transform.position.z;
        _h = destination.x - transform.position.x;

        currentV = Mathf.Lerp(currentV, _v, Time.deltaTime * 20);
        currentH = Mathf.Lerp(currentH, _h, Time.deltaTime * 20);

        Vector3 direction = transform.forward * currentV + transform.right * currentH;
        float _directionLength = direction.magnitude;
        direction.y = 0;
        direction = direction.normalized * _directionLength;



        if(CanDash)
        {
            navMeshAgent.acceleration = 15f;
            navMeshAgent.speed = 9;
        }
        else
        {
            navMeshAgent.acceleration = 95f;
            //나중에 속도 조절
            navMeshAgent.speed = 100;
        }
      
        OriDirection.y = 0;
        //상수 조절해서 원하는값으로 바꾸기 
        transform.rotation = Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(OriDirection),5f*Time.deltaTime);
        AnimationControl(direction.magnitude);
       

        if(navMeshAgent.hasPath)
        {     
            navMeshAgent.acceleration = (navMeshAgent.remainingDistance < closeToEnoughMeters) ? 60f:15f; 
        }


        if (PunManager.MyPlayer == null)
            return; 

        CineCam.Instance.FollowTarget(transform);
    }
    float closeToEnoughMeters = 0.5f;
    //시작 애니메이션 아이들로 고정시킬수있게 하는 함수 => init 와 연결 해서 
    public void AniMovementRPC()
    {
        if(animator!=null)
        {
            animator.Play("Movement");
        }
    }

    public void SetDestination(Vector3 dest)
    {
        //미끄러짐 방지
        float Dot = Vector3.Dot(transform.forward, (dest - transform.position).normalized);
        float Radian = Mathf.Acos(Dot);
        float Degree = Radian * Mathf.Rad2Deg;
        Debug.Log($"<color=yellow>Degree :{Degree}</color>");

        navMeshAgent.isStopped = false;
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;

        if (Degree>0.5f)
        {
            //navMeshAgent.SetDestination(transform.position);
            navMeshAgent.nextPosition= transform.position;
            navMeshAgent.ResetPath();
        }

       
        navMeshAgent.SetDestination(dest);
        destination = dest;
        IsMove = true;
    }

    public void StopNavMeshAgent()
    {
        //이걸로 대쉬 만들수 있음 
        navMeshAgent.isStopped = true;
        StartCoroutine(ReworkNav(2));
    }

    IEnumerator ReworkNav(float _time)
    {

        yield return new WaitForSeconds(_time);
        navMeshAgent.isStopped = false;
    }

    public void UsingSkill()
    {
        if (navMeshAgent.isStopped)
        {
            
            //상승해서 공격가능 애니메이션을 구동시킬때, 파라미터값을 추가하여 애니메이션을 재생하면 됨
            transform.position = Vector3.Slerp(transform.position, transform.position + transform.forward.normalized, 0.2f);
        }
    }
    
    void Roll()
    {
        if (IsRun != true)
            return;

        if (RollSt != RollSt.NONE && CanRoll != true)
            return;

        if(RollCool != 0)
        {
            RollCool -= Time.deltaTime;
            if(RollCool <=0 )
            {
                RollCool = 0;
                CanRoll = true;
            }
        }

        if(Input.GetKey(KeyCode.LeftControl))
        {
            if(CanRoll &&IsNpc ==false)
            {
                RollCool = 2f;
                CanRoll = false;
                RollSt = RollSt.ROLL;

            }
        }
    }

    void Dash()
    {
        if(charController.enabled)
        {
            if (IsMonster)
                return;

            if (IsNpc)
                return;

            if (!CanDash)
                return;

            if(Input.GetKey(KeyCode.Space))
            {

                navMeshAgent.isStopped = true;
                navMeshAgent.updatePosition = false;
                navMeshAgent.updateRotation = false;
                SetDestination(transform.position+transform.forward*10);
                //대쉬에 쿨타임을 적용 후 CanDashing을 만들어서 false
                CanDash = false;
                //Vector3 direction = transform.forward * currentV + transform.right * currentH;
                //direction.y = 0;
                //Debug.LogError($"direction : {direction}");

                //SetDestination함수에 CanDashing이 false일때만으로 조건을 추가 혹은 다른 함수를 만들어서 첨가
                //TODO 테스트
                StartCoroutine(Dashing(2));
            }
        }
    }

    //TODO 테스트 값변형 및 X,Z 추가
    IEnumerator Dashing(double _time)
    {
        //float _z = currentDirection.z;
        //currentDirection.z = _z*2;
        yield return new WaitForSeconds((float)_time);
        CanDash = true ;
        //currentDirection = moveValue;
    }
    #endregion
    
    #region CheckField
    void CheckGround()
    {
        if (IsNpc /*|| IsMonster */|| IsSeaSwimming || UseChairNo > 0)
            return;

#if UNITY_EDITOR
        for (int i = 0; i < groundChkPosList.Count; i++)
        {
            Debug.DrawLine(transform.position + groundChkPosList[i], transform.position + groundChkPosList[i] + (Vector3.down * groundChkDis), Color.red);
        }
#endif

        if (JumpSt != JumpSt.NONE)
        {
            return;
        }

        groundChkResultCnt = 0;

        for (int i = 0; i < groundChkPosList.Count; i++)
        {
            if (Physics.Raycast(transform.position + groundChkPosList[i], Vector3.down, groundChkDis) == false)
                groundChkResultCnt++;
        }
        if (groundChkResultCnt >= 3)
        {
            JumpSt = JumpSt.START;
        }
    }

    void CheckSeaSwimming()
    {
        if (JumpSt != JumpSt.NONE)
        {
            return;
        }

        isInSea = false;
        isInMap = false;
        seaChkRaycastHit = Physics.RaycastAll(transform.position + groundChkPosList[0], Vector3.down, seaChkDis);

#if UNITY_EDITOR
        //        Debug.DrawLine(transform.position + groundChkPosList[0], transform.position + groundChkPosList[0] + (Vector3.down * seaChkDis), Color.green);
#endif

        if (seaChkRaycastHit.Length > 0)
        {
            for (int i = 0; i < seaChkRaycastHit.Length; i++)
            {
                if (seaChkRaycastHit[i].collider.CompareTag("MV_WATER_SWIMMING"))
                    isInSea = true;
            }
        }
    }
    #endregion

    #region Ani
    void AnimationControl(float _moveSpeed)
    {
            if (!nPlayer.CanAttack)
                return;

            animator.Play("Movement");
            SetMoveSpeed(_moveSpeed);
            //TODO 테스트

    }

    void SetMoveSpeed(float _speed)
    {
        animator.SetFloat("MoveSpeed", _speed);
    }
    void RollAni()
    {
        if(RollSt == RollSt.ROLL)
        {
            if (charController.isGrounded)
            {
                animator.Play("Roll");
                RollSt = RollSt.NONE;
            }
        }
    }
    public void AttackAni()
    {
        switch (nPlayer.job)
        {
            case Job.NONE:
                animator.Play("AttackCom");
                animator.SetFloat("AttackSeq",nPlayer.AttackBlend);
              
                break;
            case Job.WIZARD: break;
            case Job.HOKE: break;
            case Job.WORRIOR: break;
        }

    }

    void JumpingAndLanding()
    {
        if (JumpSt == JumpSt.START)
        {
            if (charController.isGrounded)
            {
                JumpSt = JumpSt.NONE;
            }
            else if (!charController.isGrounded)
            {
                JumpSt = JumpSt.JUMPING;
                AniJump();
            }
        }
        else if (JumpSt == JumpSt.JUMPING)
        {
            if (charController.isGrounded)
            {
                JumpSt = JumpSt.NONE;
                AniLand();
            }
        }
    }

    public void AniJump()
    {
        animator.Play("Jump"); 
    }

    public void AniLand()
    {
        AniCrossFade("Landing");
    }

    #endregion

    #region MonsterAni
    public void SetMonsterAni(string _rscPath)
    {
        animator.Play($"{_rscPath}");
    }
    #endregion

    #region Bool,Int,ETC
    public bool IsAttack()
    {
        if (Input.GetKey(KeyCode.LeftControl))
            return true;
        return false;
    }

    public bool IsRoll()
    {
        if (Input.GetKey(KeyCode.LeftControl))
            return true;
        return false;
    }

    #endregion
    public void AniCrossFade(string _stateName)
    {
        animator.CrossFade(_stateName, 0.1f);
    }

    void LateUpdate()
    {
        DefaultMove();
        CheckGround();

        UsingSkill();
        //Jump();
        //Roll();
        Dash();
    }

    public void SetSturn()
    {
        IsSturn = true;
        SturnTime = 0.8f;
    }
}

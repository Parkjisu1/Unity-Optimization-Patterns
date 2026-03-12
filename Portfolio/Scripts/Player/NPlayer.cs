using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Newtonsoft.Json;
using PN = Photon.Pun.PhotonNetwork;
using System.Collections;
using CodeStage.AntiCheat.ObscuredTypes;


using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Nplayer : MonoBehaviourPunCallbacks, IPunObservable
{
    public string Nickname { get; set; }
    public ObscuredInt ActorNum { get; private set; }
    public PlayerKind playerKind { get; set; }

    /// <summary> 나중에 게임을 종료 했을 떄 사용함=> 데이터 저장을 위해/// </summary>
    public PlayerInfo MyPlayerInfo; 

    private Rigidbody rigidbody;

    public bool IsNpc { get; set; }
    public ObscuredInt NpcID { get; private set; }
    public NpcKind NpcKind { get; private set; }

    public bool IsDead { get; set; }

    #region Tutorial
    #endregion

    private List<GameObject> walkDustList = new List<GameObject>();

    #region PlayerInfo In Game
    public float HP { get; set; }
    public float MP { get; set;}
    //TODO 테스트=> 공통적으로 쓰고 게임을 종료혹은 시작할떄 UserData에서 save or load를 한다
    public int LEVEL { get; set;}
    public float EXP { get; set; }
    #endregion
    #region Monster
    //TODO 테스트 :몬스터에따라 바꾸기
    public float traceDist = 10.0f;
    public float AttackDist = 3.2f;
    private NavMeshAgent nvAgent;
    public MonsterState monstate;
    public int MonsterKind { get; set; }

    public GameObject MonsterBody;
    #endregion
    #region Attack

    //TODO TEST
    [SerializeField]
    bool DebugMode = false;
    public float viewRadious = 15.0f;
    [Range(0,360)]
    public float viewAngle = 120.0f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public List<GameObject> HitTargets = new List<GameObject>();


    public double AttackCool { get; private set; }
    public bool CanAttack { get; set; }
    public bool IsAttack { get; set; }
    public float AttackBlend { get; set; }

    #endregion
    [HideInInspector]
    public Vector3 CurrPos;
    [HideInInspector]
    public Quaternion currRot;

    [Header("[NplayerMove]")]
    public NplayerMove nPlayerMove;
    [Header("[NplayerRPC]")]
    public NplayerRPC nPlayerRpc;

    [Header("[Canvas]")]
    public Canvas canvas;

    [Header("[NIckname]")]
    public Text TxtName;

    [Header("[Quest]")]
    public GameObject QuestInfoObj;
    public Image IconQuest;
    public Button BtnQuest;

    //[Header("[Player_State]")]
    //public PlayerState playerState;

    [Header("DustEff Zone")]
    public Transform DustEffZone;

    //TODO 테스트
    public Job job;

    private void Awake()
    {
        if(photonView !=null&& photonView.InstantiationData != null)
        {
            Init(photonView.InstantiationData);
        }
    }

    public void Init(object[] _instantiateData)
    {
        DistinguishPlayerKind((PlayerKind)JsonDecode.ToInt(_instantiateData[0]));

        switch ((PlayerKind)JsonDecode.ToInt(_instantiateData[0]))
        {
            case PlayerKind.PLAYER:
                object[] _playerData = new object[] { JsonDecode.ToInt(_instantiateData[0]) };
                nPlayerMove.Init(_playerData);
                job = (Job)JsonDecode.ToInt(_instantiateData[1]);
                //playerState = (PlayerState)JsonDecode.ToInt(_instantiateData[2]);
                GameManager.Instance.SecondTutorialAttackCnt = 0;
                
                SetUtilLoadData();
                break;
            case PlayerKind.NPC:
                //TODO 테스트 
                object[] _npcData = new object[] { JsonDecode.ToInt(_instantiateData[0]),NpcKind.SHOP};
                nPlayerMove.Init(_npcData);
                break;
            case PlayerKind.MONSTER:
                //TODO 테스트 :몬스터 종류
                int temp = JsonDecode.ToInt(_instantiateData[1]);
                bool Isboss = JsonDecode.ToBool(_instantiateData[2]);
                if (Isboss)
                {
                    switch (temp)
                    {
                        //TODO 테스트 
                        case (int)MapType.MY_HOME: MonsterKind = 1028; break;
                        case (int)MapType.MONSTERFIELD: MonsterKind = 1029; break;
                        case (int)MapType.MAIN_TOWN: MonsterKind = 1030; break;
                    }
                }
                else
                {
                    switch (temp)
                    {
                        //TODO 테스트 
                        case (int)MapType.MY_HOME: MonsterKind = 1001; break;
                        case (int)MapType.MONSTERFIELD: MonsterKind = 1002; break;
                        case (int)MapType.MAIN_TOWN: MonsterKind = 1003; break;
                    }
                }
                //switch (temp)
                //{
                //    //TODO 테스트 
                //    case (int)MapType.MY_HOME:MonsterKind =1001 ; break;
                //    case (int)MapType.MONSTERFIELD: MonsterKind = 1002; break;
                //    case (int)MapType.MAIN_TOWN: MonsterKind = 1003; break;
                //}

                LEVEL = DataTblMng.Instance.GetMonsterData(MonsterKind).Idx;
                HP = DataTblMng.Instance.GetMonsterData(MonsterKind).HP;

                //MonsterKind = JsonDecode.ToInt(_instantiateData[1]);
                //TODO 테스트: 위치는 랜덤으로 돌리기
                object[] _monsterData = new object[] { JsonDecode.ToInt(_instantiateData[0]),(int)MonsterState.IDLE };
                nPlayerMove.Init(_monsterData);
                nvAgent = GetComponent<NavMeshAgent>();
                //StartCoroutine(CheckState());
                //StartCoroutine(CheckStateForAction());
                break;
        }
        //TODO 테스트
        transform.localPosition = Vector3.one*2;

        //object[] _tempData = new object[] { JsonDecode.ToInt(_instantiateData[0]) };
        //nPlayerMove.Init(_tempData);

        //SecondTutorialAttackCnt = 0;
        //job = (Job)JsonDecode.ToInt(_instantiateData[1]);
        //playerState = (PlayerState)JsonDecode.ToInt(_instantiateData[2]);

        CanAttack = true;
        AttackBlend = 1;
        AttackCool = 0;

        if(photonView !=null &&photonView.IsMine)
        {
            if(IsNpc ==false)
            {
                name = "MYPlayer";

                PunManager.MyPlayer = this;
                PunManager.MyRpc = nPlayerRpc;


            }
        }
        else
        {
            if(IsNpc==false)
            {
                PunManager.Instance.AddOtherPlayer(this);
            }
        }
        //IsDead = false;

        ////TODO 테스트
        //만약 UserData에 정보가 없다고 하면 PlayerLevel 1,EXP 0 으로 고정
        //몬스터랑 같이 사용할것 
        //LEVEL = 1;
        //HP = DataTblMng.Instance.GetPlayerLevelData(LEVEL).HP;
        //MP = DataTblMng.Instance.GetPlayerLevelData(LEVEL).MP; 
        //EXP = 0;
    }

    void CheckState()
    {
        if (GameManager.Instance.nPlayer == null)
            return;

        //변경
        float dist = Vector3.Distance(GameManager.Instance.nPlayer.transform.position,transform.position);

        if (IsDead)
            return;

        if (dist <= AttackDist)
        {
            monstate = MonsterState.ATTACK;
        }
        else if(dist <= traceDist && dist >AttackDist)
        {
            monstate = MonsterState.TRACE;
        }
        else if(dist > traceDist)
        {
            monstate = MonsterState.IDLE;
        }

   
    }
    void CheckStateForAction()
    {
        if (playerKind != PlayerKind.MONSTER)
            return;

        if (IsDead)
        {
            nPlayerMove.SetDestination(transform.position);
            nPlayerMove.SetMonsterAni("Dead");
            return;
        }

        switch (monstate)
        {
            case MonsterState.IDLE:
                nvAgent.SetDestination(transform.position);
                nPlayerMove.SetMonsterAni("Idle");
                break;
            case MonsterState.TRACE:
                //변경
                nvAgent.SetDestination(HitTargets[0].gameObject.transform.position);
                //nvAgent.SetDestination(GameManager.Instance.nPlayer.transform.position);
                nPlayerMove.SetMonsterAni("Trace");
                break;
            case MonsterState.ATTACK:
                nvAgent.SetDestination(transform.position);
                nPlayerMove.SetMonsterAni("Attack");
                break;
        }
      
    }
    public void DistinguishPlayerKind(PlayerKind _kind)
    {
        switch (_kind)
        {
            case PlayerKind.PLAYER:
                QuestInfoObj.gameObject.SetActive(false);
                if(photonView !=null && photonView.IsMine)
                {
                    PunManager.MyRpc = nPlayerRpc;
                    GameManager.Instance.nPlayer = this;
                }
                break;
            case PlayerKind.NPC:
                //TODO 테스트
                QuestInfoObj.gameObject.SetActive(false);
                GameManager.Instance.NpcList.Add(this);
                break;
            case PlayerKind.MONSTER:
                QuestInfoObj.gameObject.SetActive(true);
                if (photonView != null)
                {
                    PunManager.Instance.MonsterList.Add(this);
                }
                break;
        }
        playerKind = _kind;
    }

    //public void CheckPlayerState(PlayerState _playerState)
    //{
    //    if (UserData.Instance.tutorialPass)
    //        return;

    //    if (UserData.Instance.StepOfTutorial >= (int)_playerState)
    //        playerState = _playerState;

    //}

    public void MakeWalkDust()
    {
        for (int i = 0; i < walkDustList.Count; i++)
        {
            if (walkDustList[i].activeSelf == false)
            {
                walkDustList[i].transform.position = transform.position;
                walkDustList[i].SetActive(true);

                return;
            }
        }

        UIManager.Instance.LoadPrefabAsset<GameObject>("Eff/WalkDust", LoadWalkDustAssetCom);
    }

    void LoadWalkDustAssetCom(GameObject _object, object[] _instantiateData)
    {
        GameObject _gameObject = Instantiate(_object, Vector3.zero, Quaternion.identity, transform);
        _gameObject.transform.SetParent(DustEffZone);
        walkDustList.Add(_gameObject);
    }

    public void MakeAttackEff()
    {
        //for (int i = 0; i < HitTargets.Count; i++)
        //{
            UIManager.Instance.LoadPrefabAsset<GameObject>("Eff/AttackEff", LoadAttackEffAssetCom);
        //}
    }

    void LoadAttackEffAssetCom(GameObject _object,object[] _instantiateData)
    {
        GameObject _gameObject = Instantiate(_object, Vector3.zero, Quaternion.identity, transform);
        _gameObject.transform.SetParent(DustEffZone);
        _gameObject.transform.position = transform.position;
    }


    #region UtilManagement
    public void SetUtilLoadData()
    {
        PlayerInfo _playerInfo = UserData.Instance.GetPlayerInfo();
        LEVEL = _playerInfo.Level;
        EXP = _playerInfo.EXP;

        PlayerData _temp = DataTblMng.Instance.GetPlayerLevelData(LEVEL);
      
        HP = _temp.HP;
        MP = _temp.MP;
        
    }

    public void SetUtilDataAfterLevelUp()
    {
        HP = DataTblMng.Instance.GetPlayerLevelData(LEVEL).HP;
        MP = DataTblMng.Instance.GetPlayerLevelData(LEVEL).MP;
        EXP = 0.0f;

        object[] _tempData = new object[] {LEVEL,EXP};
        UserData.Instance.UpdateUserDataByLevelUp(_tempData);
    }
    //TODO 테스트
    /// <summary>  체력 및 마나 관리하는 함수</summary>
   
     //TODO 테스트
    /// <summary>  몬스터 체력 관리하는 함수</summary>
    public void CalculateMonsterHP(object[] _data)
    {
        float AttackDam = JsonDecode.ToFloat(_data[1]);
        int LasthitPlayerNum = JsonDecode.ToInt(_data[2]);
        HP += AttackDam;
        nPlayerRpc.SendTypeRPC((int)RPCType.SHOW_ATTACK_EFF, RpcTarget.AllBuffered);

        if(HP<=0)
        {
            //MonsterState에서 DEAD는 별의미 없음
            object[] _istanceData=new object[] {(int)RPCType.DEAD,(int)MonsterState.DEAD,photonView.ViewID,LasthitPlayerNum };
            nPlayerRpc.SendRPC(_istanceData);
        }


    }

    public void LevelUP()
    {
        if (!photonView.IsMine)
            return;

        //TODO 테스트 
        LEVEL++;
        SetUtilDataAfterLevelUp();
    }

 
    #endregion
    #region MonAttack
    public void MonAttack()
    {
        MonsterData monData = DataTblMng.Instance.GetMonsterData(MonsterKind);
        for(int i= 0; i< HitTargets.Count; i++)
        {
            Nplayer nplayer = HitTargets[i].GetComponent<Nplayer>();
            Debug.Log($"<color=red>MonsterDam:{monData.AttackDam}</color>");

            //데미지 추가
            nPlayerRpc.SendRPC(new object[] {(int)RPCType.ATTACK_PLAYER,nplayer.photonView.ViewID });
            //nplayer.SetUtil(UtilKind.HP, -monData.AttackDam);
        }
    }
    #endregion

    #region Attack
    void CheckAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //자기 공격은 자기만 구동시키기
            if (!photonView.IsMine)
                return;

            if (playerKind != PlayerKind.PLAYER)
                return;

            if (CanAttack && IsNpc == false)
            {
                //TODO 테스트 AttackBlend가 1,2,3 일때의 +1값을 변경하라
                //ex)1일때는 0.9f 이런식
                float _temp = AttackBlend + 1;
                do
                {
                    AttackBlend += Time.deltaTime;
                }
                while (AttackBlend <= _temp);

                if (AttackBlend >= 4)
                {
                    AttackBlend = 1;
                }
                //공격하는건 나중에 매개 변수로 바꿔서 넣기
                AttackCool = AttackBlend<=2&&AttackBlend>=1? 1.3f: 1.4f;

                //공격력 가져오기
                PlayerData playerData = DataTblMng.Instance.GetPlayerLevelData(LEVEL);

                //범위 안에 들어온 녀석들은 전부 데미지 가감 
                for(int i= 0; i< HitTargets.Count; i++)
                {
                    Nplayer nplayer = HitTargets[i].GetComponent<Nplayer>();

                    if(!nplayer.IsDead)
                    {
                        PunManager.MyRpc.SendRPC(new object[] {(int)RPCType.ATTACK_MONSTER, nplayer.photonView.ViewID, -playerData.AttackDam,photonView.ViewID/*마지막을친 플레이어의 photonviw */});
                        Debug.Log($"<color=yellow>Target[{ nplayer.photonView.ViewID}] is UnderAttack \n Target[{ nplayer.photonView.ViewID}].HP :{nplayer.HP}</color>");
                     
                    }
                    
                }
                StartCoroutine(CheckAttackTIme(AttackCool));

                if (CanAttack)
                    StartCoroutine(Attack());
                
                //TODO 테스트
                CanAttack = false;



           
            }

        }
    }
    IEnumerator CheckAttackTIme(double _time)
    {
        yield return new WaitForSeconds((float)_time);
        CanAttack = true;
    }
    IEnumerator Attack()
    {
        nPlayerMove.AttackAni();
        yield return new WaitForSeconds(0.8f);
    }
    #endregion

    #region Skill
    void CheckSkill()
    {
        if(Input.GetKeyDown(KeyCode.A))
        {
            if(playerKind==PlayerKind.PLAYER)
            {
                if(photonView.IsMine)
                {
                    //nPlayerMove.StopNavMeshAgent();
                }
            }
          
            //nPlayerMove.SetDestination(this.transform.position +Vector3.up);
        }
    }
    #endregion
    #region AfterDead
    public void AfterMonsterDead()
    {
        StartCoroutine(Respawn(2f));
    }
    IEnumerator Respawn(float _time)
    {
        yield return new WaitForSeconds(_time);
        initMonsterInfo();

        
    }

    public void initMonsterInfo()
    {
        IsDead = false;
        MonsterBody.gameObject.SetActive(true);
        HP = DataTblMng.Instance.GetMonsterData(MonsterKind).HP;
        Debug.Log($"<color=green>{HP}</color>");
    }
    #endregion

    #region NoneVoid

    public bool CheckThirdTutorial()
    {
        return false;
    }

    //public float GetHp()
    //{
    //    return HP;
    //}
    #endregion

    void SerializeTransform()
    {
        if(IsNpc ==false && photonView.IsMine ==false&&PN.IsConnected)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, CurrPos, Time.deltaTime * 15.5f);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, currRot, Time.deltaTime * 15.5f);
        }
    }

    private void LateUpdate()
    {
        CheckState();
        CheckStateForAction();
        CheckAttack();

        CheckSkill();

        SerializeTransform();
        canvas.transform.LookAt(CineCam.Instance.CineCamera.transform);
    }


    private void OnDrawGizmos()
    {
        if (!DebugMode)
            return;

        Vector3 myPos = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawWireSphere(myPos, viewRadious);

        float lookingAngle = transform.eulerAngles.y;
        Vector3 rightDIr = AngleToDir(transform.eulerAngles.y + viewAngle * 0.5f);
        Vector3 leftDIr = AngleToDir(transform.eulerAngles.y - viewAngle * 0.5f);
        Vector3 lookDIr = AngleToDir(lookingAngle);

        Debug.DrawRay(myPos, rightDIr * viewRadious, Color.blue);
        Debug.DrawRay(myPos, leftDIr * viewRadious, Color.blue);
        Debug.DrawRay(myPos, lookDIr * viewRadious, Color.cyan);

        HitTargets.Clear();
        Collider[] Targets = Physics.OverlapSphere(myPos, viewRadious, targetMask);

        if (Targets.Length == 0)
            return;

        foreach(Collider EnemyColl in Targets)
        {
            Vector3 targetPos = EnemyColl.transform.position;
            Vector3 targetDir = (targetPos = myPos).normalized;
            float targetAngle = Mathf.Acos(Vector3.Dot(lookDIr, targetDir)) * Mathf.Rad2Deg;
            if(targetAngle <= viewAngle *0.5f &&!Physics.Raycast(myPos,targetDir,viewRadious,obstacleMask))
            {
                HitTargets.Add(EnemyColl.gameObject);

                if (DebugMode)
                    Debug.DrawLine(myPos, targetPos, Color.red);
            }
        }
    }

    Vector3 AngleToDir(float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));
    }

    Vector3 serializerRot;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        Debug.LogError($"OnPhotonSerializeView :{transform.position}");
        
        if (IsNpc)
            return; 

        if (stream.IsWriting)
        {
            if (photonView.IsMine && PN.IsConnected)
            {
                stream.SendNext(transform.localPosition.x);
                stream.SendNext(transform.localPosition.y);
                stream.SendNext(transform.localPosition.z);

                serializerRot = transform.localRotation.eulerAngles;
                stream.SendNext(serializerRot.x);
                stream.SendNext(serializerRot.y);
                stream.SendNext(serializerRot.z);
            }
        }
        else
        {
            if (photonView.IsMine == false && PN.IsConnected)
            {
                CurrPos = new Vector3(JsonDecode.ToFloat(stream.ReceiveNext()), JsonDecode.ToFloat(stream.ReceiveNext()), JsonDecode.ToFloat(stream.ReceiveNext()));

                serializerRot = new Vector3(JsonDecode.ToFloat(stream.ReceiveNext()), JsonDecode.ToFloat(stream.ReceiveNext()), JsonDecode.ToFloat(stream.ReceiveNext()));
                currRot = Quaternion.Euler(serializerRot);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

using Photon.Pun;
using Photon.Realtime;
using PN = Photon.Pun.PhotonNetwork;
using Newtonsoft.Json;
//SceneChgLoadObjWaitCnt
public class PunManager : MonoBehaviourPunCallbacks
{
    public static PunManager Instance;
    public  static Nplayer MyPlayer { get; set; }
    public static NplayerRPC MyRpc { get; set; }
    public Dictionary<int, Nplayer> OtherPlayer { get; set; }

    public byte UserNum = 5;


    #region MonsterPool
    public List<Nplayer> MonsterList = new List<Nplayer>();
    #endregion

    [SerializeField]
    private bool connect = false;

    private void Awake()
    {
        Instance = this;
        PhotonServerSetting();
    }
    private void Start()
    {
        //게임 시작전 플레이어의 정보 Load
        GameManager.Instance.LoadUserData();
    }

    public void PhotonServerSetting()
    {
        AppSettings _pnAppSettings = PN.PhotonServerSettings.AppSettings;

        _pnAppSettings.AppIdRealtime = "8c42ded6-adcc-41bb-b45c-9e9962fe898a";
        _pnAppSettings.UseNameServer = true;
        _pnAppSettings.EnableProtocolFallback = true;

        _pnAppSettings.Server = string.Empty;
        _pnAppSettings.Port = 0;

        _pnAppSettings.AppVersion = "1.0.0";
        _pnAppSettings.FixedRegion = "kr";

        PN.PhotonServerSettings.DevRegion = string.Empty;

        Debug.Log("<color=red>PhotonServerSetting is Complete</color>");

        Connect();
    }

    public void Connect()
    {
        PN.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("<color=red>Connecting Server Is Complete</color>");
        connect = true;
    }

    public void Disconnect()
    {
        PN.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("<color=red>Disconnecting Server Is Complete</color>");
    }

    public override void OnJoinedLobby()
    {
        if (connect)
            JoinMyHome();
    }

    public void JoinMyHome()
    {
        byte _maxPlayers = UserNum;
        PN.JoinOrCreateRoom($"MY_HOME_{UserNum}", new RoomOptions { MaxPlayers = _maxPlayers },null);
        
        object[] _data = new object[] { (int)MapType.MY_HOME };
        MapManager.Instance.Init(_data);
    }

    public override void OnJoinedRoom()
    {
        PN.LoadLevel("MY_HOME");
        Debug.Log($"<color=cyan>==== OnJoinedRoom() roomname :{PN.CurrentRoom.Name}</color>");
        //mapinfo 에 있는거 사용해서 플레이어 소환
        ////플레이어 소환
        //Debug.LogError("Game Start");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        JoinMyHome();
    }

    public void CheckCurrentRoomFirstPlayer()
    {
        SpawnPlayer();
    }

    Nplayer SpawnPlayer()
    {
        string _prefabName = "Player";

        Vector3 _pos = new Vector3(0, 2f, 0);
        Vector3 _rot = Vector3.zero;

        string _pathName = $"1_Prefab/Player/Player/{_prefabName}";

        object[] _tempData = new object[] { (int)PlayerKind.PLAYER, (int)UserData.Instance.job, (int)PlayerState.NONE };

        GameObject _player = PN.Instantiate(_pathName, _pos, Quaternion.Euler(_rot), 0, null);
        Nplayer _nplayer = _player.GetComponent<Nplayer>();

        _nplayer.Init(_tempData);
        //TODO 테스트 
        if(_nplayer.photonView.IsMine)
        {
            MyPlayer = _nplayer;
            UIManager.Instance.LoadDefaultUI();

        }
        else
        {
            //다른 사람들의 정보를 더함
            OtherPlayer.Add(_nplayer.photonView.ViewID,_nplayer);
            //OtherPlayer.Add(_nplayer.ActorNum, _nplayer);
        }
        return null;
    }

    void SpawnPlayerComplete(GameObject _gameObject)
    {
        MyPlayer = _gameObject.GetComponent<Nplayer>();
        CurrentRoomFistPlayerToLoadObj(MyPlayer);
    }

    public void CurrentRoomFistPlayerToLoadObj(Nplayer _nPlayer)
    {
        if (_nPlayer != null && _nPlayer.photonView)
            UIManager.Instance.LoadDefaultUI();
    }

    public void SpawnMonsters()
    {
        //IsMasterClient 추가
        if (IsMasterClient())
            MyRpc.SendTypeRPC((int)RPCType.SPAWN_MONSTER, RpcTarget.AllBuffered);
        //SpawnMonster();
    }
   
    public Nplayer SpawnMonster()
    {

        string _preName = MapManager.Instance.mapType.ToString();

        Vector3 _pos = new Vector3(Random.Range(-5, 5), 2f, Random.Range(-5, 5));
        Vector3 _rot = Vector3.zero;

        string _pathName = $"1_Prefab/Monster/Monster_{_preName}";

        object[] _tempData = new object[] { (int)PlayerKind.MONSTER, (int)MapType.MY_HOME, false };

        GameObject _monster = PN.Instantiate(_pathName, _pos, Quaternion.Euler(_rot), 0, null);
        Nplayer _nplayer = _monster.GetComponent<Nplayer>();

        _nplayer.Init(_tempData);
        return null;
    }

    public void SpawnBossMonster()
    {
        MyRpc.SendTypeRPC((int)RPCType.SPAWN_BOSS_MONSTER, RpcTarget.AllBuffered);
    }

    public Nplayer SpwawnBossMonster()
    {
        string _preName = "Monster_Boss";

        Vector3 _pos = new Vector3(20, 2f, 30);
        Vector3 _rot = Vector3.zero;

        string _pathName = $"1_Prefab/Monster/{_preName}";

        object[] _temmpData = new object[] { (int)PlayerKind.MONSTER, (int)MapType.MY_HOME, true };
        GameObject _bossMonster = PN.Instantiate(_pathName, _pos, Quaternion.Euler(_rot), 0, null);
        Nplayer _nplayer = _bossMonster.GetComponent<Nplayer>();

        _nplayer.Init(_temmpData); ;

        return null;
    }
    public Room CurrentRoom
    {
        get
        {
            return PN.CurrentRoom;
        }
    }

    public int PlayerCnt()
    {
        if (PN.CurrentRoom == null)
            return 0;

        return PN.CurrentRoom.PlayerCount;
    }

    public void RoomClose()
    {
        PN.CurrentRoom.IsOpen = false;
    }

    public bool IsMasterClient()
    {
        if (MyPlayer == null)
            return false;

        return MyPlayer.photonView.Owner.IsMasterClient;
    }

    public void CheckMonsterState()
    {
        StartCoroutine(ReSpawnMonster());
        //for(int i= 0; i < MonsterList.Count; i++)
        //{
        //    if(MonsterList[i].IsDead)
        //    {
        //        //부활= 활성화
        //        MonsterList[i].gameObject.SetActive(true);
        //        MonsterList[i].nPlayerRpc.SendRPC(new object[] { (int)RPCType.RESPAWN_MONSTER, MonsterList[i].photonView.ViewID });
        //    }
        //}
    }

    IEnumerator ReSpawnMonster()
    {
        yield return new WaitForSeconds(5f);
        for (int i = 0; i < MonsterList.Count; i++)
        {
            if (MonsterList[i].IsDead)
            {
                //부활= 활성화
                MonsterList[i].gameObject.SetActive(true);
                MonsterList[i].nPlayerRpc.SendRPC(new object[] { (int)RPCType.RESPAWN_MONSTER, MonsterList[i].photonView.ViewID });
            }
        }
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

using Photon.Pun;
using Photon.Realtime;
using PN = Photon.Pun.PhotonNetwork;
using Newtonsoft.Json;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ExitGames.Client.Photon;


public enum IslandType
{
    ISLAND_PLAYER = 0,
    AQUARIUM = 1,
    BATTLE_ROYAL = 2,
    FOREST_SILENCE = 3,
    MY_HOME = 4,
    CAMPING = 5,
  
    ISLAND_CENTER = 7,
   // BATTLE_ROYAL_SNOW_FIGHT=8,

    ISLAND_OTHER_PLAYER = 90,

    NONE = 99,
}

public class PunManager : MonoBehaviourPunCallbacks
{
    public static PunManager Instance;
    public static NPlayer MyPlayer { get; set; }
    public static NPlayerRPC MyRPC { get; set; }
    public Dictionary<int, NPlayer> OtherPlayer { get; set; }

    /// <summary> Dic(NpcID, NPlayer) </summary>
    public Dictionary<int, NPlayer> NpcPlayer { get; set; }

    public Vector3 MyIslandPos { get; private set; }
    public Quaternion MyIslandRot { get; private set; }

//    public ObscuredBool IsOtherIslandLoad { get; private set; }
    public string IslandName { get; private set; }
    private IslandType islandType;
    public IslandType IslandType
    {
        get { return islandType; }
        set
        {
            islandType = value;
            CineCam.Instance.SetRoomCameraType();
        }
    }

    private IslandType prevIslandType;
    public IslandType PrevIslandType
    {
        get
        {
            if (prevIslandType == IslandType.NONE)
                return IslandType.ISLAND_CENTER;
            else
                return prevIslandType;
        }
        set { prevIslandType = value; }
    }

    public int PlayRoomNo { get; set; }

    public ObscuredInt HomeFloorNo { get; private set; }
    public ObscuredInt HomeRoomNo { get; private set; }

    private int installedItemCnt = -1;

    public bool IsOtherConnectIsland { get; set; }
    private List<Player> otherPlayerEnterRoomList = new List<Player>();

    private Queue<EventData> assetLoadEventList = new Queue<EventData>();
    private int myPlayerLoadCompleteCnt = 0;

    public int SceneChgLoadObjWaitCnt { get; set; }

    public Vector3 tempPos { get; set; }

//    public DefaultDel miniGamePlayerConnectCheckDel = null;

    void Awake()
    {
        Instance = this;
        IsOtherConnectIsland = false;
        PrevIslandType = IslandType.NONE;
    }

    public void Init()
    {
        OtherPlayer = new Dictionary<int, NPlayer>();
        NpcPlayer = new Dictionary<int, NPlayer>();
        MyIslandPos = Vector3.zero;
    }

    public void PhotonServerSetting()
    {
        AppSettings _pnAppSettings = PN.PhotonServerSettings.AppSettings;

#if UNITY_EDITOR
        
        _pnAppSettings.AppIdRealtime = "9300af45-70fa-439c-b671-97b4fab5ac78";
        _pnAppSettings.AppIdChat = "e4652687-76e6-47c6-b406-73d7c50a16c9";
        _pnAppSettings.AppIdVoice = "14085181-4eca-43c6-ba18-8a44d8b4d9fb";
        _pnAppSettings.UseNameServer = true;
        _pnAppSettings.EnableProtocolFallback = true;

        _pnAppSettings.Server = string.Empty;
        _pnAppSettings.Port = 0;

/*        
        _pnAppSettings.AppIdRealtime = string.Empty;
        //        _pnAppSettings.AppIdChat = string.Empty;
        _pnAppSettings.AppIdChat = "e4652687-76e6-47c6-b406-73d7c50a16c9";
        _pnAppSettings.AppIdVoice = string.Empty;
        _pnAppSettings.UseNameServer = false;
        _pnAppSettings.EnableProtocolFallback = false;

        _pnAppSettings.Server = "112.185.203.223";
        _pnAppSettings.Port = 25055;
*/        
#else
/*
        _pnAppSettings.AppIdRealtime = string.Empty;
        _pnAppSettings.AppIdChat = string.Empty;
        _pnAppSettings.AppIdVoice = string.Empty;
        _pnAppSettings.UseNameServer = false;
        _pnAppSettings.EnableProtocolFallback = false;

        _pnAppSettings.Server = "112.185.203.223";
        _pnAppSettings.Port = 25055;
*/
#endif

        _pnAppSettings.AppVersion = "1.0.0";
        _pnAppSettings.FixedRegion = "kr";

        PN.PhotonServerSettings.DevRegion = string.Empty;
    }

    public string GetPhotonServerRegion()
    {
        return PN.PhotonServerSettings.DevRegion;
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    public void Disconnect()
    {
        PN.Disconnect();
    }

    public void AddOtherPlayer(NPlayer _nPlayer)
    {
        if (OtherPlayer.ContainsKey(_nPlayer.ActorNum) == false)
        {
            OtherPlayer.Add(_nPlayer.ActorNum, _nPlayer);
        }
    }

    void RemoveOtherPlayer(int _actorNum)
    {
        if (OtherPlayer.ContainsKey(_actorNum))
            OtherPlayer.Remove(_actorNum);
    }

    public NPlayer GetOtherPlayer(int _actorNum)
    {
        if (OtherPlayer.ContainsKey(_actorNum))
            return OtherPlayer[_actorNum];

        return null;
    }

    public List<NPlayer> GetOtherPlayerList()
    {
        if (OtherPlayer == null)
            return null;

        return new List<NPlayer>(OtherPlayer.Values);
    }

    public List<string> GetOtherPlayerNickNameList()
    {
        List<string> _nickList = new List<string>();
        var _aa = OtherPlayer.GetEnumerator();
        while(_aa.MoveNext())
        {
            _nickList.Add(_aa.Current.Value.Nickname);
        }
        return _nickList;
    }

    public NPlayer GetPlayerInfo(int _actorNum)
    {
        if (MyPlayer.ActorNum == _actorNum)
            return MyPlayer;

        return GetOtherPlayer(_actorNum);
    }

    public bool IsMyPlayer(int _actorNum)
    {
        return MyPlayer.ActorNum == _actorNum;
    }

    public List<NPlayer> GetPlayerList()
    {
        List<NPlayer> _playerList = new List<NPlayer>(OtherPlayer.Values);
        _playerList.Add(MyPlayer);

        return _playerList;
    }

    //public List<int> PlayerActorNumList()
    //{
    //    return new List<int>(PN.CurrentRoom.Players.Keys);
    //}

    public void AddNPCPlayer(NPlayer _nPlayer)
    {
        if (NpcPlayer.ContainsKey(_nPlayer.NpcID) == false)
            NpcPlayer.Add(_nPlayer.NpcID, _nPlayer);
    }

    public void SetIslandName(string _islandName)
    {
        IslandName = _islandName;
    }

    void LateUpdate()
    {
        //if (miniGamePlayerConnectCheckDel != null)
        //    miniGamePlayerConnectCheckDel();

        if (otherPlayerEnterRoomList.Count <= 0)
            return;

        if(MyPlayer != null)
        {
            for (int i = 0; i < otherPlayerEnterRoomList.Count; i++)
                MyPlayer.OtherPlayerEnterRoom(otherPlayerEnterRoomList[i]);
            otherPlayerEnterRoomList.Clear();
        }
    }

#region Pun
    public void ConnectServer(string _nickname, string _islandName)
    {
        MyDebug.Log(TextData.Red("==== ConnectServer()"));
        IslandName = _islandName;

        PN.ConnectUsingSettings();
        PN.NickName = string.IsNullOrEmpty(_nickname) ? string.Format("Player_{0}", Random.Range(1, 100)) : _nickname;
    }

    public override void OnDisconnected(DisconnectCause _cause)
    {
        UINetConnect.Instance.Show(false);

        if (UIManager.Instance.IsOpenUI<UITitle>())
            UITitle.Instance.BtnStartView(true);
    }

    public override void OnConnectedToMaster()
    {
        MyDebug.LogWarning(TextData.Green("==== OnConnectedToMaster()"));
        OtherPlayer.Clear();
        NpcPlayer.Clear();

        OnJoinedLobby();
    }

    public string GetRoomName()
    {
        string _roomName = string.Empty;

        switch (IslandType)
        {
            case IslandType.ISLAND_PLAYER:
                _roomName = IslandName;
                break;
            case IslandType.AQUARIUM:
                _roomName = string.Format(TextData.StrFormat[1], IslandName, "Aquarium");
                break;
            case IslandType.MY_HOME:
                _roomName = string.Format(TextData.StrFormat[20], IslandName, "MyHome_", HomeFloorNo, HomeRoomNo);
                break;

            // 고요의숲, 캠핑은 채팅 채널명으로만 사용
            case IslandType.FOREST_SILENCE:
            case IslandType.CAMPING:
            case IslandType.ISLAND_CENTER:

                _roomName = CurrentRoom.Name;
                break;
        }

        return _roomName;
    }

    public override void OnJoinedLobby()
    {
        MyDebug.LogWarning(TextData.Green("==== OnJoinedLobby()"));
        switch (IslandType)
        {
            case IslandType.FOREST_SILENCE: JoinForestSilence(); break;
            case IslandType.CAMPING: JoinCamping(); break;
            case IslandType.CHO_SEOK_TOWN: JoinChoSeokTown(); break;
            case IslandType.BATTLE_ROYAL:JoinPlayRoom();break;
            case IslandType.ISLAND_CENTER: JoinIslandCenter(); break;
            default:
                {
                    byte _maxPlayers = 0;

                    if (IslandType == IslandType.ISLAND_PLAYER)
                        _maxPlayers = (byte)DataTblMng.Instance.GetConfigData(ConfigType.ISLAND_MAX_PLAYER);
                    else if (IslandType == IslandType.AQUARIUM)
                        _maxPlayers = (byte)DataTblMng.Instance.GetConfigData(ConfigType.ISLAND_AQUARIUM_MAX_PLAYER);
                    else if (IslandType == IslandType.MY_HOME)
                        _maxPlayers = (byte)DataTblMng.Instance.GetConfigData(ConfigType.ISLAND_MY_HOME_MAX_PLAYER);

#if UNITY_EDITOR && !UNITY_ANDOROID
                    _maxPlayers = 20;
#endif

                    RoomOptions _roomOption = new RoomOptions { MaxPlayers = _maxPlayers };

                    PN.JoinOrCreateRoom(GetRoomName(), _roomOption, null);
                }
                break;
        }
    }

    void JoinPlayRoom()
    {
        byte _maxPlayers = (byte)DataTblMng.Instance.GetMiniGameMaxPlayer(MiniGameManager.Instance.MiniGameType, MiniGameManager.Instance.MiniGameKind);
#if UNITY_EDITOR && !UNITY_ANDOROID
        _maxPlayers = 20;
#endif

        PN.JoinOrCreateRoom($"PlayRoom_{MiniGameManager.Instance.MiniGameKind}_{PlayRoomNo}", new RoomOptions { MaxPlayers = _maxPlayers }, null);
    }
    void JoinForestSilence()
    {
        byte _maxPlayers = (byte)DataTblMng.Instance.GetConfigData(ConfigType.ISLAND_FOREST_SILENCE_MAX_PLAYER);
#if UNITY_EDITOR && !UNITY_ANDOROID
        _maxPlayers = 20;
#endif
        PN.JoinOrCreateRoom($"ForestSilence_{PlayRoomNo}", new RoomOptions { MaxPlayers = _maxPlayers }, null);
    }

    void JoinCamping()
    {
        byte _maxPlayers = (byte)DataTblMng.Instance.GetConfigData(ConfigType.ISLAND_CAMPING_MAX_PLAYER);
#if UNITY_EDITOR && !UNITY_ANDOROID
        _maxPlayers = 20;
#endif
        PN.JoinOrCreateRoom($"Camping_{PlayRoomNo}", new RoomOptions { MaxPlayers = _maxPlayers }, null);
    }

    void JoinChoSeokTown()
    {
        byte _maxPlayers = (byte)DataTblMng.Instance.GetConfigData(ConfigType.CHO_SEOK_TOWN_MAX_PLAYER);
#if UNITY_EDITOR && !UNITY_ANDOROID
        _maxPlayers = 20;
#endif

        string _roomName = $"ChoSeokTown_{MiniGameManager.Instance.MiniGameKind}_{PlayRoomNo}";
        MyDebug.Log(TextData.Yellow("==== JoinChoSeokTown() roomName : " + _roomName));

        //        PN.JoinOrCreateRoom($"ChoSeokTown_{MiniGameManager.Instance.GameNo}", new RoomOptions { MaxPlayers = _maxPlayers }, null);
        PN.JoinOrCreateRoom($"ChoSeokTown_{MiniGameManager.Instance.MiniGameKind}_{PlayRoomNo}", new RoomOptions { MaxPlayers = _maxPlayers }, null);
    }

    void JoinIslandCenter()
    {
        byte _maxPlayers = (byte)DataTblMng.Instance.GetConfigData(ConfigType.ISLAND_CENTER_MAX_PLAYER);
#if UNITY_EDITOR && !UNITY_ANDOROID
        _maxPlayers = 20;
#endif
        PN.JoinOrCreateRoom($"IslandCenter_{PlayRoomNo}", new RoomOptions { MaxPlayers = _maxPlayers }, null);

    }

    public override void OnJoinedRoom()
    {
        MyDebug.LogWarning(TextData.Green("==== OnJoinedRoom() roomname : "+PN.CurrentRoom.Name));

        switch (IslandType)
        {
            case IslandType.ISLAND_PLAYER:
            case IslandType.ISLAND_CENTER:
                if (IslandType == IslandType.ISLAND_PLAYER)
                {
                    PN.LoadLevel("IslandPlayer");
                }
                else
                {
#if UNITY_EDITOR
                    if(Main.Instance.TestSetting.IsRenewalIslandConn)
                        PN.LoadLevel("RenewalIslandCenter");
                    else
#endif
                        PN.LoadLevel("IslandCenter");
                }
                break;

            case IslandType.AQUARIUM: PN.LoadLevel("Aquarium"); break;
            case IslandType.FOREST_SILENCE: PN.LoadLevel("ForestSilence"); break;
            case IslandType.MY_HOME: PN.LoadLevel("MyHome"); break;
          
            case IslandType.BATTLE_ROYAL: PN.LoadLevel("BattleRoyal"); break;

            case IslandType.CAMPING:
                CampingManager.Instance.Init();
                PN.LoadLevel("Camping");
                break;
        }

        ChattingManager.Instance.Connect();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        MyDebug.LogWarning(TextData.Green("==== OnJoinRoomFailed() returnCode : " + returnCode+", message : "+message));
        switch (IslandType)
        {
            case IslandType.BATTLE_ROYAL:
            case IslandType.FOREST_SILENCE:
            case IslandType.CAMPING:
            case IslandType.CHO_SEOK_TOWN:
            case IslandType.ISLAND_CENTER:
                PlayRoomNo++;

                if (IslandType == IslandType.BATTLE_ROYAL)
                    JoinPlayRoom();
                else if (IslandType == IslandType.FOREST_SILENCE)
                    JoinForestSilence();
                else if (IslandType == IslandType.CAMPING)
                    JoinCamping();
            
                else if (IslandType == IslandType.ISLAND_CENTER)
                    JoinIslandCenter();
                break;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        MiniGameManager.Instance.OtherPlayerEnterRoom(newPlayer);
    }

    void PlayerEnteredRoom()
    {
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        MyDebug.LogWarning(TextData.Green("==== OnPlayerLeftRoom() actorNum : " + otherPlayer.ActorNumber + ", name : " + otherPlayer.NickName));
        RemoveOtherPlayer(otherPlayer.ActorNumber);
        MiniGameManager.Instance.OtherPlayerLeftRoom(otherPlayer);
        CampingManager.Instance.OtherPlayerLeftRoom(otherPlayer);
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
    }

    public bool IsMasterClient()
    {
        if (MyPlayer == null)
            return false;

        return MyPlayer.photonView.Owner.IsMasterClient;
    }

    public bool IsMasterClient(int _actorNum)
    {
        if (MyPlayer == null)
            return false;

        if (MyPlayer.photonView.Owner.ActorNumber == _actorNum && IsMasterClient())
            return true;

        return false;
    }

    public void SetMasterClient()
    {
        if(IsMasterClient() == false)
            PN.SetMasterClient(MyPlayer.photonView.Owner);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // OnPlayerLeftRoom 에서 OtherPlayerLeftRoom()을 호출하는데 마스터 클라이언트가 아직 안바뀌어서 CheckPlayerCount() 호출 안될수도 있어서
        // 마스터 클라 바뀌면 한번 호출
        if (MyPlayer != null && newMasterClient.ActorNumber == MyPlayer.ActorNum)
            MiniGameManager.Instance.OtherPlayerLeftRoom(null);
    }

    public override void OnLeftRoom()
    {
        CameraManager.Instance.CineCam.EnableAudioListener(true);

        // 다른 씬(고요의 숲, 아쿠아리움, 배틀로얄) 들어갈때 타이틀로 빠져버려서 if 추가
        if(IsOtherConnectIsland == false)
        {
            CameraManager.Instance.CineCam.CamEnable(false);
            CameraManager.Instance.UICamAudioEnable(true);

            Main.Instance.RestartGame();
        }
    }

    public string CurrentRoomName()
    {
        if (PN.CurrentRoom == null)
            return string.Empty;

        return PN.CurrentRoom.Name;
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

    public bool IsInRoom()
    {
        return PN.InRoom;
    }

    public bool IsMyRoom()
    {
        if (MyPlayer == null)
            return false;

        if((NetManager.Instance.UserNo != UserInfoManager.Instance.VisitIslandOtherUserSidx && UserInfoManager.Instance.VisitIslandOtherUserSidx > -1)
            || (islandType == IslandType.AQUARIUM && CurrentRoomName() != string.Format(TextData.StrFormat[1], MyPlayer.photonView.Owner.NickName, "Aquarium")))
            return false;

        return true;
    }

    public int RoomMasterClient()
    {
        return PN.CurrentRoom.MasterClientId;
    }

    public void RoomClose()
    {
        PN.CurrentRoom.IsOpen = false;
    }
#endregion

#region Connect Other Room
    void SaveMyIslandPosRotInfo()
    {
        //MyIslandPos = MyPlayer.transform.localPosition + new Vector3(0, 1, 0);
        //MyIslandRot = MyPlayer.transform.localRotation;

        MyDebug.LogWarning(TextData.Red("==== SaveMyIslandPosRotInfo()"));
    }

    public void ConnectOtherIsland(IslandType _otherIslandType, bool _savePos = false)
    {
        MyDebug.Log(TextData.Red("==== ConnectOtherIsland() otherIslandType : " + _otherIslandType + ", savePos : " + _savePos));

        if (IslandType != IslandType.BATTLE_ROYAL && ((_otherIslandType == IslandType.BATTLE_ROYAL && _savePos) || _otherIslandType != IslandType.ISLAND_PLAYER))
            SaveMyIslandPosRotInfo();

        if(islandType == IslandType.BATTLE_ROYAL)
            PrevIslandType = PrevIslandType;
        else
            PrevIslandType = IslandType;
      
        IslandType = _otherIslandType;

        if (IslandType == IslandType.ISLAND_OTHER_PLAYER)
        {
            IslandType = IslandType.ISLAND_PLAYER;
        }
        else if(IslandType != IslandType.MY_HOME && IslandType != IslandType.AQUARIUM)
        {
            UserData.Instance.InitOtherUserInstalledDecoDic();
            UserInfoManager.Instance.VisitIslandOtherUserSidx = -1;
        }

//        IsOtherIslandLoad = true;

        PlayRoomNo = 1;

#if UNITY_EDITOR
        if (Main.Instance.TestSetting.IslandNo > 0)
            PlayRoomNo = Main.Instance.TestSetting.IslandNo;
#endif
        MiniGameManager.Instance.GameEndToInitList();

//        if(islandType != IslandType.CHO_SEOK_TOWN)
            MiniGameManager.Instance.SetState(MiniGameSt.NONE);

        IsOtherConnectIsland = true;

        CheckOtherConnectIsland();
    }

    public void CheckOtherConnectIsland()
    {
        MyDebug.Log(TextData.Green("==== CheckOtherConnectIsland()"));
        if (IsOtherConnectIsland == false)
            return;

        PN.LeaveRoom();

        MyRPC = null;
        MyPlayer = null;
    }

    public void SetHomeRoomInfo(int _floorNo, int _roomNo)
    {
        HomeFloorNo = _floorNo;
        HomeRoomNo = _roomNo;
    }
#endregion

    public void PlayerCanvasShow(bool _isShow)
    {
        MyPlayer.CanvasTran.gameObject.SetActive(_isShow);

        var _otherPlayerDic = OtherPlayer.GetEnumerator();
        while (_otherPlayerDic.MoveNext())
            _otherPlayerDic.Current.Value.CanvasTran.gameObject.SetActive(_isShow);

        var _npcDic = NpcPlayer.GetEnumerator();
        while (_npcDic.MoveNext())
            _npcDic.Current.Value.CanvasTran.gameObject.SetActive(_isShow);
    }

    public void CheckCurrentRoomFirstPlayer()
    {
        SpawnPlayer();
    }

    NPlayer SpawnPlayer()
    {
        string _prefabName = "WPlayer";
        if (UserData.Instance.Gender == Gender.MAN)
            _prefabName = "MPlayer";

        
        Vector3 _pos = new Vector3(0, 2f, 0);
        Vector3 _rot = Vector3.zero;

        GetPlayerStartPos(ref _pos, ref _rot);

        string _pathName = $"Prefabs/Player/{_prefabName}";

        PunInstantiateEquipCostumeData _punCostumeData = new PunInstantiateEquipCostumeData(UserData.Instance.GetEquipCostumeDic());
        string _costumeDataStr = JsonConvert.SerializeObject(_punCostumeData);

        object[] _instantiateData = new object[]
                                    { false
                                        , 0
                                        , (int)UserData.Instance.Gender
                                        , (int)UserData.Instance.ProfileImgNo
                                        , (int)UserData.Instance.ProfileBgNo
                                        , (int)NetManager.Instance.UserNo
                                        , _pos
                                        , _rot
                                        , 0
                                        , _costumeDataStr};

        PN.InstantiateAsset(_pathName, _pos, Quaternion.Euler(_rot), 0, _instantiateData, SpawnPlayerComplete);

        return null;
    }

    void SpawnPlayerComplete(GameObject _gameObject)
    {
        MyPlayer = _gameObject.GetComponent<NPlayer>();
        CurrentRoomFistPlayerToLoadObj(MyPlayer);
    }

    public void CurrentRoomFistPlayerToLoadObj(NPlayer _nPlayer)
    { 
        if (_nPlayer != null && _nPlayer.photonView.IsMine)
            UIManager.Instance.LoadDefaultUI();

        IsOtherConnectIsland = false;

        switch (IslandType)
        {
            case IslandType.ISLAND_CENTER:
                if (CurrentRoom.PlayerCount <= 1)
                {
                    ProductionManager.Instance.LoadMudGarbage();
                }

                SpawnNpcPlayer();
                LoadInstalledDecoItemAll();

                break;

            case IslandType.AQUARIUM:
                if (CurrentRoom.PlayerCount <= 1)
                    LoadFish();
                else
                    Invoke("IslandSceneLoadComplete", 0.5f);

                break;

            case IslandType.BATTLE_ROYAL:
           
            case IslandType.ISLAND_PLAYER:
            case IslandType.FOREST_SILENCE:
            case IslandType.MY_HOME:
            case IslandType.CAMPING:

                LoadInstalledDecoItemAll();
                break;

         
        }
    }

    bool IsPlayerReloadCostumeFinish()
    {
        if (MyPlayer == null || MyPlayer.skinnMechCombinerFinish == false)
            return false;

        if (OtherPlayer == null)
            return false;

        List<NPlayer> _otherPlayer = new List<NPlayer>(OtherPlayer.Values);
        for(int i=0; i<_otherPlayer.Count; i++)
        {
            if (_otherPlayer[i].skinnMechCombinerFinish == false)
                return false;
        }

        return true;
    }

//    void CheckMiniGamePlayerConnect()
//    {
//        MyDebug.LogWarning("==== otherPlayer.Count:" + OtherPlayer.Count+ ", GamePlayerInfoDic.count : " + MiniGameManager.Instance.GameAcceptPlayerInfoCnt()+", ReloadCostumeFinish : "+IsPlayerReloadCostumeFinish());
//        if(OtherPlayer.Count + 1 >= MiniGameManager.Instance.GameAcceptPlayerInfoCnt() && IsPlayerReloadCostumeFinish())
//        {
//            miniGamePlayerConnectCheckDel = null;
////            MiniGameManager.Instance.ChkGameMaster();

//            MyRPC.SendTypeRPC((int)RPCType.MINIGAME_SCENE_LOAD_COM);
//        }
//    }

    public BuildLoc IslandTypeToBuildLoc()
    {
        switch(IslandType)
        {
            case IslandType.ISLAND_PLAYER: return BuildLoc.ISLAND_PLAYER;
            case IslandType.AQUARIUM: return BuildLoc.AQUARIUM;
            case IslandType.FOREST_SILENCE: return BuildLoc.FOREST_SILENCE;
            case IslandType.MY_HOME: return BuildLoc.MY_HOME;
            case IslandType.CAMPING: return BuildLoc.CAMPING;
            case IslandType.CHO_SEOK_TOWN: return BuildLoc.CHO_SEOK_TOWN;            
            case IslandType.ISLAND_CENTER: return BuildLoc.ISLAND_CENTER;
        }

        return BuildLoc.MAX;
    }

    /// <summary> 설치된 데코 아이템 불러오기 </summary>
    public void LoadInstalledDecoItemAll()
    {
        List <InvenData> _installedItemList = new List<InvenData>();
        _installedItemList = UserData.Instance.GetInstalledItemLIst(IslandTypeToBuildLoc());

        if(_installedItemList != null)
            installedItemCnt = _installedItemList.Count;

        switch(IslandType)
        {
            case IslandType.ISLAND_PLAYER:
                if (_installedItemList != null)
                {
                    for (int i = 0; i < _installedItemList.Count; i++)
                        LoadInstallDecoItem(_installedItemList[i]);
                }
                break;

            case IslandType.ISLAND_CENTER:
                if (_installedItemList != null)
                {
                    for (int i = 0; i < _installedItemList.Count; i++)
                        LoadInstallDecoItem(_installedItemList[i]);
                }
                break;

            case IslandType.FOREST_SILENCE:
            case IslandType.MY_HOME:
            case IslandType.CAMPING:
            case IslandType.CHO_SEOK_TOWN:
                if (_installedItemList != null)
                {
                    for (int i = 0; i < _installedItemList.Count; i++)
                        LoadInstallDecoItem(_installedItemList[i]);
                }
                break;
        }

        if(_installedItemList == null || _installedItemList.Count <= 0)
            Invoke("IslandSceneLoadComplete", 0.5f);
    }

    public void LoadInstallDecoItem(InvenData _invenData)
    {
        ItemData _itemData = _invenData.GetItemData();
        string _path = string.Empty;
        switch (IslandType)
        {
            case IslandType.ISLAND_PLAYER:
            case IslandType.ISLAND_CENTER:
            case IslandType.FOREST_SILENCE:
       
            case IslandType.MY_HOME:
                _path = string.Format(TextData.StrFormat[0], "Prefabs/Obj/Interior_home", _itemData.RscName);
                break;
            case IslandType.AQUARIUM:
                _path = string.Format(TextData.StrFormat[0], "Prefabs/Obj/Aquarium", _itemData.RscName);
                break;
            case IslandType.CAMPING:
                _path = string.Format(TextData.StrFormat[0], "Prefabs/Obj/Camping", _itemData.RscName);
                break;
        }

        //Addressables.LoadAssetAsync<GameObject>(_path).Completed +=
        //            (AsyncOperationHandle<GameObject> _obj) =>
        //            {
        //                GameObject _gameObject = Instantiate(_obj.Result, Vector3.zero, Quaternion.identity);
        //                LoadAssetInstallDecoItemComplete(_gameObject, _path, _invenData);
        //                Addressables.Release(_obj);
        //            };

        UIManager.Instance.LoadAsset<GameObject>(_path, LoadInstallDecoItemAssetCom, new object[] { _invenData });
    }

//    void LoadAssetInstallDecoItemComplete(GameObject _decoObj, string _path, InvenData _invenData)
    void LoadInstallDecoItemAssetCom(GameObject _obj, object[] _data)
    {
        // 초기화 데이타
        PunInstantiateInvenData _punInvenData = new PunInstantiateInvenData((InvenData)_data[0]);
        string _invenDataStr = JsonConvert.SerializeObject(_punInvenData);
        //        object[] _instantiateData = { _invenDataStr, _path, 0 };

        GameObject _decoObj = Instantiate(_obj);
        DecoController _decoCtr = _decoObj.GetComponent<DecoController>();

        object[] _instantiateData = new  object[]{ _invenDataStr };
        _decoCtr.Init(_instantiateData);

        if(installedItemCnt > -1)
        {
            installedItemCnt--;
            if(installedItemCnt == 0)
            {
                installedItemCnt = -1;
                IslandSceneLoadComplete();
            }
        }
    }

    public void InstantiateRoomObj(string _path, object[] _instantiateData, IPunPrefabPool.AssetLoadCompleteObjDel _delObj = null)
    {
        PN.InstantiateRoomObjectAsset(_path, Vector3.zero, Quaternion.identity, 0, _instantiateData, _delObj);
    }

    public void InstantiateRoomObj(string _path, Vector3 _pos, Quaternion _rot, object[] _instantiateData, IPunPrefabPool.AssetLoadCompleteObjDel _delObj = null)
    {
        PN.InstantiateRoomObjectAsset(_path, _pos, _rot, 0, _instantiateData, _delObj);
    }

    public void LoadFish()
    {
        List<InvenData> _aquaInvenDataList = UserData.Instance.GetAquaInvenDataList(AquaInvenType.OUT);
        string _fishRscName = string.Empty;

        for (int i = 0; i < _aquaInvenDataList.Count; i++)
        {
            _fishRscName = DataTblMng.Instance.GetFishResName(_aquaInvenDataList[i].Idx);
            SpawnFish(string.Format(TextData.StrFormat[0], "Prefabs/Obj/Fish", _fishRscName), AquaMapInfo.Instance.FishParent, _aquaInvenDataList[i]);
        }

        Invoke("IslandSceneLoadComplete", 0.5f);
    }

    public void SpawnFish(string _fishName, Transform _parent, InvenData _invenData)
    {
        PunInstantiateInvenData _punInvenData = new PunInstantiateInvenData(_invenData);
        string _invenDataStr = JsonConvert.SerializeObject(_punInvenData);
        object[] _instantiateData = { _invenDataStr };

        PN.InstantiateRoomObjectAsset(_fishName, new Vector3(150f, 11f, 150f), Quaternion.identity, 0, _instantiateData, SpawnFishComplete);
    }

    public void SpawnFishComplete(GameObject _gameObject)
    {
        FishController _fishController = _gameObject.GetComponent<FishController>();
        _fishController.InitAquarium();
    }

    public void RemoveInroomObj(PhotonView _photonView)
    {
        PN.Destroy(_photonView);
    }

    public void RemoveInroomObj(GameObject _gameObject)
    {
        PN.Destroy(_gameObject);
    }

    public void IslandSceneLoadComplete()
    {
        RequestAquaFishOwnership();

        switch (islandType)
        {
            case IslandType.ISLAND_PLAYER:
            case IslandType.BATTLE_ROYAL:
            case IslandType.MY_HOME:
            case IslandType.CAMPING:
                VirtualKeypad.Instance.InitInteractionBtnDisable();
                break;
            case IslandType.AQUARIUM:
                VirtualKeypad.Instance.SetDecoFunctionTypeBtn(DecoFunctionType.AQUARIUM_GATE, false, null);
                break;
            case IslandType.FOREST_SILENCE:
                ForestSilenceMapInfo.Instance.ForestMapSet.SetBgmEffSpeakList();
                break;
        }

        if(IsMyRoom())
        {
            NetManager.Instance.ReqEnter(IslandType, ResEnter);
        }

        UIManager.Instance.CloseUI<UITitle>();
        
        UINetConnect.Instance.Show(false);

        CheckSceneChgLoadObjWaitCnt();

        Main.Instance.SetState(MainState.PLAY);
    }

    public void CheckSceneChgLoadObjWaitCnt()
    {
        if (SceneChgLoadObjWaitCnt <= 0)
            UILoading.Instance.Show(false);
    }

    void ResEnter(Dictionary<string, object> _data)
    {
    }

    void RequestAquaFishOwnership()
    {
        if (IsMyRoom() == false)
            return;
        
        if(islandType == IslandType.AQUARIUM)
        {
            List<FishController> _fishCtrList = AquaManager.Instance.GetFishCtrList();

            for(int i=0; i<_fishCtrList.Count; i++)
            {
                _fishCtrList[i].RequestOwnership();
            }
        }
    }

    //public void RequestDecoItemOwnership()
    //{
    //    SetMasterClient();

    //    // 데코 아이템
    //    List<DecoController> _decoCtrList = GridSystem.Instance.InstalledDecoCtrObjList;
    //    for (int i = 0; i < _decoCtrList.Count; i++)
    //        _decoCtrList[i].RequestOwnership();
    //}

   public void GetPlayerStartPos(ref Vector3 _pos, ref Vector3 _rot)
    {
        _pos = new Vector3(0, 2f, 0);
        _rot = Vector3.zero;

        switch (IslandType)
        {
            case IslandType.ISLAND_PLAYER:
                if (MyIslandPos == Vector3.zero)
                {
                    IslandMapSet _myMapType = IslandPlayerMapInfo.Instance.MyMapSet();
                    if (_myMapType != null)
                        _pos = _myMapType.StartPosTr.position + new Vector3(0, 3f, 0);
                }
                else
                {
                    //_pos = MyIslandPos;
                    //_rot = MyIslandRot.eulerAngles;

                    //MyIslandPos = Vector3.zero;
                }

                // TODO : 테스트
                //                _pos = new Vector3(9.69f, 0.969f, 10.08f);      // 섬 중앙

                //_pos = new Vector3(4.76f, 0.969f, 10.09f);      // 내 집앞
                //_rot = new Vector3(0, 189, 0);

                _pos = new Vector3(9.41f, 0.96f, 5.62f);        // 아쿠아리움 앞
                _rot = new Vector3(0, 152.36f, 0);

                // 테스트 끝

                break;

            case IslandType.ISLAND_CENTER:
                IslandMapSet _islandMapset = IslandCenterMapInfo.Instance.MyMapSet();
                if (_islandMapset != null)
                {
                    _pos = _islandMapset.StartPosTr.position + new Vector3(0, 3f, 0);
                    _rot = _islandMapset.StartPosTr.rotation.eulerAngles;
                }

                // TODO : 테스트
                //                _pos = new Vector3(11.71f, 2.5f, 18.13f);      // 고요의숲 입구
                //                    _pos = new Vector3(23.19f, 1.5f, 5f);         // 낚시 지역
                //                _pos = new Vector3(10.58f, 1.5f, 5.17f);         // 아쿠아리움 앞
                //                _pos = new Vector3(16.076f, 0.969f, 14.631f);              // 캠핑장 입구
                //                _pos = new Vector3(17.22f, 0.96f, 8.36f);   // 상점 앞
                //                _pos = new Vector3(9.76f, 0.96f, 8.62f);   // 코스튬 상점 앞
                //                _pos = new Vector3(13.51f, 0.96f, 10.83f);   // 섬 인테리어 상점 앞
                //                _pos = new Vector3(15.11f, 0.96f, 5.76f);  // 레저(낚시, 미끼) 용품 상점 앞
                //                _pos = new Vector3(7.55f, 0.96f, 14.89f);       // 낭만상인

                // 오락기 앞
                _pos = new Vector3(8f, 1.5f, 12f);
                _rot = new Vector3(0, 135f, 0);

#if UNITY_EDITOR
                if (Main.Instance.TestSetting.IsRenewalIslandConn)
                    _pos = new Vector3(0, 30, 0);
#endif
                break;

            case IslandType.AQUARIUM:
                _pos = new Vector3(17.31f, 5f, 7.98f);
                break;

            case IslandType.BATTLE_ROYAL:
                {
                    switch(MiniGameManager.Instance.MiniGameKind)
                    {
                        case MiniGameKind.MAZE:
                            {
                                float _x = Random.Range(0, (MazeGenerator.Instance.MazeWid - 1f) * 9f);
                                float _y = Random.Range(-2, -7f);

                                // 3.3f : BattleRoyal Scene에 MapInfo/TempPlayerWaitArea/Plane y좌표가 3. 
                                _pos = new Vector3(_x, 3.3f, _y);
                            }
                            break;

                        case MiniGameKind.SNOW_FIGHT:
                            {
                                _pos = new Vector3(0, 3f, 0);
                                _rot = Vector3.zero;
                            }
                            break;

                        case MiniGameKind.MIND_MATCH:
                            {
                                float _x = Random.Range(6, 18);
                                float _z = Random.Range(6, 18);

                                _pos = new Vector3(_x, 5, _z);
                                _rot = Vector3.zero;
                            }
                            break;

                       
                    }
                }
                break;

            case IslandType.FOREST_SILENCE:
                IslandMapSet _forestMapType = ForestSilenceMapInfo.Instance.ForestMapSet;
                if (_forestMapType != null)
                    _pos = _forestMapType.StartPosTr.position + new Vector3(0, 1f, 0);
                break;

            case IslandType.MY_HOME:
                _rot = new Vector3(0, 45f, 0);
                break;

            case IslandType.CAMPING:
                CampingMapSet _campingMapset = CampingMapInfo.Instance.CampingMapSet;
                if(_campingMapset != null)
                {
                    _pos = _campingMapset.StartPosTr.position + new Vector3(0, .3f, 0);
                    _rot = _campingMapset.StartPosTr.rotation.eulerAngles;
                }

                // TODO : 테스트
                //                _pos = new Vector3(-2.371f, 2.999f, -2.394f);
                _pos = new Vector3(2.03f, 1.99f, 2.07f);
                // 테스트 끝
                break;

        }

//        CineCam.Instance.RotTarget(_rot);
    }

#region Test Character
    public List<NPlayer> TestPlayerList = new List<NPlayer>();
    public bool IsTestReloadCostumeAll = true;

    public void TestPlayerSpawn()
    {
        IsTestReloadCostumeAll = false;

        Vector3 _pos = new Vector3(0, 2f, 0);
        Vector3 _rot = Vector3.zero;

        GetPlayerStartPos(ref _pos, ref _rot);

        object[] _instantiateData = new object[] { false, 0, (int)Gender.MAN };
        GameObject _gameObject = PN.Instantiate("Prefabs/Player/MPlayer", _pos, Quaternion.Euler(_rot), 0, _instantiateData);
        NPlayer _nPlayer = _gameObject.GetComponent<NPlayer>();
        _nPlayer.AudioListenerOnOrOff();

        TestPlayerList.Add(_nPlayer);

        _nPlayer.nPlayerMove.TestCharSelctIdx = TestPlayerList.Count;
        UIHud.Instance.TestSelectCharIdx = TestPlayerList.Count;
    }

    public void TestPlayerRemove()
    {
        if (TestPlayerList.Count > 0)
        {
            PN.Destroy(TestPlayerList[TestPlayerList.Count - 1].gameObject);
            TestPlayerList.RemoveAt(TestPlayerList.Count - 1);
        }

        UIHud.Instance.TestSelectCharIdx = TestPlayerList.Count;
    }
#endregion

    void SpawnNpcPlayer()
    {
        List<ObscuredInt> _npcIdxList = DataTblMng.Instance.GetBuildLocNpcDataList();
        if (_npcIdxList == null)
            return;
        
        for(int i=0; i<_npcIdxList.Count; i++)
        {
            LoadNpcAsset(_npcIdxList[i]);
        }
    }

    void LoadNpcAsset(int _npcID)
    {
        NpcData _npcData = null;
        string _basePath = string.Empty;

        _npcData = DataTblMng.Instance.GetNpcData(_npcID);
        _basePath = _npcData.IsUseCostumParts ? "Prefabs/Player/" : "Prefabs/Player/NPC/";

        string _path = $"{_basePath}{_npcData.RscName}";

        //Addressables.LoadAssetAsync<GameObject>(_pathName).Completed +=
        //    (AsyncOperationHandle<GameObject> _obj) =>
        //    {
        //        if (_obj.Status == AsyncOperationStatus.Succeeded)
        //        {
        //            GameObject _gameObject = Instantiate(_obj.Result, _npcData.Pos, Quaternion.Euler(0, _npcData.RotY, 0));
        //            LoadNpcAssetComplete(_gameObject, _npcID);
        //            Addressables.Release(_obj);
        //        }
        //    };

        UIManager.Instance.LoadAsset<GameObject>(_path, LoadNpcAssetCom, new object[] { true, _npcID, 0, _npcData.Pos, Quaternion.Euler(0, _npcData.RotY, 0) });
    }

//    void LoadNpcAssetComplete(GameObject _npcObj, int _npcID)
    void LoadNpcAssetCom(GameObject _obj, object[] _data)
    {
        //        object[] _instantiateData = new object[] { true, _npcID, 0 };

        GameObject _npcObj = Instantiate(_obj);
        NPlayer _nPlayer = _npcObj.GetComponent<NPlayer>();
        _nPlayer.Init(_data);
        _nPlayer.nPlayerMove.Init(_data);
    }

    public bool IsSameHomeRoom(HomeRoomData _data)
    {
        if (HomeFloorNo == _data.FloorNo && HomeRoomNo == _data.RoomNo)
            return true;

        return false;
    }

#region Chat
    public void ChatMsg(object[] _data)
    {
        string _channel = JsonDecode.ToString(_data[0]);
        string _nickname = JsonDecode.ToString(_data[1]);
        string _msg = JsonDecode.ToString(_data[2]);

        if (MyPlayer.Nickname.Equals(_nickname))
        {
            MyPlayer.ChatMsg(_msg);
        }
        else
        {
            if (ChattingManager.Instance.IsSubscribe(_channel) == false && IslandType == IslandType.CAMPING && CampingManager.Instance.MyCampNo == -1)
                _msg = "...";
            else if(ChattingManager.Instance.IsSubscribe(_channel) == false)
                _msg = string.Empty;

            if (string.IsNullOrEmpty(_msg) == false)
            {
                var _dicEnu = OtherPlayer.GetEnumerator();
                while (_dicEnu.MoveNext())
                {
                    if (_dicEnu.Current.Value.Nickname.Equals(_nickname))
                    {
                        _dicEnu.Current.Value.ChatMsg(_msg);
                        return;
                    }
                }
            }
        }
    }
#endregion

#region Quest
    public void RefreshNpcQuestInfo(int _chkNpcID = -1)
    {
        List<NPlayer> _npcList = null;
        if (_chkNpcID == -1)
        {
            _npcList = new List<NPlayer>(NpcPlayer.Values);
        }
        else if(NpcPlayer.ContainsKey(_chkNpcID))
        {
            _npcList = new List<NPlayer>() { NpcPlayer[_chkNpcID] };
        }

        if (_npcList != null)
        {
            for (int i = 0; i < _npcList.Count; i++)
                _npcList[i].CheckQuestState();
        }
    }
#endregion

#region MiniGame Interaction Info Show/Hide
    public void InteractionBtnAllShow(bool _show)
    {
        var _dicEnu = NpcPlayer.GetEnumerator();
        while (_dicEnu.MoveNext())
        {
            if(_dicEnu.Current.Value.CanvasTran != null)
                _dicEnu.Current.Value.CanvasTran.gameObject.SetActive(_show);
        }

        GridSystem.Instance.InteractionBtnAllShow(_show);

        if(_show == false)
            VirtualKeypad.Instance.SetDecoFunctionTypeBtn(DecoFunctionType.FISHING_AREA, _show);
    }

#endregion

#region BattleRoyal Snowfight
    public void SnowFightItemExe(SnowFightItempType _type)
    {
/*
        switch (_type)
        {
            case SnowFightItempType.ITEM_SNOW_BULLET:
                MiniGameManager.Instance.ManagerBullet(MyPlayer.ActorNum, (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_GET_SNOW_BULLET_RELOAD_AMOUNT));
                break;
            case SnowFightItempType.ITEM_HEAL:
                MyRPC.SnowFightHPManager(MyPlayer.ActorNum, (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_GET_HEAL_ITEM_INCREASE_HP));
                break;
            case SnowFightItempType.ITEM_SPEEDUP:
                MyPlayer.nPlayerMove.GetSpeedItem(DataTblMng.Instance.GetConfigData(ConfigType.SF_SPEED_UP_ITEM_MOVE_SPEED));
                break;
            case SnowFightItempType.ITEM_SHIELD:
                MyRPC.BRSShowEff((int)DataTblMng.Instance.GetConfigData(ConfigType.SF_GET_SHIELD_ITEM));
                break;
            case SnowFightItempType.ITEM_MACHINEGUN:
                MyRPC.BRShowChangeGun(MyPlayer.ActorNum);
                break;
            case SnowFightItempType.ITEM_POWER_BULLET:
                MiniGameManager.Instance.GetPowerBulletItem(MyPlayer.ActorNum,true);
                break;
        }
*/
    }


#endregion
}



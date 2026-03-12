using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization.Json;
public class GameManager :Singletone<GameManager>
{
    public int GameNum;
    #region Save
    //TODO 테스트 
    string GameDataFileName = "RoomData.json";
    string UserDataFileName = "UserData.json";
    string CostumDataFileName = "CostumeData.json";
    //사용할것=> 위에 게 아니라
    string TmpCostumDataFileName = "TmpCostumeData.json";

    private List<RoomData> RoomsDataList = new List<RoomData>();
    #endregion
    //결국 데이터를 넣어줄때 Userdata에도 넣어주고 nPlayer에도 넣어줘야 한다=> 지금스크립트로는 
    public Nplayer nPlayer;
    public List<Nplayer> NpcList = new List<Nplayer>();

  
    #region Tutorial
    public bool WelcomeSaying { get; set; } 

    public List<TutorialCell> TutorialFirstStep = new List<TutorialCell>();
    //TODO 테스트
    public double RemainTIme { get; set; }
    public double StartTime { get; private set; }
    public double LimitTime { get; set; }

    public int SecondTutorialAttackCnt { get; set; }
    #endregion

    #region SetPlayerInfo
    public List<PlayerData> playerLevelDataList = new List<PlayerData>();
    #endregion

    #region MonsterPool
    public List<Nplayer> MonsterList = new List<Nplayer>();
    //public List<GameObject> MonsterPool = new List<GameObject>();
    private MonsterKind _tempKind;
    #endregion

    //TODO 테스트 
    public void SetNplayerInfo(PlayerInfo _temp)
    {
        //값 넘어서 오는것 까지 확인 완
        string Nickname = _temp.NickName;
        int LEVEL = _temp.Level;

    }
    #region Tutorial
    public void TutorialSetPlayTime(double _playTime)
    {
        if (_playTime > 0)
        {
            LimitTime = _playTime;

            //if (LimitTime <= 0)
                nPlayer.ChangeTutorialState(nPlayer.playerState);
        }
    }

    void TutorialRemainTime()
    {
        if (nPlayer == null)
            return;

        //if (!CheckWelcomeSaying())
        //    return;

        switch (nPlayer.playerState)
        {
            case PlayerState.NONE:
            case PlayerState.DOING_FIRST_TUTORIAL: 
            case PlayerState.COMPLETE_FIRST_TUTORIAL:
            case PlayerState.DOING_SECOND_TUTORIAL:
            case PlayerState.COMPLETE_SECOND_TUTORIAL:
                RemainTIme = Mathf.Max(0, (float)(LimitTime - Time.deltaTime));
                break;
        }

        if(nPlayer.playerState == PlayerState.NONE)
        {
            TutorialSetPlayTime(3);
            UIGame.Instance.SetMsg("가상 세계에 오신 걸 환영합니다");
        }
        else if (nPlayer.playerState == PlayerState.DOING_FIRST_TUTORIAL)
        {
            if(CheckFirstTutorialClear())
            {
                TutorialSetPlayTime(3);
            }
            else 
            {
                UIGame.Instance.SetMsg("모든 상자를 따라가세요!");
            }
        }
        else if (nPlayer.playerState == PlayerState.DOING_SECOND_TUTORIAL)
        {
            if (CheckSecondTutorial())
            {
                TutorialSetPlayTime(3);
            }
            else
            {
                UIGame.Instance.SetMsg("'Z'키를 눌러 5회 공격을 하세요");
            }
        }
        else if (nPlayer.playerState == PlayerState.COMPLETE_FIRST_TUTORIAL || nPlayer.playerState == PlayerState.COMPLETE_SECOND_TUTORIAL /*|| nPlayer.playerState == PlayerState.COMPLETE_THIRD_TUTORIAL*/)
        {

            if(nPlayer.playerState == PlayerState.COMPLETE_FIRST_TUTORIAL)
            {
                TutorialSetPlayTime(5);
            }
            else if(nPlayer.playerState == PlayerState.COMPLETE_SECOND_TUTORIAL)
            {
                UIGame.Instance.SetMsg("COMPLETE ALL TUTORIAL");
                TutorialSetPlayTime(5);
                MapInfo.Instance.HideTutorialWall();
            }
        }
    }

    public void LoadFistTutorailCell()
    {
        for (int i = 0; i < (int)Test.THIRD; i++)
            UIManager.Instance.LoadPrefabAsset<GameObject>("Fx/Appear_fx", LoadFirstTutorialCellAsset);
    }

    void LoadFirstTutorialCellAsset(GameObject _gameObject, object[] _data)
    {
        GameObject _temp = Instantiate(_gameObject);

        TutorialCell _tutorialCell = _temp.GetComponent<TutorialCell>();
        TutorialFirstStep.Add(_tutorialCell);

        _tutorialCell.Init(Test_objectType.TUTORIAL_FX);
    }

    public void SetSecondTutorial()
    {
        UIManager.Instance.LoadPrefabAsset<GameObject>("Player/NPC/Tester", LoadTesterAsset);
    }
  
    void LoadTesterAsset(GameObject _gameObject, object[] _data)
    {
        GameObject _player = Instantiate(_gameObject);
        NplayerMove _nplayerMove = _player.GetComponent<NplayerMove>();
        Nplayer _nplayer = _player.GetComponent<Nplayer>();

       // object[] _tempData = new object[] { (int)PlayerKind.NPC, (int)PlayerState.DOING_SECOND_TUTORIAL };
        object[] _temp = new object[] { (int)PlayerKind.NPC ,(int)PlayerState.DOING_SECOND_TUTORIAL };


        //_nplayerMove.Init(_tempData);
        _nplayer.Init(_temp); 
    }

    public void LoadMiniMapCam()
    {
        UIManager.Instance.LoadPrefabAsset<GameObject>("Camera/MiniMapCamera", LoadMiniMapCamCell);
    }

    void LoadMiniMapCamCell(GameObject _gameObject, object[] _data)
    {
        GameObject _temp = Instantiate(_gameObject);

        _temp.transform.SetParent(CamaraManager.Instance.transform);
        CamaraManager.Instance.MiniMapCamera = _temp;
    }
    #endregion

    public bool CheckFirstTutorialClear()
    {
        int _temp = 0;

        for(int i= 0; i< TutorialFirstStep.Count; i++)
        {
            if (!TutorialFirstStep[i].gameObject.activeSelf)
                _temp++;
        }

        if (_temp == 3)
            return true;

        return false;
    }
    public bool CheckSecondTutorial()
    {
        if (CheckFirstTutorialClear())
        {
            if (SecondTutorialAttackCnt >= 5)
                return true;
        }
        return false;
    }

    #region FXLoad
    public void LoadHitEffect()
    {
        //TODO 테스트
        UIManager.Instance.LoadPrefabAsset<GameObject>("", LoadHitEffectAssetCom);
    }

    void LoadHitEffectAssetCom(GameObject _gameObject, object[] _data)
    {
        GameObject _eff = Instantiate(_gameObject);
        //TODO 테스트
        //_eff.transform.position = nPlayer.HittedEffectZone.transform.postion;
    }
    #endregion

    #region PlayerLoad
    //Local테스트용=> 포톤 사용x 
    public void LoadPlayer()
    {
        UIManager.Instance.LoadPrefabAsset<GameObject>("Player/Player/Player", LoadPlayerAsset);
    }

    void LoadPlayerAsset(GameObject _gameObject, object[] _data)
    {
        GameObject _player = Instantiate(_gameObject);
        Nplayer _nplayer = _player.GetComponent<Nplayer>();

        object[] _tempData = new object[] { (int)PlayerKind.PLAYER, (int)UserData.Instance.job, (int)PlayerState.NONE };

        _nplayer.Init(_tempData);

        //TODO 테스트 :플레이어대한 정보
        //GameManager.Instance.SetPlayerInfo();
    }
    #endregion

    #region MonsterLoad
    //TODO 테스트
    //Local테스트용=> 포톤 사용x 
    public void LoadMonsterPool(MapType _mapType)
    {
        switch (_mapType)
        {
            case MapType.MY_HOME: _tempKind = (MonsterKind)(int)_mapType; break;
            case MapType.MAIN_TOWN: _tempKind = (MonsterKind)(int)_mapType; break;
            case MapType.MONSTERFIELD: _tempKind = (MonsterKind)(int)_mapType; break;
        }

        for (int i = 0; i < 2; i++)
        {
            LoadMonsterPerStage(_tempKind);
        }
    }


    public void LoadMonsterPerStage(MonsterKind _monKind)
    {
        UIManager.Instance.LoadPrefabAsset<GameObject>($"Monster/Monster_MY_HOME",LoadMonsterPerStageAssetCom);
        //UIManager.Instance.LoadPrefabAsset<GameObject>($"Monster/Monster_{_monKind}", LoadMonsterPerStageAssetCom);
    }

    void LoadMonsterPerStageAssetCom(GameObject _gameObject,object[] _data)
    {
        GameObject _object = Instantiate(_gameObject);
        //MonsterPool.Add(_object);

        Nplayer _nplayer = _object.GetComponent<Nplayer>();
        //맵 매니저를 만들어서 거기서 맵 타입을 넘겨서 받아오게 하기 
        object[] _tempData = new object[] { (int)PlayerKind.MONSTER,(int)MapType.MY_HOME/*, MapManager.Instance.mapTypeMonsterKind.Wolf */ };
        _nplayer.Init(_tempData);
    }
    #endregion

    #region Get,SetPlayerInfo
    public Nplayer GetPlayerInfo()
    {
        return nPlayer;
    }
    
    //레벨업 할때마다 올려놓기
    public void SetPlayerInfo()
    {
        //TODO 테스트
        playerLevelDataList = DataTblMng.Instance.GetPlayerLevelSysDataList(PlayerKind.PLAYER);

        for(int i= 0; i < playerLevelDataList.Count; i++)
        {
            if (playerLevelDataList[i].Idx ==nPlayer.LEVEL)
            {
                //nPlayer.playerData = playerLevelDataList[i];
                //UserData.Instance.SetUserData(playerLevelDataList[i]);
                //Nplayer에게 값전달하기
            }
        }
    }
    #endregion

    #region Welcome Saying
    public bool CheckWelcomeSaying()
    {
        return false;
    }

    #endregion

    #region Save
    public void SaveUserData()
    {
        PlayerInfo playerInfoData = new PlayerInfo();

        playerInfoData.Level = UserData.Instance.Level;
        playerInfoData.NickName =UserData.Instance.Nickname;
        playerInfoData.EXP = UserData.Instance.EXP;
        playerInfoData.Money = UserData.Instance.Money;

        Debug.Log($"<color=blue>playerInfoData.NickName:{playerInfoData.NickName}</color> ");

        string ToJson = JsonUtility.ToJson(playerInfoData);
        string filePath = Application.persistentDataPath + "/" + UserDataFileName;

        Debug.Log($"<color=white>{ToJson}</color> ");
        File.WriteAllText(filePath, ToJson);
    }

    public void SaveCostumeData()
    {
        //TODO 테스트 
        List<ItemData> _temp = DataTblMng.Instance.GetPerCateItemDataList(7);

        string ToJson = JsonConvert.SerializeObject(_temp);
        string filePth = Application.persistentDataPath + "/" + CostumDataFileName;

        File.WriteAllText(filePth,ToJson);

        //찐 사용할 코드 
        ////저장할때 Dictionary<part,itemNum>이렇게 들어있어서 이걸로 저장하면 됨=> itemNum은 Index로 한다
        Dictionary<int, int> _part = new Dictionary<int, int>();
        Dictionary<CostumeParts, ItemData> _itemData = new Dictionary<CostumeParts, ItemData>();

        _itemData = UserData.Instance.GetequipToolDic();
        for(int i=(int)CostumeParts.HEAD;i<=(int)CostumeParts.TOOL;i++)
        {

            if(_itemData.ContainsKey((CostumeParts)i)==false)
            {
                _itemData.Add((CostumeParts)i,null);
            }


            if (_part.ContainsKey(i) == false)
            {
                if(_itemData[(CostumeParts)i]==null)
                {
                    _part.Add(i, -1);
                }
                else
                {
                    _part.Add(i, _itemData[(CostumeParts)i].Idx);
                }
            }    
        }

        string TOJSON = JsonConvert.SerializeObject(_part);
        string FILEPATH = Application.persistentDataPath + "/" + TmpCostumDataFileName;

        File.WriteAllText(FILEPATH, TOJSON);
        ////TODO 테스트
        //Dictionary<int, List<ItemData>> _itemDic = new Dictionary<int, List<ItemData>>();
        //for (int i = 0; i < _temp.Count; i++)
        //{
        //    if (!_itemDic.ContainsKey(_temp[i].category))
        //        _itemDic.Add(_temp[i].category, new List<ItemData>());
        //    _itemDic[_temp[i].category].Add(_temp[i]);
        //}



        ////Dictionary<Equippart,itemData>//
        //Dictionary<int, ItemData> UserCostume = new Dictionary<int, ItemData>();
        //for (int i= 0; i < _temp.Count; i++)
        //{
        //    UserCostume.Add(_temp[i].category, _temp[i]);
        //}



        //Dictionary<CostumeParts, ItemData> equipToolDic = new Dictionary<CostumeParts, ItemData>();
        //equipToolDic = UserData.Instance.GetequipToolDic();
        //List<ItemData> _itemData = new List<ItemData>();
        //for (int i = (int)CostumeParts.HEAD; i <= (int)CostumeParts.TOOL; i++)
        //{
        //    _itemData.Add(equipToolDic[(CostumeParts)i]);
        //}
    }

    //게임 종료시 데이터 저장 후 게임 종료 
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log($"<color=pink>Quit the Game</color>");
    }

    private void OnApplicationQuit()
    {
        SaveUserData();
        SaveCostumeData();
        PunManager.Instance.Disconnect();
    }
    #endregion

    #region DataLoad
    public void LoadUserData()
    {
        string filePath = Application.persistentDataPath + "/" + UserDataFileName;

        if (!File.Exists(filePath))
        {
            return;
        }

        string saveFIle = File.ReadAllText(filePath);
        PlayerInfo playerInfo = JsonUtility.FromJson<PlayerInfo>(saveFIle);
       
        //UserData에설정 
        UserData.Instance.LoadUserData(playerInfo);
    }

    //아이템 정보 로드 
   public void LoadItemData()
    {
        string filePath = Application.persistentDataPath + "/" + TmpCostumDataFileName;

        if(!File.Exists(filePath))
        {
            return;
        }

        string saveFile = File.ReadAllText(filePath);
        //Dictionary<Part,Item's Idx>
        Dictionary<int, int> _tempDic = JsonConvert.DeserializeObject<Dictionary<int, int>>(saveFile);

        UserData.Instance.SetItemData(_tempDic);
    }
    #endregion

    #region MonsterRespawn
    //포톤을 붙일 경우 몬스터 사망및 재 소환하는걸 RPC로 보낼것
    public void CheckMonsterIsServive(Nplayer _mon)
    {
        
        for(int i=0; i < MonsterList.Count; i++)
        {
          if (MonsterList[i]==_mon)
          {
                StartCoroutine(Respawn(5f, i));
          }
        }
    }

    IEnumerator Respawn(float _time,int _num)
    {
        yield return new WaitForSeconds(_time);
        MonsterList[_num].IsDead = false;
        MonsterList[_num].gameObject.SetActive(true);
    }

    #endregion

    private void LateUpdate()
    {
        TutorialRemainTime();
    }

    
}

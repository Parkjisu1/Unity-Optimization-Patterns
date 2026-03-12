using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using PN = Photon.Pun.PhotonNetwork;

public partial class MiniGameManager : MonoBehaviourPunCallbacks
{
    public float ShieldSustainTime { get; private set; }
    public float PowerBulletSustainTime { get; private set; }
    public float MachineGunSustainTime { get; set; }
    public float SpeedUpSustainTime { get; set; }

    public List<int> impossibleGetSnowItemList = new List<int>();
    private Queue<object[]> getSnowItemQueue = new Queue<object[]>();
    private Queue<object[]> playerHitQueue = new Queue<object[]>();

  
    void BRSSnowFightRemainTime()
    {
        if (PunManager.MyRPC == null)
            return;

        switch (MiniGameSt)
        {
            case MiniGameSt.BR_PLAYER_WAIT:
            case MiniGameSt.READY_COUNT_DOWN:
            case MiniGameSt.PLAY:
            case MiniGameSt.FINISH:
                RemainTime = Mathf.Max(0, (float)(LimitTime - (PN.Time - StartTime)));
                break;
        }

        if(MiniGameSt == MiniGameSt.BR_PLAYER_WAIT)
        {
            if (PunManager.Instance.OtherPlayer.Count + 1 >= DataTblMng.Instance.GetMiniGameMaxPlayer(MiniGameType, MiniGameKind))
            {
                if (IsGameMaster())
                    PunManager.MyRPC.MiniGameStateChg((int)MiniGameSt.BRS_SELECT_GAME, GameNo);
            }
            else if (RemainTime < 1)
            {
#if UNITY_EDITOR
                if (Main.Instance.TestSetting.WaitMinPlayer && PunManager.Instance.OtherPlayer.Count + 1 < DataTblMng.Instance.GetMiniGameMinPlayer(MiniGameType.BATTLE_ROYAL, MiniGameKind))
                    return;
                else
#endif
                SetNextState();
            }
        }
        else if (MiniGameSt == MiniGameSt.READY_COUNT_DOWN || MiniGameSt == MiniGameSt.PLAY || MiniGameSt == MiniGameSt.FINISH)
        {
            if (RemainTime < 1)
            {
                SetNextState();
            }
        }
    }
    void SetBRSnowFightState()
    {
        switch (MiniGameSt)
        {
            case MiniGameSt.NONE:
                impossibleGetSnowItemList.Clear();
                getSnowItemQueue.Clear();
                playerHitQueue.Clear();
                UIGame.Instance.buffListItems.Clear();

                UIManager.Instance.CloseUI<UIGame>();
                PunManager.Instance.InteractionBtnAllShow(true);

                if (UIManager.Instance.OpenUICount <= 0)
                    UIHud.Instance.SetHudView(HudViewType.ISLAND);

                MiniGameKind = MiniGameKind.NONE;
                break;
            case MiniGameSt.BRS_FIELD_LOAD:
                InitGamePlayerInfoDic();

                if (IsGameMaster())
                {
                    int _maptype = Random.Range(0, DataTblMng.Instance.GetSFMapTypeCnt()) + 1;
                    int _randSeed = Random.Range(0, 10000);

                    PunManager.MyRPC.BRSFSetMapType(_maptype, _randSeed);

                    SetBRPlayerStartPos();
                    PlayerPosInfo();
                }
                break;
            case MiniGameSt.BRS_FIELD_MAKE:
                if (IsGameMaster())
                {
                    //                    PunManager.MyRPC.BRSFShowHud();
                    PunManager.MyRPC.SendTypeRPC((int)RPCType.BRSF_SHOW_HUD);
                }
                break;

            case MiniGameSt.CREATE_FAIL:
                string _msg = string.Format(TextData.Msg["MINI_GAME_CREATE_FAIL"]
                                          , DataTblMng.Instance.GetMiniGameMinPlayer(MiniGameType, MiniGameKind)
                                          , TextData.GameName[((int)MiniGameKind).ToString()]);
                UIBRPlayerWait.Instance.SetMsg(_msg);
                SetState(MiniGameSt.NONE);
                break;

            case MiniGameSt.BRS_SHOW_HUD:
                BRMapInfo.Instance.PlayerWaitAreaHide();

                UIManager.Instance.OpenUI<UIGame>(UIManager.Hash("path", "UI/Game"));
                UIGame.Instance.Init();
//                UIGame.Instance.ShowSFHud();

                if (UIBRPlayerWait.Instance != null)
                    Destroy(UIBRPlayerWait.Instance.gameObject);

                SetState(MiniGameSt.READY_COUNT_DOWN);
                break;
            case MiniGameSt.READY_COUNT_DOWN:
                if (IsGameMaster())
                {
//                    PunManager.MyRPC.SendRPC(new object[] { (int)RPCType.BR_READY_COUNT, (int)GameNo });
                    PunManager.MyRPC.BRReadyCount(GameNo);
                }
                break;

            case MiniGameSt.GAME_START:
                UIGame.Instance.CountDownStart(false);
                UIGame.Instance.GameStart();
                UIGame.Instance.SpawnBuff();
                impossibleGetSnowItemList.Clear();

                if (MiniGameKind == MiniGameKind.SNOW_FIGHT)
                {
                    UIGame.Instance.SnowFightInfoShow(true);
                    //                    PunManager.MyRPC.BRSFShowGun();
                    PunManager.MyRPC.SendTypeRPC((int)RPCType.BRSF_SHOW_GUN);
                    PunManager.MyPlayer.HeartIconShow(false);
                    if (PunManager.Instance.OtherPlayer.Count > 0)
                    {
                        var _dicPlayer = PunManager.Instance.OtherPlayer.GetEnumerator();
                        while (_dicPlayer.MoveNext())
                        {
                             NPlayer _nplayer = _dicPlayer.Current.Value;

                            if (PunManager.Instance.OtherPlayer.ContainsKey(_nplayer.ActorNum))
                                _nplayer.HeartIconShow(true);
                        }
                    }
                }
                SetState(MiniGameSt.PLAY);
                break;

            case MiniGameSt.FINISH:
                UIGame.Instance.TimeOutShow(true);
                break;

            case MiniGameSt.RESULT:
                HideAllPlayer();

                UIGame.Instance.HideUI();
                UIChat.Instance.Show(false);
                UIManager.Instance.JoystickPadShow(false);

                PopupManager.Instance.OpenPopup<UIPopGameRankResult>();//눈싸움에대한 결과도 처리하게 나중에 스크립트 수정
                UIPopGameRankResult.Instance.Init(MiniGameKind);
                SetState(MiniGameSt.NONE);
                break;
        }
    }

    void SetBRSnowFightNextState()
    {
        switch (MiniGameSt)
        {
            //            case MiniGameSt.BRS_PLAYER_WAIT:
            case MiniGameSt.BR_PLAYER_WAIT:
                SetState(MiniGameSt.STATE_CHG_WAIT);
                if (PunManager.Instance.OtherPlayer.Count + 1 < DataTblMng.Instance.GetMiniGameMinPlayer(MiniGameType.BATTLE_ROYAL, MiniGameKind))
                {
                    PunManager.MyRPC.SendRPC(new object[] { (int)RPCType.BR_CREATE_FAIL, (int)GameNo });
                    //PunManager.MyRPC.BRCreateFail();
                }
                else
                {
                    PunManager.MyRPC.SendRPC(new object[] { (int)RPCType.BR_READY, (int)GameNo });
                   // PunManager.MyRPC.BRReady();
                    PunManager.Instance.RoomClose();
                }
                break;

            case MiniGameSt.READY_COUNT_DOWN:
                SetState(MiniGameSt.STATE_CHG_WAIT);
                PunManager.MyRPC.BRStart(GameNo);
                break;

            case MiniGameSt.PLAY:
                SetState(MiniGameSt.STATE_CHG_WAIT);
                PunManager.MyRPC.GameFinish();
                break;
            case MiniGameSt.FINISH:
                SetState(MiniGameSt.STATE_CHG_WAIT);
                PunManager.MyRPC.SendRPC(new object[] { (int)RPCType.GAME_RESULT, (int)GameNo });
                //PunManager.MyRPC.GameResult();
                break;
        }
    }

    public void PlayerPosInfo()
    {
        List<SnowFightPosData> snowFightPosDataList = DataTblMng.Instance.GetSnowFightPosDataLIst(SnowFightFieldGenerator.Instance.mapType);
        List<SnowFightPosData> spawnPosDataList = new List<SnowFightPosData>();
        for (int i = 0; i < snowFightPosDataList.Count; i++)
        {
            if (snowFightPosDataList[i].BlockType == (int)SnowFightBlockType.SPAWNPOS)
            {
                spawnPosDataList.Add(snowFightPosDataList[i]);
            }
        }

        int _playerCnt = PunManager.Instance.PlayerCnt();
        object[] _playerPos = new object[(_playerCnt * 2) + 1];
        _playerPos[0] = (int)RPCType.BRSF_PLAYER_POS;

        int _rand = Random.Range(0, spawnPosDataList.Count);
        SnowFightPosData _posData = spawnPosDataList[_rand];
        _playerPos[1] = (int)PunManager.MyPlayer.ActorNum;
        _playerPos[2] = _posData.Pos;
        spawnPosDataList.RemoveAt(_rand);

        List<int> _actorNumList = new List<int>(PunManager.Instance.OtherPlayer.Keys);
        for (int i = 1; i < _playerCnt; i++)
        {
            _rand = Random.Range(0, spawnPosDataList.Count);
            _posData = spawnPosDataList[_rand];
            _playerPos[(i * 2) + 1] = _actorNumList[i - 1];
            _playerPos[(i * 2) + 2] = _posData.Pos;

            spawnPosDataList.RemoveAt(_rand);
        }

        //        PunManager.MyRPC.BRSPlayerPos(_playerPos);
        PunManager.MyRPC.SendRPC(_playerPos);
    }

    public void BRSFieldMake()
    {
        SetState(MiniGameSt.BRS_FIELD_MAKE);
    }

    public void IsPossibleGetSnowItem(object[] _data)
    {
        if(IsGameMaster())
            getSnowItemQueue.Enqueue(_data);
    }

    void CheckPossibleGetSnowItem()
    {
        if (MiniGameKind != MiniGameKind.SNOW_FIGHT)
            return;

        if(getSnowItemQueue.Count>0)
        {
            object[] _data = getSnowItemQueue.Dequeue();

            int _gameNo = JsonDecode.ToInt(_data[0]);

            if(IsMyGame(_gameNo))
            {
                int _actorNum = JsonDecode.ToInt(_data[1]);
                int _itemIndex = JsonDecode.ToInt(_data[2]);

                bool _isPossible = true;

                if(impossibleGetSnowItemList.Contains(_itemIndex))
                    _isPossible = false;
                else
                    impossibleGetSnowItemList.Add(_itemIndex);

                _data = new object[] { (int)RPCType.BRSF_IS_POSSIBLE_GET_SNOW_ITEM_RESULT, _gameNo, _actorNum, _itemIndex, _isPossible };
                PunManager.MyRPC.SendRPC(_data);
            }
        }
    }

    public void SetRespawnItemIdxChange(object[] _data)
    {
        int _idx = JsonDecode.ToInt(_data[0]);
        int _regenIdx = JsonDecode.ToInt(_data[1]);

        if(impossibleGetSnowItemList.Contains(_regenIdx) == false)
            impossibleGetSnowItemList.Add(_regenIdx);

        if (impossibleGetSnowItemList.Contains(_idx))
            impossibleGetSnowItemList.Remove(_idx);
    }

    public void IsPossibleGetSnowItemResult(object[] _data)
    {
        if (IsMyGame(JsonDecode.ToInt(_data[0])) && JsonDecode.ToBool(_data[3]))
        {
            int _idx = JsonDecode.ToInt(_data[2]);
            int _actorNum = JsonDecode.ToInt(_data[1]);
            bool _isMyActorNum = PunManager.MyPlayer.ActorNum == _actorNum;

            SnowFightItempType _itemType = SnowFightFieldGenerator.Instance.GetItemType(_idx);
            switch(_itemType)
            {
                case SnowFightItempType.ITEM_SNOW_BULLET:
                    if(_isMyActorNum)
                        ManagerBullet(_actorNum, (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_GET_SNOW_BULLET_RELOAD_AMOUNT));
                    break;

                case SnowFightItempType.ITEM_HEAL:
                    SnowFightHeal(_actorNum, (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_GET_HEAL_ITEM_INCREASE_HP));
                    break;

                case SnowFightItempType.ITEM_SPEEDUP:
                    if (_isMyActorNum)
                        GetSpeedItem();
                    //PunManager.MyPlayer.nPlayerMove.GetSpeedItem(DataTblMng.Instance.GetConfigData(ConfigType.SF_SPEED_UP_ITEM_MOVE_SPEED));
                   
                    break;
                case SnowFightItempType.ITEM_SHIELD:
                    ShowEffSnowFight(_actorNum, (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_GET_SHIELD_ITEM));
                    break;

                case SnowFightItempType.ITEM_MACHINEGUN:
                    NPlayer _nPlayer = PunManager.Instance.GetPlayerInfo(_actorNum);
                    GetMachineGunItem(_nPlayer);
                    _nPlayer.LoadMachineGun();
                    break;

                case SnowFightItempType.ITEM_POWER_BULLET:
                    if (_isMyActorNum)
                        GetPowerBulletItem(_actorNum, true);
                    break;
            }

            SnowFightFieldGenerator.Instance.BRSRespawnItem(_idx);
        }
    }

    public void ManageDeath(int _actorNum)
    {
        GamePlayerInfoDic[_actorNum].Died = true;
        GamePlayerInfoDic[_actorNum].SnowFightAddDeathCnt();
    }

    public void SnowFightHitCnt(int _actorNum, int _targetNum, int _point)
    {
        if (GamePlayerInfoDic.ContainsKey(_actorNum))
        {
            GamePlayerInfoDic[_actorNum].SnowFightAddHitCnt(_point);
            if (GamePlayerInfoDic[_targetNum].Died == true)
            {
                GamePlayerInfoDic[_actorNum].SnowFightAddKillCnt();
            }
        }
    }

    public void HideAllPlayer()
    {
        var _otherPlayerDic = PunManager.Instance.OtherPlayer.GetEnumerator();
        while (_otherPlayerDic.MoveNext())
            _otherPlayerDic.Current.Value.ShowPlayer(false);

        PunManager.MyPlayer.ShowPlayer(false);
    }

    public void InitSnowFightPlayer(int _actorNum)
    {
        GamePlayerInfoDic[_actorNum].SnowFightRespawnInit();

    }

    public void SnowBulletDamage(object[] _instantiateData)
    {
        if(IsGameMaster())
        {
            // isPlayerHit
            if (JsonDecode.ToBool(_instantiateData[0]))
            {
                playerHitQueue.Enqueue(_instantiateData);
            }
            else
            {
                SnowFightFieldGenerator.Instance.RemoveSnowBullet(JsonDecode.ToInt(_instantiateData[1]), JsonDecode.ToInt(_instantiateData[2]));
            }
        }
    }

    void CheckSnowBulletPlayerHit()
    {
        if(playerHitQueue.Count > 0)
        {
            object[] _data = playerHitQueue.Dequeue();

            /*
            Ball.SetDamage()에서 instantiateData 생성
            _instantiateData = new object[] { _isPlayerHit, OwnerActorNum, BulletNo, _collision.contacts[0].point, _nPlayer.ActorNum, _damage, _point };
            */

            if (SnowFightFieldGenerator.Instance.IsExistSnowBullet(JsonDecode.ToInt(_data[1]), JsonDecode.ToInt(_data[2])))
            {
                PunManager.MyRPC.BRSFHPManager(JsonDecode.ToInt(_data[4]), JsonDecode.ToInt(_data[5]));
                PunManager.MyRPC.BRSFHitCnt(JsonDecode.ToInt(_data[1]), JsonDecode.ToInt(_data[4]), JsonDecode.ToInt(_data[6]));

                SnowFightFieldGenerator.Instance.RemoveSnowBullet(JsonDecode.ToInt(_data[1]), JsonDecode.ToInt(_data[2]));
            }
        }
    }

    public void SnowBulletCrashEff(object[] _instantiateData)
    {
        UIManager.Instance.LoadPrefabAsset<GameObject>("Eff/SnowCrash", LoadSnowBulletCrashEffAssetCom, new object[] { (Vector3)_instantiateData[3] });
    }

    void LoadSnowBulletCrashEffAssetCom(GameObject _gameObject, object[] _instantiateData)
    {
        UtilManager.SetObjParentPosRot(Instantiate(_gameObject), BRMapInfo.Instance.transform, (Vector3)_instantiateData[0], Vector3.zero);
    }

    public void SnowFightDamage(int _actorNum,int _damage)
    {
        NPlayer _nplayer = PunManager.Instance.GetPlayerInfo(_actorNum);

        if (GamePlayerExistShield(_actorNum) == false)
        {
            if(GamePlayerInfoDic[_actorNum].HPCnt >0 && GamePlayerInfoDic[_actorNum].HPCnt <= DataTblMng.Instance.GetConfigData(ConfigType.SF_HP_MAX))
            {
                GamePlayerInfoDic[_actorNum].HPCnt = Mathf.Max(0, GamePlayerInfoDic[_actorNum].HPCnt + _damage);

                if (_nplayer.photonView.IsMine)
                {
                    UIMiniGameEff.Instance.StartShowEff();
                    UIGame.Instance.ManageHearts(_damage, _actorNum);
                }
                else
                    _nplayer.ManageHearts(_actorNum);

                if (GamePlayerInfoDic[_actorNum].HPCnt <=0)
                {
                    ManageDeath(_actorNum);

                    //                    _nplayer.nPlayerRPC.BRSFShowDieAni();
                    _nplayer.nPlayerRPC.SendTypeRPC((int)RPCType.BRSF_SHOW_DIE_ANI);
                }
            }
        }
        else if(GamePlayerInfoDic[_actorNum].GetShield ==true)
        {
            GamePlayerInfoDic[_actorNum].GetShield = false;
            _nplayer.DestroyShield();
            return;
        }
    }

    public void SnowFightHeal(int _actorNum, int _heal)
    {
        NPlayer _nplayer = PunManager.Instance.GetPlayerInfo(_actorNum);

        if (GamePlayerInfoDic[_actorNum].HPCnt >= 3)
            return;

        GamePlayerInfoDic[_actorNum].HPCnt += _heal;

        if (_nplayer.photonView.IsMine)
            UIGame.Instance.ManageHearts(_heal,_actorNum);
        else
        {
            _nplayer.ManageHearts(_actorNum);
        }

        _nplayer.nPlayerRPC.BRSFShowEff(_heal);
    }

    public void ManagerBullet(int _actorNum,int _temp)
    {
        GamePlayerInfoDic[_actorNum].BulletCnt += _temp;
    }

    public void ManagerBullet(bool _isAdd, int _cnt)
    {
        if(_isAdd)
        {
            _cnt = (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_GET_SNOW_BULLET_RELOAD_AMOUNT);
        }

        GamePlayerInfoDic[PunManager.MyPlayer.ActorNum].BulletCnt += _cnt;
    }

    public void ShowEffSnowFight(int _actorNum, int _temp)
    {
        NPlayer _nPlayer = PunManager.Instance.GetPlayerInfo(_actorNum);

        if (_temp > 0)
            UIManager.Instance.LoadPrefabAsset<GameObject>("Eff/HealEff", _nPlayer.LoadHealItemEffAssetCom);
        else if (_temp == 0)
            GetShieldItem(_nPlayer);
    }

    public void ShowEffBullet(int _actorNum)
    {
        NPlayer _nplayer = PunManager.Instance.GetPlayerInfo(_actorNum);
        UIManager.Instance.LoadPrefabAsset<GameObject>("Eff/FX_Snow_Bullet",_nplayer.LoadShotBulletCom);
    }

    public void GetShieldItem(NPlayer _nPlayer)
    {
        if (GamePlayerExistShield(_nPlayer.ActorNum) == false)
        {
            UIManager.Instance.LoadPrefabAsset<GameObject>("Eff/ShieldEff", _nPlayer.LoadShieldItemEffAssetCom);
            GamePlayerInfoDic[_nPlayer.ActorNum].GetShield = true;
        }

        if (_nPlayer.ActorNum == PunManager.MyPlayer.ActorNum)
        {
            ShieldSustainTime = (float)DataTblMng.Instance.GetConfigData(ConfigType.SF_SHIELD_SUSTAIN_TIME);

            UIGame.Instance.SFBuffInitLoop(SnowFightItempType.ITEM_SHIELD);
        }
    }

    void SustainShieldItemTime()
    {
        if (ShieldSustainTime > 0)
        {
            if ((ShieldSustainTime -= Time.deltaTime) <= 0)
            {
                RemoveShield();
                UIGame.Instance.SFBuffItemHide(SnowFightItempType.ITEM_SHIELD);
            }
        }
    }

    void RemoveShield()
    {
        if (MiniGameSt != MiniGameSt.PLAY)
            return;

        if (GamePlayerExistInDic(PunManager.MyPlayer.ActorNum))
        {
            //            PunManager.MyRPC.BRSFHideShield();
            PunManager.MyRPC.SendTypeRPC((int)RPCType.BRSF_HIDE_SHIELD);
           
        }      
    }

   
    public void GetMachineGunItem(NPlayer _nPlayer)
    {
        if (PunManager.Instance.IsMyPlayer(_nPlayer.ActorNum))
        {
            UIManager.Instance.LoadPrefabAsset<GameObject>("Eff/MachineGunEff", PunManager.MyPlayer.LoadMachineGunItemEffAssetCom);

            MachineGunSustainTime = DataTblMng.Instance.GetConfigData(ConfigType.SF_MACHINEGUN_SUSTAIN_TIME);
            PunManager.MyPlayer.fireTr.BulletReloadTime = (float)DataTblMng.Instance.GetConfigData(ConfigType.SF_LARGE_GUN_RELOAD_TIME);

            UIGame.Instance.SFBuffInitLoop(SnowFightItempType.ITEM_MACHINEGUN);
        }

        ChangeGamePlayerMachinegun(_nPlayer.ActorNum, true);
    }

    void SustainMachinegunTime()
    {
        if (MachineGunSustainTime > 0)
        {
            if ((MachineGunSustainTime -= Time.deltaTime) <= 0)
            {
                PunManager.MyPlayer.fireTr.BulletReloadTime = (float)DataTblMng.Instance.GetConfigData(ConfigType.SF_SMALL_GUN_RELOAD_TIME);
                //                PunManager.MyRPC.BRSFLoadSmallGun();
                PunManager.MyRPC.SendTypeRPC((int)RPCType.BRSF_LOAD_SMALL_GUN);

                UIGame.Instance.SFBuffItemHide(SnowFightItempType.ITEM_MACHINEGUN);
            }
        }
    }

    public void GetPowerBulletItem(int _actorNum, bool _get)
    {
        UIManager.Instance.LoadPrefabAsset<GameObject>("Eff/PowerBulletEff", PunManager.MyPlayer.LoadPowerBulletItemEffAssetCom);

        GamePlayerInfoDic[_actorNum].GetPowerBullet = _get;
        PowerBulletSustainTime = (float)DataTblMng.Instance.GetConfigData(ConfigType.SF_POWER_BULLET_SUSTAIN_TIME);

        if(_actorNum ==PunManager.MyPlayer.ActorNum)
        {
            UIGame.Instance.SFBuffInitLoop(SnowFightItempType.ITEM_POWER_BULLET);
        }

    }

    void SustainPowerBulletItemTime()
    {
        if(PowerBulletSustainTime > 0)
        {
            if ((PowerBulletSustainTime -= Time.deltaTime) <= 0)
            {
                RemovePowerBullet();
                UIGame.Instance.SFBuffItemHide(SnowFightItempType.ITEM_POWER_BULLET);
            }
        }
    }

    void RemovePowerBullet()
    {
        if (MiniGameSt != MiniGameSt.PLAY)
            return;

        if (GamePlayerExistInDic(PunManager.MyPlayer.ActorNum))
        {
            GamePlayerInfoDic[PunManager.MyPlayer.ActorNum].GetPowerBullet = false;
        }
    }
 
    public void GetSpeedItem()
    {
        UIManager.Instance.LoadPrefabAsset<GameObject>("Eff/SpeedEff", PunManager.MyPlayer.LoadSpeedItemEffAssetCom);

        PunManager.MyPlayer.nPlayerMove.SetMoveSpeed(DataTblMng.Instance.GetConfigData(ConfigType.SF_SPEED_UP_ITEM_MOVE_SPEED));

        UIGame.Instance.SpeedUpSustainTime = (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_SPEED_UP_SUSTAIN_TIME);
        SpeedUpSustainTime = (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_SPEED_UP_SUSTAIN_TIME);

        UIGame.Instance.SFBuffInitLoop(SnowFightItempType.ITEM_SPEEDUP);
    }

    void SustainSpeedUpItemTime()
    {
        if (SpeedUpSustainTime > 0)
        {
            SpeedUpSustainTime -= Time.deltaTime;

            if (SpeedUpSustainTime <= 0)
            {
                PunManager.MyPlayer.nPlayerMove.SetMoveSpeed();
                UIGame.Instance.SFBuffItemHide(SnowFightItempType.ITEM_SPEEDUP);
            }

        }
    }

    public void RespawnInvincibility(object[] _data)
    {
        if (GameNo != JsonDecode.ToInt(_data[0]))
            return;

        StartCoroutine(Invincibility(JsonDecode.ToInt(_data[1])));
    }

    IEnumerator Invincibility(int _actorNum)
    {
        GamePlayerInfoDic[_actorNum].Invinsivility = true;
        yield return new WaitForSeconds(2f);
        PunManager.MyRPC.SendRPC(new object[] { (int)RPCType.BRSF_UNINVINCIBILITY, (int)GameNo, _actorNum });
    }

    public void UnInvincibility(object[] _data)
    {
        if (GameNo != JsonDecode.ToInt(_data[0]))
            return;

        GamePlayerInfoDic[JsonDecode.ToInt(_data[1])].Invinsivility = false;
    }
    private void Update()
    {
        if(MiniGameKind==MiniGameKind.SNOW_FIGHT)
        {
            SustainPowerBulletItemTime();
            SustainShieldItemTime();
            SustainMachinegunTime();
            SustainSpeedUpItemTime();

            CheckPossibleGetSnowItem();
            CheckSnowBulletPlayerHit();
        }
    }
    public bool GamePlayerCanShoot(int _actorNum)
    {
        if (GamePlayerExistInDic(_actorNum))
        {
            if (GamePlayerInfoDic[_actorNum].BulletCnt > 0)
                return true;
        }
        return false;
    }

    public bool GamePlayerExistMachinegun(int _actorNum)
    {
        if(GamePlayerExistInDic(_actorNum))
        {
            return GamePlayerInfoDic[_actorNum].GetMachinegun;
        }
        return false;
    }

    public bool GamePlayerExistPowerBullet(int _actorNum)
    {
        if (GamePlayerExistInDic(_actorNum))
        {
            if (GamePlayerInfoDic[_actorNum].GetPowerBullet)
                return true;
        }
        return false;
    }

    public bool GamePlayerExistShield(int _actorNum)
    {
        if (GamePlayerExistInDic(_actorNum))
        {
            if (GamePlayerInfoDic[_actorNum].GetShield)
                return true;
        }
        return false;
    }

    public int GamePlayerBulletCnt(int _actorNum)
    {
        if(GamePlayerExistInDic(_actorNum))
            return GamePlayerInfoDic[_actorNum].BulletCnt;
        return 0;
    }

    public int GamePlayerHpCnt(int _actorNum)
    {
        if (GamePlayerExistInDic(_actorNum))
            return GamePlayerInfoDic[_actorNum].HPCnt;
        return 0;
    }
    
    public void ChangeGamePlayerMachinegun(int _actorNum, bool _state)
    {
        if (GamePlayerExistInDic(_actorNum))
        {
            if(_state)
                GamePlayerInfoDic[_actorNum].GetMachinegun = _state;
            else
                GamePlayerInfoDic[_actorNum].GetMachinegun = _state;
        }
           
    }

    public bool IsGamePlayerDie(int _actorNum)
    {
        if (GamePlayerExistInDic(_actorNum))
            return GamePlayerInfoDic[_actorNum].Died;
        return false;
    }

    public int PointInfo(bool _isPowerBullet)
    {
        if (_isPowerBullet)
        {
            return (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_POWER_BULLET_POINT);
        }
        else
        {
            return (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_NORMAL_BULLET_POINT);
        }
    }

    public int DamageInfo(bool _isPowerBullet)
    {
        if(_isPowerBullet)
        {
            return (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_DAMAGE_POWER_BULLET);
        }
        else
        {
            return (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_DAMAGE_NORMAL_BULLET);
        }
    }

    public int BulletCntInfo(int _actorNum)
    {
        if(GamePlayerExistMachinegun(_actorNum))
        {
            return (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_RALGE_USED_BULLET_CNT);
        }
        else 
        {
            return (int)DataTblMng.Instance.GetConfigData(ConfigType.SF_NORMAL_USED_BULLET_CNT);
        }
    }

    #region TODO_TEST

    public void ShowPlayerInfo(int _actorNum)
    {
        if (MiniGameKind != MiniGameKind.SNOW_FIGHT)
            return;

        if(GamePlayerInfoDic !=null)
        {   
            if (GamePlayerInfoDic.ContainsKey(_actorNum))
            { 
                UIGame.Instance.TxtDeathCnt.text=GamePlayerInfoDic[_actorNum].DeathCnt.ToString();
                UIGame.Instance.TxtKillCnt.text = GamePlayerInfoDic[_actorNum].KillCnt.ToString();
                UIGame.Instance.TxtHitCnt.text = GamePlayerInfoDic[_actorNum].HitCnt.ToString();
    
            }
        }
    }
#endregion 
}

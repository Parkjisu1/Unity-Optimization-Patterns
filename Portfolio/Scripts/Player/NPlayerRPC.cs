using Photon.Pun;
using UnityEngine;
using PN = Photon.Pun.PhotonNetwork;
using Photon.Realtime;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;

public class NplayerRPC :MonoBehaviourPun
{
    private RPCType rpcType;

    private Nplayer nPlayer;
    private NplayerMove nPlayerMove;


    private void Awake()
    {
        GetPlayerComponent();
    }

    void GetPlayerComponent()
    {
        if (nPlayer == null)
            nPlayer = GetComponent<Nplayer>();

        if (nPlayerMove == null)
            nPlayerMove = GetComponent<NplayerMove>();
    }
    //값을 넘길때
    public void SendRPC(object[] _data, RpcTarget _rpcTarget = RpcTarget.All)
    {
        GetPlayerComponent();

        photonView.RPC("ReceiveRPC",_rpcTarget,_data);

    }

    [PunRPC]
    public void ReceiveRPC(object[] _data)
    {
        rpcType = (RPCType)JsonDecode.ToInt(_data[0]);
        Debug.Log($"<color=red> RPC Type is : {rpcType}</color>");
        object[] _rpcData = new object[_data.Length - 1];

        for (int i = 1; i < _data.Length; i++)
            _rpcData[i - 1] = _data[i];

        RpcTypeFunctionCall(_rpcData);
    }
    //액션을 넘길때 
    public void SendTypeRPC(int _rpcType, RpcTarget _rpcTarget = RpcTarget.All)
    {
        GetPlayerComponent();
        if (nPlayer == null)
            nPlayer = GetComponent<Nplayer>();

        if (nPlayerMove == null)
            nPlayerMove = GetComponent<NplayerMove>();

        photonView.RPC("ReceiveTypeRPC",_rpcTarget,_rpcType);
    }

    [PunRPC]
    public void ReceiveTypeRPC(int _type)
    {
        rpcType = (RPCType)_type;

        if (nPlayer == null || nPlayerMove == null)
            return;

        RpcTypeFunctionCall(null);
    }
    void RpcTypeFunctionCall(object[] _rpcData)
    {
        GetPlayerComponent();

        switch (rpcType)
        {
            case RPCType.NONE: NoneRPC(_rpcData);break;

            case RPCType.SPAWN_MONSTER: PunManager.Instance.SpawnMonster();break;
            case RPCType.SPAWN_BOSS_MONSTER:PunManager.Instance.SpwawnBossMonster();break;

            case RPCType.ANI_MOVEMENT: nPlayerMove.AniMovementRPC();break;

            case RPCType.SHOW_ATTACK_EFF:MakeAttackEffRPC();break;
            case RPCType.ATTACK_MONSTER: CalculateMonsterHPRPC(_rpcData); break;

            case RPCType.DEAD:AfterMonsterDeadRPC(_rpcData);break;

            case RPCType.RESPAWN_MONSTER: RespawnMonsterRPC(_rpcData); break;

            case RPCType.ATTACK_PLAYER: AttackPlayerRPC(_rpcData); break;

        }
    }

    void NoneRPC(object[] _data)
    {
        Debug.Log($"<color=red> This is NoneRPC , Nothing Will be....</color>");
    }

    void  CalculateMonsterHPRPC(object[] _data)
    {
        for(int i=0; i<nPlayer.HitTargets.Count;i++)
        {
            if(nPlayer.HitTargets[i].GetPhotonView().ViewID==JsonDecode.ToInt(_data[0]))
            {
                Nplayer monster = nPlayer.HitTargets[i].GetComponent<Nplayer>();
                monster.CalculateMonsterHP(_data);
            }
            //nPlayer.HitTargets[i].CalculateMonsterHP(_data);
        }
        
    }
    void MakeAttackEffRPC()
    {
        Nplayer monster = this.gameObject.GetComponent<Nplayer>();
        monster.MakeAttackEff();
    }

    void AfterMonsterDeadRPC(object[] _data)
    {
        //경험치
        if(PunManager.MyPlayer.photonView.ViewID==JsonDecode.ToInt(_data[2]))
        {
            //TODO 테스트 
            PunManager.MyPlayer.EXP += 10;
            UserData.Instance.EXP += 10;
        }
        else
        {
            if(PunManager.Instance.OtherPlayer.ContainsKey(JsonDecode.ToInt(_data[2])))
            {
                PunManager.Instance.OtherPlayer[JsonDecode.ToInt(_data[2])].EXP += 10;
            }
        }
        
      
        for(int i= 0; i<PunManager.Instance.MonsterList.Count;i++)
        {
            if (PunManager.Instance.MonsterList[i].photonView.ViewID == JsonDecode.ToInt(_data[1]))
            {
                PunManager.Instance.MonsterList[i].IsDead = true;
                PunManager.Instance.CheckMonsterState();
            }
        }
    }

    void RespawnMonsterRPC(object[] _data)
    {
        Debug.Log($"<color=cyan>{photonView.ViewID} is {nPlayer.IsDead}</color>");
        nPlayer.AfterMonsterDead();
    }

    //TODO 테스트 매개변수로 데미지도 추가
    void AttackPlayerRPC(object[] _data)
    {
        int _tempPlayerNum = JsonDecode.ToInt(_data[0]);

        if(PunManager.MyPlayer.photonView.ViewID ==_tempPlayerNum)
        {
            PunManager.MyPlayer.HP -= 10;
            return;
        }
        else if(PunManager.Instance.OtherPlayer.ContainsKey(_tempPlayerNum))
        {
            PunManager.Instance.OtherPlayer[_tempPlayerNum].HP -= 10;
        }

    }
}

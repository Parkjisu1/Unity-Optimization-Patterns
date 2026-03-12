using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;


public class PlayerData 
{
    //추후 데이터 테이블에 추가할 내용 
    public Job job { get; private set; }
    public int Idx { get; set; }
    public float HP { get; private set; }
    public float MP { get; private set; }
    public float AttackDam { get; private set; }
    public float Defence { get; private set; }
    public float Exp { get; private set; }


    public PlayerData(Dictionary<string,object> _dataDic)
    {
        Idx = JsonDecode.ToIntObscured("INDEX", _dataDic);
        HP = JsonDecode.ToFloatObscured("HP", _dataDic);
        MP = JsonDecode.ToFloatObscured("MP", _dataDic);
        AttackDam = JsonDecode.ToFloatObscured("ATTACK_DAMAGE", _dataDic);
        Defence = JsonDecode.ToFloatObscured("DEFENCE", _dataDic);
        Exp = JsonDecode.ToFloatObscured("EXP", _dataDic);
    }
}


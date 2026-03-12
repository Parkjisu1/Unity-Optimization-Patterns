
using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class MonsterData 
{
    public int Idx { get; set; }  
    public string Name { get; set; }
    public float HP { get; private set; }
    public float AttackDam { get; private set; }
    public float Defence { get; private set; }
    public float Exp { get; private set; }

    public MonsterData(Dictionary<string,object> _dataDic)
    {
        Idx = JsonDecode.ToIntObscured("INDEX", _dataDic);
        Name = JsonDecode.ToStringObscured("Name", _dataDic);
        HP = JsonDecode.ToFloatObscured("HP", _dataDic);
        AttackDam = JsonDecode.ToFloatObscured("ATTACK_DAMAGE", _dataDic);
        Defence = JsonDecode.ToFloatObscured("DEFENCE", _dataDic);
        Exp = JsonDecode.ToFloatObscured("EXP", _dataDic);
    }
}

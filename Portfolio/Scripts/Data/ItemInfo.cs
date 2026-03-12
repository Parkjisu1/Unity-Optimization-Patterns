using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;


public class ItemData
{
    public ObscuredInt Idx { get; set; }
    public ObscuredInt OverLap { get; set; }
    public int category { get; set; }
    public ObscuredInt PurchaseGoodType { get; set; }
    public ObscuredInt PuchasePrice { get; set; }
    public ObscuredInt Setparts { get; set; }
    public float HP { get; set; }
    public float Damage { get; set; }
    public float Defence { get; set; }

    public ItemData(Dictionary<string,object> _dataDic)
    {
        Idx = JsonDecode.ToIntObscured("INDEX", _dataDic);
        OverLap = JsonDecode.ToIntObscured("OVERLAP", _dataDic);
        category = JsonDecode.ToIntObscured("CATEGORY", _dataDic);
        PurchaseGoodType = JsonDecode.ToIntObscured("PURCHASE_GOODS_TYPE", _dataDic);
        PuchasePrice = JsonDecode.ToIntObscured("PURCHASE_PRICE", _dataDic);
        Setparts = JsonDecode.ToIntObscured("SET_PARTS", _dataDic);
        HP = JsonDecode.ToIntObscured("HP", _dataDic);
        Damage = JsonDecode.ToIntObscured("DAMAGE", _dataDic);
        Defence = JsonDecode.ToFloatObscured("DEFENCE", _dataDic);
    }
}

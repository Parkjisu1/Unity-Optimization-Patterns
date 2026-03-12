
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;

public enum GameDataType
{
    NONE = 0,

    LEVEL,
    MONSTER,

    ITEM,

}
public class DataTblMng : Singletone<DataTblMng>
{
    private Dictionary<ObscuredInt, PlayerData> playerLevelTable = new Dictionary<ObscuredInt, PlayerData>();
    private Dictionary<ObscuredInt, MonsterData> monsterLevelTable = new Dictionary<ObscuredInt, MonsterData>();
    private Dictionary<ObscuredInt, ItemData> ItemDataTable = new Dictionary<ObscuredInt, ItemData>();
   //실질적으로사용해야하는것
    private Dictionary<ObscuredInt, List<ItemData>> _itempDataTable = new Dictionary<ObscuredInt, List<ItemData>>();
    #region Init All Table
    public void InitAllTable()
    {
        playerLevelTable.Clear();
        monsterLevelTable.Clear();
        ItemDataTable.Clear();
        _itempDataTable.Clear();
    }
    #endregion


    #region Local Data Load

    public void LoadLocalDataAll()
    {
        LoadLocalLevelSysData();
        LoadLocalMonsterSysData();
        LoadLocalItemSysData();
    }

    List<object> LoadJson(string _fileName)
    {
        string _tempStr = (Resources.Load(string.Format("JsonData/{0}", _fileName)) as TextAsset).text;
        List<object> _rtnObjList = (List<object>)MiniJSON.Json.Deserialize(AES.Decrypt256(_tempStr, AES.AESKey256[0], AES.AESKey256[1]));

        return _rtnObjList;
    }
    #endregion
    #region ItemSystem
    void LoadLocalItemSysData()
    {
        List<object> _jsonDataList = LoadJson(GameDataType.ITEM.ToString());
        for(int i= 0; i < _jsonDataList.Count; i++)
        {
            AddItemData(new ItemData((Dictionary<string, object>)_jsonDataList[i]));
        }
    }

    void AddItemData(ItemData _itemData)
    {
        if (!_itempDataTable.ContainsKey(_itemData.category))
            _itempDataTable.Add(_itemData.category, new List<ItemData>());
            _itempDataTable[_itemData.category].Add(_itemData);
        //ItemDataTable.Add(_itemData.Idx, _itemData);
    }

    public List<ItemData> GetItemSysDataList()
    {
        List<ItemData> _temp = new List<ItemData>();

        for(int i= 0; i < ItemDataTable.Count; i++)
        {
            _temp.Add(ItemDataTable[i]);
        }
        return _temp;
    }

    public List<ItemData> GetPerCateItemDataList(int _cate)
    {
        List<ItemData> _temp = new List<ItemData>();

        for(int i= 0; i<_itempDataTable.Count;i++)
        {
            if (_itempDataTable.ContainsKey(_cate))
            {
                _temp = _itempDataTable[_cate];
            }

        }

        //for (int i = 0; i < ItemDataTable.Count; i++)
        //{
        //    if (ItemDataTable[i].category == _cate)
        //        _temp.Add(ItemDataTable[i]);
        //}
        return _temp;
    }

    public ItemData GetItemData(int _part,int _item)
    {
        return _itempDataTable[_part][_item];
    }
    #endregion
    #region MonsterSystem
    void LoadLocalMonsterSysData()
    {
        List<object> _jsonDataList = LoadJson(GameDataType.MONSTER.ToString());
        for(int i=0; i < _jsonDataList.Count;i++)
        {
            AddMonsterData(new MonsterData((Dictionary<string, object>)_jsonDataList[i]));
        }
    }
    
     void AddMonsterData(MonsterData _monsterData)
    {
        monsterLevelTable.Add(_monsterData.Idx, _monsterData);
    }

    public List<MonsterData> GetMonsterSYsDataList(PlayerKind _playerKind)
    {
        List<MonsterData> _temp = new List<MonsterData>();

        if(_playerKind == PlayerKind.MONSTER)
        {
            for(int i= 0; i <monsterLevelTable.Count; i++)
            {
                _temp.Add(monsterLevelTable[i]);
            }

            return _temp;
        }
        return null;
    }

    public MonsterData GetMonsterData(int _kind)
    {
        if (monsterLevelTable.ContainsKey(_kind))
            return monsterLevelTable[_kind];

        return null;
    }
    #endregion
    #region LevelSystem

    void LoadLocalLevelSysData()
    {
        List<object> _jsonDataList = LoadJson(GameDataType.LEVEL.ToString());
        for(int i= 0; i< _jsonDataList.Count; i++)
        {
            AddLevelSysData(new PlayerData((Dictionary<string, object>)_jsonDataList[i]));
        }
    }

    void AddLevelSysData(PlayerData _playerData)
    {
        playerLevelTable.Add(_playerData.Idx,_playerData);
    }

    public List<PlayerData> GetPlayerLevelSysDataList(PlayerKind _playerKind)
    {
        List<PlayerData> _temp = new List<PlayerData>();

        if(_playerKind ==PlayerKind.PLAYER )
        {
            for(int i=1; i < playerLevelTable.Count+1; i++)
            { 
                _temp.Add(playerLevelTable[i]);
            }

            return _temp;
        }
        return null;
    }

    public PlayerData GetPlayerLevelData(int _level)
    {
        if (playerLevelTable.ContainsKey(_level))
            return playerLevelTable[_level];

        return null;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region TODO 테스트
public enum Test
{
  NONE =0 ,
  FIRST,
  SECOND,
  THIRD,
}

public enum Test_objectType
{
  TUTORIAL_WALL,
  TUTORIAL_FX,
}


public enum UtilKind
{ 
  HP,
  MP,
  EXP,
}
#endregion

#region GameSequence
public enum PlayerState
{
    NONE = 0,
    DOING_FIRST_TUTORIAL,
    COMPLETE_FIRST_TUTORIAL,
    DOING_SECOND_TUTORIAL,
    COMPLETE_SECOND_TUTORIAL,
    DOING_THIRD_TUTORIAL,
    COMPLETE_THIRD_TUTORIAL,

    COMPLETE_ALL_TUTORIAL,
}
#endregion

#region PLAYERINFO
public enum Job
{
   NONE,
   WIZARD,
   HOKE,
   WORRIOR,
}

#endregion
#region MapType

public enum MapType
{
  NONE=-1,
  MAIN_TOWN=0,
  MONSTERFIELD,
  MY_HOME,
}

#endregion

#region PlayerKind
public enum PlayerKind
{
  NONE =0,
  PLAYER=1,
  NPC=2,
  MONSTER=3,

}
#endregion

#region Player Jump
public enum JumpSt
{
    NONE = 0,
    START = 1,
    JUMPING = 2,
}
#endregion

public enum AttackSt
{ 
    NONE =0,
    ATTACK =1,

}

public enum RollSt
{ 
   NONE = 0, 
   ROLL =1,
}
#region NPC
public enum NpcKind
{
   SHOP=0,
   STAFF,
}

#endregion
#region Monster
public enum MonsterState
{ 
  IDLE =0,
  TRACE,
  ATTACK,
  DEAD,
}
public enum MonsterKind
{
    None = 0,
    Wolf = 1,
    Elephant =2,
}

#endregion

#region Goods
public enum GoodsType
{
    GAME_MONEY=1,
    CRISTAL,
}
#endregion

#region Equip
public enum EquipPart
{ 
   GUN,
   SWORD,
   ARROW,
   SHORT_SWORD,
}
#endregion

#region Costum
public enum CostumeParts
{
    HEAD=1,

    HAND=2,

    UPPER=3,
    DOWNER=4,

    FOOT=5,

    GLOVE=6,

    NECK=7,

    TOOL=8,//weapon


    NONE =99,
}
#endregion
#region Shop
public enum ItemKind
{ 
    UpperCloth =1,
    DownCloth =2,
    Shoes =3,
    Accesory =4,
    Glove = 5,
    Cap = 6,
    Weapon =7,
}
#endregion
#region Camera
public enum CamTargetType
{
    NONE = 0,
    PLAYER,
    MONSTER,

    MAX,
}

#endregion


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using TDBID = System.UInt64;
public class EnemyUIDict 
{

    public static Dictionary<string, Action<TDBID>> ButtonTypeToEventUp = new Dictionary<string, Action<TDBID>>();
}

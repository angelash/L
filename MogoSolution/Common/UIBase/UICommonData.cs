using System;
using System.Collections.Generic;
using UnityEngine;
using Mogo.GameData;

public class UICommonData
{
    #region 常用物体

    public class UITransformNames
    {
        public const string NormalMainUI = "NormalMainUI";
        public const string MogoMainUIPanel = "MogoMainUIPanel";
        public const string MogoGlobalUIPanel = "MogoGlobalUIPanel";
        public const string MogoGlobalUIPanelRoot = "MogoGlobalUIPanelRoot";
        public const string MogoGlobal2UIPanel = "MogoGlobal2UIPanel";
        public const string MogoMainUICamera = "MogoMainUICamera";
        public const string MogoGlobalUICamera = "GlobalUICamera";
        public const string MogoGlobal2UICamera = "Global2UICamera";
        public const string BattleMainUI = "MainUI";
        public const string BattleBillboardList = "BattleBillboardList";
    }

    public static Transform NormalMainUIPanel { get { return GetObject<Transform>(UITransformNames.NormalMainUI); } }
    public static Transform MogoMainUIPanel { get { return GetObject<Transform>(UITransformNames.MogoMainUIPanel); } }
    public static Transform MogoGlobalUIPanel { get { return GetObject<Transform>(UITransformNames.MogoGlobalUIPanel); } }
    public static Transform MogoGlobalUIPanelRoot { get { return GetObject<Transform>(UITransformNames.MogoGlobalUIPanelRoot); } }
    public static Transform MogoGlobal2UIPanel { get { return GetObject<Transform>(UITransformNames.MogoGlobal2UIPanel); } }
    public static Transform BattleMainUI { get { return GetObject<Transform>(UITransformNames.BattleMainUI); } }
    public static Transform BattleBillboardList { get { return GetObject<Transform>(UITransformNames.BattleBillboardList); } }
    public static Camera MogoMainUICamera { get { return GetObject<Camera>(UITransformNames.MogoMainUICamera); } }
    public static Camera MogoGlobalUICamera { get { return GetObject<Camera>(UITransformNames.MogoGlobalUICamera); } }
    public static Camera MogoGlobal2UICamera { get { return GetObject<Camera>(UITransformNames.MogoGlobal2UICamera); } }


    static Dictionary<string, UnityEngine.Object> m_transformDic = new Dictionary<string, UnityEngine.Object>();

    public static void RegisterObject<T>(string name, T obj) where T : UnityEngine.Object
    {
        if (m_transformDic.ContainsKey(name) == true)
            m_transformDic[name] = obj;
        else
            m_transformDic.Add(name, obj);
    }

    public static T GetObject<T>(string name) where T : UnityEngine.Object
    {
        if (m_transformDic.ContainsKey(name) ==  true)
            return m_transformDic[name] as T;
        else
            return default(T);
    }

    #endregion

    public static string ConvertToTimeString(int seconds)
    {
        int hour = seconds / 3600;
        int min = seconds % 3600 / 60;
        int sec = seconds % 60;
        return string.Concat(hour.ToString("d2"), ":", min.ToString("d2"), ":", sec.ToString("d2"));
    }

    public static string ConvertToTimeLanguage(int seconds)
    {
        int hour = seconds / 3600;
        int min = seconds % 3600 / 60;
        int sec = seconds % 60;
        int day = hour / 24;
        hour = hour % 24;

        string text = "";

        if (day > 0)
            text = string.Concat(text, day, LanguageData.GetContent(7100));
        if (hour > 0)
            text = string.Concat(text, hour, LanguageData.GetContent(7101));
        if (min > 0)
            text = string.Concat(text, min, LanguageData.GetContent(7102));
        if (sec > 0 || (day == 0 && hour == 0 && min == 0))
            text = string.Concat(text, sec, LanguageData.GetContent(7103));
        return text;
    }

    public static string ConvertToTimeLanguage(int seconds, bool ignoreSecond)
    {
        if (ignoreSecond == false)
            return ConvertToTimeLanguage(seconds);

        int hour = seconds / 3600;
        int min = seconds % 3600 / 60;
        int day = hour / 24;
        hour = hour % 24;
        
        string text = "";

        if (day > 0)
            text = string.Concat(text, day, LanguageData.GetContent(7100));
        if (hour > 0)
            text = string.Concat(text, hour, LanguageData.GetContent(7101));
        if (min > 0 || (day == 0 && hour == 0))
            text = string.Concat(text, min, LanguageData.GetContent(7102));

        return text;
    }
}

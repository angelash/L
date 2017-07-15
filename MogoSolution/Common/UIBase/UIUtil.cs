using System;
using UnityEngine;

public static class ActionExtensions
{
    public static void SafeInvoke(this Action mAction)
    {
        if (mAction != null)
            mAction();
    }

    public static void SafeInvoke<T>(this Action<T> mAction, T param)
    {
        if (mAction != null)
            mAction(param);
    }

    public static void SafeSetActive(this GameObject obj, bool bActive)
    {
        if (obj != null)
            obj.SetActive(bActive);
    }

    public static void SafeSetActive(this Transform obj, bool bActive)
    {
        if (obj != null)
            obj.gameObject.SetActive(bActive);
    }

    public static string TrimSuffix(this string str)
    {
        str.Replace("\\", "/");
        if (-1 == str.LastIndexOf('/'))
        {
            if (-1 == str.LastIndexOf('.'))
            {
                return str;
            }
            else
            {
                return str.Substring(0, str.LastIndexOf('.'));
            }
        }
        else
        {
            if (-1 == str.LastIndexOf('.'))
            {
                return str.Substring(str.LastIndexOf('/') + 1);
            }
            else
            {
                return str.Substring(str.LastIndexOf('/') + 1, str.LastIndexOf('.'));
            }
        }
    }
}

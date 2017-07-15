using Mogo.Util;
using Mogo.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

public class Point2D
{
    public double x;
    public double y;

    public Point2D()
    {
        x = 0;
        y = 0;
    }

    public Point2D(double x, double y)
    {
        this.x = x;
        this.y = y;
    }
}

public class Rectangle
{
    public double x;
    public double y;
    public double width;
    public double height;

    public Rectangle(double x, double y, double width, double height)
    {
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
    }
}
public static class UtilsEx
{
    public static IPAddress GetIP()
    {
        string hostname = Dns.GetHostName();
        IPHostEntry localhost = Dns.GetHostEntry(hostname);
        IPAddress ip = localhost.AddressList[0];
        return ip;
    }

    /// <summary>
    /// 支持深层孩子
    /// </summary>
    static public Transform GetChild(Transform transform, string boneName)
    {
        Transform child = transform.FindChild(boneName);
        if (child == null)
        {
            foreach (Transform c in transform)
            {
                child = GetChild(c, boneName);
                if (child != null) return child;
            }
        }
        return child;
    }

    public static Type GetTypeByExtension(String extension)
    {
        //Debug.LogWarning(extension);
        switch (extension.ToLower())
        {
            case ".prefab":
                return typeof(GameObject);
            case ".fbx":
                return typeof(UnityEngine.Object);
            case ".mat":
                return typeof(Material);
            case ".png":
            case ".tga":
                return typeof(Texture);
            case ".anim":
                return typeof(AnimationClip);
            case ".ttf":
                return typeof(Font);
            case ".shader":
                return typeof(Shader);
            case ".unity":
                return typeof(UnityEngine.Object);
            case ".mp3":
            case ".wav":
                return typeof(AudioClip);
            default:
                return typeof(UnityEngine.Object);
        }
    }

    public static void SetObjectLayer(int layer, GameObject obj)
    {
        if (!obj)
            return;

        obj.layer = layer;

        foreach (Transform item in obj.transform)
        {
            SetObjectLayer(layer, item.gameObject);
        }
    }

    public static string GetXMLListContent<T>(string path, List<T> data, string attrName = "record")
    {
        try
        {
            var root = new System.Security.SecurityElement("root");
            var props = typeof(T).GetProperties();
            foreach (var item in data)
            {
                if (item == null)
                {
                    LoggerHelper.Error("null item: " + path);
                    continue;
                }
                var xml = new System.Security.SecurityElement(attrName);
                foreach (var prop in props)
                {
                    var type = prop.PropertyType;
                    String result = String.Empty;
                    object obj = prop.GetGetMethod().Invoke(item, null);
                    if (obj == null)
                    {
                        LoggerHelper.Error("null obj: " + prop.Name);
                        continue;
                    }
                    //var obj = prop.GetValue(item, null);
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        var o = typeof(Utils).GetMethod("PackMap")
                    .MakeGenericMethod(type.GetGenericArguments())
                        .Invoke(null, new object[] { obj, ':', ',' });
                        if (o != null)
                            result = o.ToString();
                        else
                            LoggerHelper.Error("null obj: " + prop.Name);
                    }
                    else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var o = typeof(Utils).GetMethod("PackList")
                    .MakeGenericMethod(type.GetGenericArguments())
                        .Invoke(null, new object[] { obj, ',' });

                        if (o != null)
                            result = o.ToString();
                        else
                            LoggerHelper.Error("null obj: " + prop.Name);
                    }
                    else
                    {
                        result = obj.ToString();
                    }
                    xml.AddChild(new System.Security.SecurityElement(prop.Name, result));
                }
                root.AddChild(xml);
            }
            return root.ToString();
        }
        catch (Exception ex)
        {
            LoggerHelper.Except(ex);
            return "";
        }
    }

    public static List<T> LoadXML<T>(string path)
    {
        var text = Utils.LoadFile(path);
        return LoadXMLText<T>(text);
    }

    public static List<T> LoadXMLText<T>(string text)
    {
        List<T> list = new List<T>();
        try
        {
            if (String.IsNullOrEmpty(text))
            {
                return list;
            }
            Type type = typeof(T);
            var xml = XMLParser.LoadXML(text);
            Dictionary<Int32, Dictionary<String, String>> map = XMLParser.LoadIntMap(xml, text);
            var props = type.GetProperties(~System.Reflection.BindingFlags.Static);
            foreach (var item in map)
            {
                var obj = type.GetConstructor(Type.EmptyTypes).Invoke(null);
                foreach (var prop in props)
                {
                    if (prop.Name == "id")
                        prop.SetValue(obj, item.Key, null);
                    else
                        try
                        {
                            if (item.Value.ContainsKey(prop.Name))
                            {
                                var value = Utils.GetValue(item.Value[prop.Name], prop.PropertyType);
                                prop.SetValue(obj, value, null);
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerHelper.Except(ex);
                        }
                }
                list.Add((T)obj);
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.Except(ex);
            LoggerHelper.Error("error text: \n" + text);
        }
        return list;
    }

    public static List<String> GetFileNamesByDirectory(string path, string filter)
    {
        var prefabs = ResourceIndexInfo.Instance.GetFileNamesByDirectory(path);//包里面的文件
        var outterPath = Path.Combine(SystemConfig.ResourceFolder, path);
        if (Directory.Exists(outterPath))
            prefabs.AddRange(Directory.GetFiles(outterPath));//sd卡上的文件
        var result = new List<string>();
        if (Application.isEditor)
        {
            foreach (var p in prefabs)
            {
                var r = Mogo.Util.Utils.GetFileNameWithoutExtention(p.Replace("\\", "/"));
                if (r.EndsWith(filter, StringComparison.CurrentCultureIgnoreCase) && !result.Contains(r))
                {
                    result.Add(r);
                }
            }
        }
        else
        {
            foreach (var p in prefabs)
            {
                var r = Mogo.Util.Utils.GetFileNameWithoutExtention(p);
                if (r.EndsWith(filter, StringComparison.CurrentCultureIgnoreCase) && !result.Contains(r))
                {
                    result.Add(r);
                }
            }
        }

        return result;
    }

    public static void SetShader(GameObject obj, Shader shader)
    {
        SkinnedMeshRenderer[] listSMR = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        if (listSMR != null)
        {
            for (int i = 0; i < listSMR.Length; ++i)
            {
                if (listSMR[i].sharedMesh != null)
                {
                    listSMR[i].material.shader = shader;
                }

            }
        }
    }

    public static string GetRandomName()
    {
        var idLast = UnityEngine.Random.Range(1, NameOrientalLastData.dataMap.Count);
        var idMale = UnityEngine.Random.Range(1, NameOrientalMaleData.dataMap.Count);

        var result = "";
        if (NameOrientalLastData.dataMap.ContainsKey(idLast))
        {
            result = NameOrientalLastData.dataMap[idLast].name;
        }
        if (NameOrientalMaleData.dataMap.ContainsKey(idLast))
        {
            result += NameOrientalMaleData.dataMap[idLast].name;
        }
        return result;
    }
}
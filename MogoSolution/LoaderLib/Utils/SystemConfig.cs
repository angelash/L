#region 模块信息
/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：SystemConfig
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.3.19
// 模块描述：系统参数配置。
//----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Mogo.Util
{
    /// <summary>
    /// 系统参数配置。
    /// </summary>
    public partial class SystemConfig
    {
        #region 常量

        public const String ASSET_FILE_HEAD = "file://";
        public const String ASSET_FILE_EXTENSION = ".u";//打包资源必须带个一样的后缀，给它坑得一塌糊涂，没有一致后缀，资源依赖就有问题
        public const String DEFINE_LIST_FILE_NAME = "entities";
        public const string SERVER_LIST_URL_KEY = "serverlist";
        public const string VOICE_URL_KEY = "voice";
        public const string GET_PHOTO_URL_KEY = "GetPhoto";
        public const string UPLOAD_PHOTO_URL_KEY = "UploadPhoto";
        public const string PHOTO_HEAD_KEY = "PhotoHeadKey";  //头像校验使用的公钥
        public const string SERVER_GROUP_URL_KEY = "servergroup";
        public const string VERSION_URL_KEY = "version";
        public const string MARKET_URL_KEY = "market";
        public const string LOGIN_MARKET_URL_KEY = "LoginMarketData";
        //public const string PACKAGE_LIST_URL_KEY = "packagelist";
        //public const string PACKAGE_URL_KEY = "packageurl";
        //public const string APK_URL_KEY = "apkurl";
        public const string CFG_FILE = "cfg.xml";
        public const string XML = ".xml";
        public const string CONFIG_SUB_FOLDER = "data/";

        public const string NOTICE_URL_KEY = "NoticeData";
        public const string NOTICE_CONTENT_KEY = "notice";

        public const string CFG_PARENT_KEY = "parent";

        //public const string CFG_PROGRAM_VERSION =  "ProgramVersion";
        //public const string CFG_RESOUCE_VERSION = "ResouceVersion";

        //public const string CFG_PACKAGE_URL = "PackageUrl";

        //public const string CFG_APK_URL =  "ApkUrl";
        //public const string CFG_APK_MD5 = "ApkMd5";

        //public const string CFG_PACKAGE_MD5_LIST = "PackageMd5List";



        public static String ENTITY_DEFS_PATH
        {
            get
            {
                return "entity_defs/";
            }
        }

        private static String m_CONFIG_FILE_EXTENSION;
        public static String CONFIG_FILE_EXTENSION
        {
            get
            {
                if (m_CONFIG_FILE_EXTENSION == null)
                    m_CONFIG_FILE_EXTENSION = SystemSwitch.ReleaseMode || IsEditor ? XML : String.Empty;
                return m_CONFIG_FILE_EXTENSION;
            }
        }

        public readonly static String MiddlePath = "/Mogo";//iOS版本需要，为了统一，直接安卓和iOS版本都加上

        public readonly static String ConfigPath = Application.persistentDataPath + MiddlePath + "/config.xml";
        public readonly static String VersionPath = Application.persistentDataPath + MiddlePath + "/version.xml";
        public readonly static string CfgPath = String.Concat(Application.persistentDataPath, MiddlePath, "/", CFG_FILE);
        public readonly static string SystemSwitchPath = String.Concat(Application.persistentDataPath, MiddlePath, "/SystemSwitch.xml");

        public static bool IsEditor;
        public static String DataPath;
        public static string BundleIdentifier = "";

        #endregion

        #region 属性

        private static String m_resourceFolder;

        /// <summary>
        /// Android资源路径。
        /// </summary>
        public static String AndroidPath
        {
            get
            {
                return String.Concat(Application.persistentDataPath, MiddlePath, "/MogoResources/");//"/sdcard/MogoResources/";
            }
        }

        /// <summary>
        /// PC资源路径。
        /// </summary>
        public static String PCPath
        {
            get
            {
                var path = Application.dataPath + "/../MogoResources/";
                return path;
            }
        }

        /// <summary>
        /// IOS资源路径。
        /// </summary>
        public static String IOSPath
        {
            get
            {
                return String.Concat(Application.persistentDataPath, MiddlePath, "/MogoResources/");
            }
        }

        /// <summary>
        /// 资源根目录。
        /// </summary>
        public static String ResourceFolder
        {
            get
            {
                if (m_resourceFolder == null)
                {
                    if (SystemSwitch.ReleaseMode)
                        m_resourceFolder = OutterPath;
                    else
                        m_resourceFolder = String.Empty;
                }
                return m_resourceFolder;
            }
            set { }
        }

        /// <summary>
        /// 外部资源目录。
        /// </summary>
        public static String OutterPath
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                    return AndroidPath;
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                    return IOSPath;
                else if (Application.platform == RuntimePlatform.WindowsPlayer)
                    return PCPath;
                else if (Application.platform == RuntimePlatform.WindowsEditor)
                    return PCPath;
                else if (Application.platform == RuntimePlatform.OSXPlayer)
                    return IOSPath;
                else if (Application.platform == RuntimePlatform.OSXEditor)
                    return IOSPath;
                else
                    return "";
            }
        }

        public static bool IsUseOutterConfig
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (Directory.Exists(String.Concat(AndroidPath, CONFIG_SUB_FOLDER)))
                        return true;
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    if (Directory.Exists(String.Concat(IOSPath, CONFIG_SUB_FOLDER)))
                        return true;
                }
                else if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    if (Directory.Exists(String.Concat(PCPath, CONFIG_SUB_FOLDER)))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 服务器URL配置信息。
        /// </summary>
        public static List<CfgInfo> CfgInfo = new List<CfgInfo>();

        public static Dictionary<string, string> CfgInfoNew = new Dictionary<string, string>();

        #endregion

        #region 公有方法

        public static void Init(Action<bool> callback)
        {
            try
            {
                LoadCfgInfo(callback);
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex);
            }
        }

        public static void SaveConfig()
        {
            //#if UNITY_IPHONE
            //            SaveXML(Instance, ConfigPath);
            //#else
            //            SaveXMLList(ConfigPath, new List<LocalSetting>() { Instance });
            //#endif
        }

        public static void SetConfig()
        {
            System.Diagnostics.Process.Start("Explorer.exe", Application.persistentDataPath.Replace('/', '\\'));
        }

        public static void SaveCfgInfo()
        {
            //SaveXMLList(CfgPath, CfgInfo, "url");
        }

        public static string GetCfgInfoUrl(String key)
        {
            string result = "";
            if (CfgInfoNew.ContainsKey(key))
            {
                result = CfgInfoNew[key];
            }
            return result;
        }

        #endregion

        #region 私有方法

        private static void SaveXMLList<T>(string path, List<T> data, string attrName = "record")
        {
            var root = new System.Security.SecurityElement("root");
            var i = 0;
            var props = typeof(T).GetProperties();
            foreach (var item in data)
            {
                var xml = new System.Security.SecurityElement(attrName);
                foreach (var prop in props)
                {
                    var type = prop.PropertyType;
                    String result = String.Empty;
                    object obj = prop.GetGetMethod().Invoke(item, null);
                    //var obj = prop.GetValue(item, null);
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        result = typeof(Utils).GetMethod("PackMap")
                        .MakeGenericMethod(type.GetGenericArguments())
                        .Invoke(null, new object[] { obj, ':', ',' }).ToString();
                    }
                    else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        result = typeof(Utils).GetMethod("PackList")
                        .MakeGenericMethod(type.GetGenericArguments())
                        .Invoke(null, new object[] { obj, ',' }).ToString();
                    }
                    else
                    {
                        result = obj.ToString();
                    }
                    xml.AddChild(new System.Security.SecurityElement(prop.Name, result));
                }
                root.AddChild(xml);
                i++;
            }
            XMLParser.SaveText(path, root.ToString());
        }

        private static void InitConfig()
        {
            if (File.Exists(ConfigPath))
                File.Delete(ConfigPath);
            SaveConfig();
        }

        public static void LoadCfgInfo(Action<bool> callback)
        {
#if !UNITY_WEBPLAYER
            string cfgStr;
            if (File.Exists(CfgPath))
            {
                cfgStr = Utils.LoadFile(CfgPath);
                //CfgInfo = LoadXMLText<CfgInfo>(cfgStr);
                CfgInfoNew = LoadCfgInfoList(cfgStr);
                if (callback != null)
                    DriverLib.Invoke(() => callback(CfgInfoNew != null && CfgInfoNew.Count > 0 ? true : false));
            }
            else
            {
#endif
            var cfgUrl = Utils.LoadResource(Utils.GetFileNameWithoutExtention(CFG_FILE));
            LoggerHelper.Info("cfgUrl: " + cfgUrl);
            var programVerStr = "";
            var programVer = Resources.Load(VERSION_URL_KEY) as TextAsset;
            if (programVer && !String.IsNullOrEmpty(programVer.text))
            {
                programVerStr = "V" + VersionManager.Instance.GetVersionInXML(programVer.text).ProgramVersion;
            }

            var s = Application.persistentDataPath.Split('/');
            //Debug.Log(s.Length);
            for (int i = s.Length - 1; i >= 0; i--)
            {
                //Debug.Log(s[i]);
                if (s[i] == "files" && i - 1 >= 0)
                {
                    BundleIdentifier = s[i - 1];
                    LoggerHelper.Info("bundleIdentifier: " + BundleIdentifier);
                    break;
                }
            }

            Action erraction = () =>
            {
                if (callback != null)
                    DriverLib.Invoke(() => callback(false));
            };
            Action<string> suaction = null;
            suaction = (res) =>
            {
                var parentinfo = LoadCfgInfoList(res);
                foreach (var pair in parentinfo)
                {
                    if (!CfgInfoNew.ContainsKey(pair.Key))
                    {
                        CfgInfoNew.Add(pair.Key, pair.Value);
                    }
                }
                if (!string.IsNullOrEmpty(BundleIdentifier) && parentinfo.ContainsKey(BundleIdentifier))//根据包名做特殊处理
                {
                    CfgInfoNew.Clear();
                    DownloadMgr.Instance.AsynDownLoadText(parentinfo[BundleIdentifier], suaction, erraction);
                }
                else if (parentinfo.ContainsKey(programVerStr))//根据版本做特殊处理
                {
                    CfgInfoNew.Clear();
                    DownloadMgr.Instance.AsynDownLoadText(parentinfo[programVerStr], suaction, erraction);
                }
                else if (parentinfo.ContainsKey(CFG_PARENT_KEY))
                {
                    DownloadMgr.Instance.AsynDownLoadText(parentinfo[CFG_PARENT_KEY], suaction, erraction);
                }
                else if (callback != null)
                    DriverLib.Invoke(() => callback(CfgInfoNew != null && CfgInfoNew.Count > 0 ? true : false));
            };
            DownloadMgr.Instance.AsynDownLoadText(cfgUrl, suaction, erraction);
#if !UNITY_WEBPLAYER
            }
#endif
        }

        private static List<T> LoadXML<T>(string path)
        {
            var text = Utils.LoadFile(path);
            return LoadXMLText<T>(text);
        }

        private static List<T> LoadXMLText<T>(string text)
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
                                LoggerHelper.Debug("LoadXML error: " + item.Value[prop.Name] + " " + prop.PropertyType);
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


        private static Dictionary<string, string> LoadCfgInfoList(string text)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            var children = XMLParser.LoadXML(text);
            if (children != null && children.Children != null && children.Children.Count != 0)
            {
                foreach (System.Security.SecurityElement item in children.Children)
                {
                    result[item.Tag] = item.Text;
                }
            }
            return result;
        }

        private static void SaveXML(object config, string path)
        {
            var root = new System.Security.SecurityElement("root");
            root.AddChild(new System.Security.SecurityElement("record"));
            var xml = root.Children[0] as System.Security.SecurityElement;
            var props = config.GetType().GetProperties();
            foreach (var item in props)
            {
                if (item.Name.Contains("GuideTimes"))
                {
                    //dictonary
                    var temp = item.GetGetMethod().Invoke(config, null);
                    string value = "";
                    foreach (var v in temp as Dictionary<ulong, string>)
                    {
                        value = value + v.Key.ToString() + ":" + v.Value + ",";
                    }
                    xml.AddChild(new System.Security.SecurityElement(item.Name, value));
                }
                else
                {
                    var value = item.GetGetMethod().Invoke(config, null);
                    xml.AddChild(new System.Security.SecurityElement(item.Name, value.ToString()));
                }
            }
            XMLParser.SaveText(path, root.ToString());
        }
        #endregion

        //private static T LoadLuaTable<T>(string path)
        //{
        //    T t = default(T);
        //    //Type type = typeof(T);
        //    var text = Utils.LoadByteFile(path);
        //    if (text == null || text.Length == 0)
        //    {
        //        return t;
        //    }
        //    LuaTable lt;
        //    if (RPC.Utils.ParseLuaTable(text, out lt))
        //    {
        //        object result;
        //        if (Utils.ParseLuaTable(lt, typeof(T), out result))
        //            t = (T)result;
        //    }
        //    return t;
        //}

        //private static void SaveLuaTable(object config, string path)
        //{
        //    LuaTable value;
        //    RPC.Utils.PackLuaTable(config, out value);
        //    var s = RPC.Utils.PackLuaTable(value);
        //    XMLParser.SaveText(path, s);
        //}
    }

    #region 配置用类

    public class CfgInfo
    {
        //public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }
}
    #endregion
#region 模块信息

/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：GameData
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.3.26
// 模块描述：配置数据抽象类。
//----------------------------------------------------------------*/

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using HMF;
using Mogo.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Mogo.GameData
{
    public abstract class GameData
    {
        public int id { get; protected set; }

        protected static Dictionary<int, T> GetDataMap<T>() where T : GameData, new()
        {
            Dictionary<int, T> dataMap;
            var sw = new Stopwatch();
            sw.Start();
            var type = typeof (T);
            var fileNameField = type.GetField("fileName");
            if (fileNameField != null)
            {
                var fileName = fileNameField.GetValue(null) as String;
                var result = GameDataControler.Instance.FormatData<T>(fileName);
                dataMap = result as Dictionary<int, T>;
                //if (SystemSwitch.UseHmf)
                //    dataMap = dataMap.OrderByKey().ToDictionary(x => x.Key, x => x.Value);
            }
            else
            {
                dataMap = new Dictionary<int, T>();
            }
            sw.Stop();
            LoggerHelper.Info(String.Concat(type, " time: ", sw.ElapsedMilliseconds), false);
            return dataMap;
        }

        /// <summary>
        ///     为hmf提供构造实体数据方法。
        /// </summary>
        /// <param name="props"></param>
        /// <param name="item"></param>
        public virtual void SetData(PropertyInfo[] props, KeyValuePair<object, object> item)
        {
            foreach (var prop in props)
            {
                if (prop.Name == "id")
                {
                    id = (int) item.Key;
                    // var v = Utils.GetValue((string)item.Key, prop.PropertyType);
                    //prop.SetValue(this, item.Key, null);
                }
                else
                {
                    var m = (Dictionary<object, object>) item.Value;
                    if (m.ContainsKey(prop.Name))
                    {
                        var v = m[prop.Name];
                        if (v.GetType() == typeof (string))
                        {
                            var value = Utils.GetValue((string) m[prop.Name], prop.PropertyType);
                            prop.SetValue(this, value, null);
                        }

                        else
                        {
                            // UnityEngine.Debug.Log(prop.Name);

                            prop.SetValue(this, Utils.GetObjectValue(v, prop.PropertyType), null);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     为xml提供构造实体数据方法。
        /// </summary>
        /// <param name="props"></param>
        /// <param name="item"></param>
        public virtual void SetData(PropertyInfo[] props, KeyValuePair<Int32, Dictionary<String, String>> item)
        {
            foreach (var prop in props)
            {
                if (prop.Name == "id")
                {
                    prop.SetValue(this, item.Key, null);
                }
                else
                {
                    if (item.Value.ContainsKey(prop.Name))
                    {
                        try
                        {
                            var value = Utils.GetValue(item.Value[prop.Name], prop.PropertyType);
                            prop.SetValue(this, value, null);
                        }
                        catch
                        {
                            //LoggerHelper.Warning(string.Format("PropertyInfo Name={0},Type={1} value is null", prop.Name,prop.PropertyType));
                            prop.SetValue(this, null, null);
                        }
                    }
                }
            }
        }
    }

    public abstract class GameData<T> : GameData where T : GameData<T>, new()
    {
        private static Dictionary<int, T> m_dataMap;

        public static Dictionary<int, T> dataMap
        {
            get { return m_dataMap ?? (m_dataMap = GetDataMap<T>()); }
            set { m_dataMap = value; }
        }
    }

    public class GameDataControler : DataLoader
    {
        private static readonly MethodInfo m_formatMethodInfo;
        private readonly SortedList<int, Type> m_defaultData = new SortedList<int, Type>(new DuplicateKeyComparer<int>());

        static GameDataControler()
        {
            Instance = new GameDataControler();
            m_formatMethodInfo = typeof (GameDataControler).GetMethod("FormatHmfDataTem");
        }

        public static GameDataControler Instance { get; private set; }

        public void Init(Action<int, int> progress = null, Action synFinished = null, Action AsynFinished = null)
        {
            m_defaultData.Clear();

            if (SystemSwitch.UseHmf)
            {
                Action synAction = () =>
                {
                    Instance.LoadData(Instance.m_defaultData, Instance.FormatHmfData, null);
                    if (synFinished != null)
                        synFinished();
                    if (IsPreloadData)
                    {
                        Action asynAction =
                            () => { Instance.InitAsynData(Instance.FormatHmfData, progress, AsynFinished); };
                        if (SystemSwitch.ReleaseMode)
                            asynAction.BeginInvoke(null, null);
                        else
                            asynAction();
                    }
                    else
                    {
                        AsynFinished();
                    }
                };
                if (SystemSwitch.ReleaseMode && !Debug.isDebugBuild)
                    synAction.BeginInvoke(null, null);
                else
                    synAction();
            }
            else
            {
                Instance.LoadData(Instance.m_defaultData, Instance.FormatXMLData, null);
                if (synFinished != null)
                    synFinished();
                if (IsPreloadData)
                {
                    Action action = () => { Instance.InitAsynData(Instance.FormatXMLData, progress, AsynFinished); };
                    if (SystemSwitch.ReleaseMode || SystemConfig.IsEditor)
                        action.BeginInvoke(null, null);
                    else
                        action();
                }
                else
                {
                    AsynFinished();
                }
            }
        }

        /// <summary>
        ///     进行读取数据准备工作和调用处理方法
        /// </summary>
        /// <param name="formatData">格式化数据方法</param>
        /// <param name="progress">处理进度回调</param>
        /// <param name="finished">处理完成回调</param>
        private void InitAsynData(Func<string, Type, object> formatData, Action<int, int> progress, Action finished)
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                var gameDataType = new SortedList<int, Type>(new DuplicateKeyComparer<int>());
                var ass = typeof (GameDataControler).Assembly;
                var types = ass.GetTypes();
                foreach (var item in types)
                {
                    if (item.Namespace == "Mogo.GameData")
                    {
                        var type = item.BaseType;
                        while (type != null)
                        {
                            if (type == typeof (GameData) ||
                                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (GameData<>)))
                            {
                                if (!m_defaultData.ContainsValue(item))
                                {
                                    var dataAttr =
                                        Attribute.GetCustomAttribute(item, typeof (DataAttribute)) as DataAttribute;
                                    if (dataAttr != null)
                                        gameDataType.Add((int) dataAttr.Priority, item);
                                    else
                                        gameDataType.Add(int.MaxValue, item);
                                }
                                break;
                            }
                            type = type.BaseType;
                        }
                    }
                }
                LoadData(gameDataType, formatData, progress);
                sw.Stop();
                LoggerHelper.Info("InitAsynData time: " + sw.ElapsedMilliseconds);
                GC.Collect();
                if (finished != null)
                    finished();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("InitData Error: " + ex.Message);
            }
        }

        /// <summary>
        ///     加载数据逻辑
        /// </summary>
        /// <param name="gameDataType">加载数据列表</param>
        /// <param name="formatData">处理数据方法</param>
        /// <param name="progress">数据处理进度</param>
        private void LoadData(SortedList<int, Type> gameDataType, Func<string, Type, object> formatData,
            Action<int, int> progress)
        {
            var count = gameDataType.Count;
            var i = 1;
            foreach (var item in gameDataType.Values)
            {
                var p = item.GetProperty("dataMap", ~BindingFlags.DeclaredOnly);
                var fileNameField = item.GetField("fileName");
                if (p != null && fileNameField != null)
                {
                    var fileName = fileNameField.GetValue(null) as String;
                    var result = formatData(String.Concat(m_resourcePath, fileName, m_fileExtention), item);
                    //var result = FormatHmfData(String.Concat(m_resourcePath, fileName, m_fileExtention), p.PropertyType, item);

                    p.GetSetMethod().Invoke(null, new[] {result});
                }
                if (progress != null)
                    progress(i, count);
                i++;
            }
        }

        public object FormatData<T>(string fileName) where T : GameData, new()
        {
            if (SystemSwitch.UseHmf)
                return FormatHmfDataTem<T>(String.Concat(m_resourcePath, fileName, m_fileExtention));
            return FormatXMLDataTem<T>(String.Concat(m_resourcePath, fileName, m_fileExtention));
        }

        public object FormatData(string fileName, Type dicType, Type type)
        {
            if (SystemSwitch.UseHmf)
                return FormatHmfData(String.Concat(m_resourcePath, fileName, m_fileExtention), dicType, type);
            return FormatXMLData(String.Concat(m_resourcePath, fileName, m_fileExtention), dicType, type);
        }

        #region xml

        private object FormatXMLData(string fileName, Type type)
        {
            return typeof (GameDataControler).GetMethod("FormatXMLDataTem").MakeGenericMethod(type)
                .Invoke(this, new object[] {fileName});
        }

        public Dictionary<int, T> FormatXMLDataTem<T>(string fileName) where T : GameData, new()
        {
            var result = new Dictionary<int, T>();
            try
            {
                Dictionary<Int32, Dictionary<String, String>> map; //int32 为 id, string 为 属性名, string 为 属性值
                if (XMLParser.LoadIntMap(fileName, m_isUseOutterConfig, out map))
                {
                    //result = dicType.GetConstructor(Type.EmptyTypes).Invoke(null);
                    var props = typeof (T).GetProperties(); //获取实体属性
                    foreach (var item in map)
                    {
                        var t = new T(); //构造实体实例
                        t.SetData(props, item);
                        //  var v1 = Utils.GetValue((string)item.Key, typeof(Int32));
                        result.Add(item.Key, t);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex, "FormatData Error: " + fileName);
            }

            return result;
        }

        private object FormatXMLData(string fileName, Type dicType, Type type)
        {
            object result = null;
            try
            {
                Dictionary<Int32, Dictionary<String, String>> map; //int32 为 id, string 为 属性名, string 为 属性值
                if (XMLParser.LoadIntMap(fileName, m_isUseOutterConfig, out map))
                {
                    //result = dicType.GetConstructor(Type.EmptyTypes).Invoke(null);
                    result = dicType.GetConstructor(new[] {typeof (int)}).Invoke(new object[] {map.Count});
                    var props = type.GetProperties(); //获取实体属性
                    foreach (var item in map)
                    {
                        var t = type.GetConstructor(Type.EmptyTypes).Invoke(null); //构造实体实例
                        foreach (var prop in props)
                        {
                            if (prop.Name == "id")
                            {
                                prop.SetValue(t, item.Key, null);
                            }
                            else
                            {
                                if (item.Value.ContainsKey(prop.Name))
                                {
                                    var value = Utils.GetValue(item.Value[prop.Name], prop.PropertyType);
                                    prop.SetValue(t, value, null);
                                }
                            }
                        }
                        dicType.GetMethod("Add").Invoke(result, new[] {item.Key, t});
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("FormatData Error: " + fileName + "  " + ex.Message);
            }

            return result;
        }

        #endregion

        #region hmf

        private object FormatHmfData(string fileName, Type type)
        {
            return m_formatMethodInfo.MakeGenericMethod(type).Invoke(this, new object[] {fileName});
        }

        public Dictionary<int, T> FormatHmfDataTem<T>(string fileName) where T : GameData, new()
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            //long f1 = 0;
            //long f2 = 0;
            //long f3 = 0;
            var result = new Dictionary<int, T>();
            try
            {
                var bs = XMLParser.LoadBytes(fileName);
                var stream = new MemoryStream(bs);
                stream.Seek(0, SeekOrigin.Begin);

                var h = new Hmf();
                var map = (Dictionary<object, object>) h.ReadObject(stream);
                //f1 = sw.ElapsedMilliseconds;

                var props = typeof (T).GetProperties(); //获取实体属性
                var sortedData = new SortedList<int, T>();
                foreach (var item in map)
                {
                    var t = new T(); //构造实体实例
                    t.SetData(props, item);
                    //  var v1 = Utils.GetValue((string)item.Key, typeof(Int32));
                    sortedData.Add(t.id, t);
                }
                //f2 = sw.ElapsedMilliseconds;
                for (var i = 0; i < sortedData.Values.Count; i++)
                {
                    var t = sortedData.Values[i];
                    result.Add(t.id, t);
                }
                //f3 = sw.ElapsedMilliseconds;
                //sw.Stop();
                //if (sw.ElapsedMilliseconds > 100 || bs.Length > 30000)
                //    LoggerHelper.Info(String.Concat(fileName, " time: ", sw.ElapsedMilliseconds, " a: ", f1, " b: ", f2 - f1, " c: ", f3 - f2, " bs.Length: ", bs.Length), false);
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex, "FormatData Error: " + fileName);
            }

            return result;
        }

        private object FormatHmfData(string fileName, Type dicType, Type type)
        {
            var sw = new Stopwatch();
            sw.Start();

            long f1 = 0;
            long f2 = 0;
            long f3 = 0;
            object result = null;

            try
            {
                var bs = XMLParser.LoadBytes(fileName);
                var stream = new MemoryStream(bs);
                stream.Seek(0, SeekOrigin.Begin);

                var h = new Hmf();
                var map = (Dictionary<object, object>) h.ReadObject(stream);
                f1 = sw.ElapsedMilliseconds;
                result = dicType.GetConstructor(new[] {typeof (int)}).Invoke(new object[] {map.Count});

                var props = type.GetProperties(); //获取实体属性
                var tempDic = new Dictionary<int, object>();
                foreach (var item in map)
                {
                    var t = type.GetConstructor(Type.EmptyTypes).Invoke(null); //构造实体实例
                    foreach (var prop in props)
                    {
                        if (prop.Name == "id")
                        {
                            // var v = Utils.GetValue((string)item.Key, prop.PropertyType);
                            prop.SetValue(t, item.Key, null);
                        }
                        else
                        {
                            var m = (Dictionary<object, object>) item.Value;
                            if (m.ContainsKey(prop.Name))
                            {
                                var v = m[prop.Name];
                                if (v.GetType() == typeof (string))
                                {
                                    var value = Utils.GetValue((string) m[prop.Name], prop.PropertyType);
                                    prop.SetValue(t, value, null);
                                }

                                else
                                {
                                    // UnityEngine.Debug.Log(prop.Name);

                                    prop.SetValue(t, Utils.GetObjectValue(v, prop.PropertyType), null);
                                }
                            }
                        }
                    }
                    //  var v1 = Utils.GetValue((string)item.Key, typeof(Int32));
                    tempDic.Add((int) item.Key, t);
                }
                f2 = sw.ElapsedMilliseconds;
                var order = new List<int>();
                foreach (var v in tempDic)
                {
                    order.Add(v.Key);
                }
                order.Sort();
                var orderDic = new Dictionary<int, object>();
                foreach (var v in order)
                {
                    orderDic.Add(v, tempDic[v]);
                }

                foreach (var item in orderDic)
                {
                    dicType.GetMethod("Add").Invoke(result, new[] {item.Key, item.Value});
                }
                f3 = sw.ElapsedMilliseconds;


                sw.Stop();
                if (sw.ElapsedMilliseconds > 100)
                    LoggerHelper.Info(
                        String.Concat(fileName, " time: ", sw.ElapsedMilliseconds, " a: ", f1, " b: ", f2 - f1, " c: ",
                            f3 - f2, " count: ", map.Count), false);
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex, "FormatData Error: " + fileName);
            }

            return result;
        }

        #endregion
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class DataAttribute : Attribute
{
    /// <summary>
    ///     加载优先级，数值越低，优先级越高
    /// </summary>
    public DataPriority Priority { get; set; }
}

public enum DataPriority
{
    Low = 100,
    Medium = 200,
    High = 300
}

public abstract class DataLoader
{
    protected Action m_finished;
    protected bool m_isPreloadData = true;
    protected Action<int, int> m_progress;
    protected readonly String m_fileExtention;
    protected readonly bool m_isUseOutterConfig;
    protected readonly String m_resourcePath;

    protected DataLoader()
    {
        SystemConfig.IsEditor = Application.isEditor;
        SystemConfig.DataPath = Application.dataPath;

        m_isUseOutterConfig = SystemConfig.IsUseOutterConfig;
        if (m_isUseOutterConfig)
        {
            m_resourcePath = String.Concat(SystemConfig.OutterPath, SystemConfig.CONFIG_SUB_FOLDER);
            m_fileExtention = SystemConfig.XML;
        }
        else
        {
            m_resourcePath = SystemConfig.CONFIG_SUB_FOLDER; //兼容文件模块
            m_fileExtention = SystemConfig.CONFIG_FILE_EXTENSION;
        }
    }

    public bool IsPreloadData
    {
        get { return m_isPreloadData; }
        set { m_isPreloadData = value; }
    }
}
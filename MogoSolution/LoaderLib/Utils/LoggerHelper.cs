#region 模块信息
/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：LoggerHelper
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.1.17
// 模块描述：日志输出类。
//----------------------------------------------------------------*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Mogo.Util
{
    /// <summary>
    /// 日志等级声明。
    /// </summary>
    [Flags]
    public enum LogLevel
    {
        /// <summary>
        /// 缺省
        /// </summary>
        NONE = 0,
        /// <summary>
        /// 调试
        /// </summary>
        DEBUG = 1,
        /// <summary>
        /// 信息
        /// </summary>
        INFO = 2,
        /// <summary>
        /// 警告
        /// </summary>
        WARNING = 4,
        /// <summary>
        /// 错误
        /// </summary>
        ERROR = 8,
        /// <summary>
        /// 异常
        /// </summary>
        EXCEPT = 16,
        /// <summary>
        /// 关键错误
        /// </summary>
        CRITICAL = 32,
    }

    /// <summary>
    /// 日志控制类。
    /// </summary>
    /// 
    public class LoggerHelper
    {
        /// <summary>
        /// 当前日志记录等级。
        /// </summary>
        public static LogLevel CurrentLogLevels = LogLevel.DEBUG | LogLevel.INFO | LogLevel.WARNING | LogLevel.ERROR | LogLevel.CRITICAL | LogLevel.EXCEPT;
        public static string DebugFilterStr = string.Empty;
        public static uint ErrorCount = 0;
        public static uint ExceptCount = 0;
        public static bool IsReportBug = false;
        /// <summary>
        /// 强制上传下一条报错日志
        /// </summary>
        public static bool IsMustUpload = false;
        /// <summary>
        /// 是否上传所有异常日志（不限频率）
        /// </summary>
        public static bool IsUploadAllLog = false;
        public static MonoBehaviour behavior = null;
        public static string UploadLogUrl = null;
        public static ulong UploadLogTick = 10;
        public static ulong ReportLogTick = 10;

        private const Boolean SHOW_STACK = true;

        private static ulong uploadLogTime = 0;
        private static ulong reportLogTime = 0;
        private static LogWriter m_logWriter;

        public static LogWriter LogWriter
        {
            get { return m_logWriter; }
        }

        static LoggerHelper()
        {
            m_logWriter = new LogWriter();
            Application.RegisterLogCallback(new Application.LogCallback(ProcessExceptionReport));
        }

        public static void Release()
        {
            m_logWriter.Release();
        }

        public static void UploadLogFile()
        {
            m_logWriter.UploadTodayLog();
        }

        static ulong index = 0;

        /// <summary>
        /// 调试日志。
        /// </summary>
        /// <param name="message">日志内容</param>
        /// <param name="isShowStack">是否显示调用栈信息</param>
        public static void Debug(object message, Boolean isShowStack = SHOW_STACK, int user = 0)
        {
            //if (user != 11)
            //    return;
            if (DebugFilterStr != "") return;

            if (LogLevel.DEBUG == (CurrentLogLevels & LogLevel.DEBUG))
                Log(string.Concat(" [DEBUG]: ", isShowStack ? GetStackInfo() : "", message, " Index = ", index++), LogLevel.DEBUG);
        }

        /// <summary>
        /// 扩展debug
        /// </summary>
        /// <param name="message"></param>
        /// <param name="filter">只输出与在DebugMsg->filter里面设置的filter一样的debug</param>
        public static void Debug(string filter, object message, Boolean isShowStack = SHOW_STACK)
        {
            if (DebugFilterStr != "" && DebugFilterStr != filter) return;
            if (LogLevel.DEBUG == (CurrentLogLevels & LogLevel.DEBUG))
            {
                Log(string.Concat(" [DEBUG]: ", isShowStack ? GetStackInfo() : "", message), LogLevel.DEBUG);
            }
        }

        /// <summary>
        /// 信息日志。
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Info(object message, Boolean isShowStack = SHOW_STACK)
        {
            if (LogLevel.INFO == (CurrentLogLevels & LogLevel.INFO))
                Log(string.Concat(" [INFO]: ", isShowStack ? GetStackInfo() : "", message), LogLevel.INFO);
        }

        /// <summary>
        /// 警告日志。
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Warning(object message, Boolean isShowStack = SHOW_STACK)
        {
            if (LogLevel.WARNING == (CurrentLogLevels & LogLevel.WARNING))
                Log(string.Concat(" [WARNING]: ", isShowStack ? GetStackInfo() : "", message), LogLevel.WARNING);
        }

        /// <summary>
        /// 异常日志。
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Error(object message, Boolean isShowStack = SHOW_STACK)
        {
            if (LogLevel.ERROR == (CurrentLogLevels & LogLevel.ERROR))
                Log(string.Concat(" [ERROR]: ", message, '\n', isShowStack ? GetStacksInfo() : ""), LogLevel.ERROR);
        }

        /// <summary>
        /// 关键日志。
        /// </summary>
        /// <param name="message">日志内容</param>
        public static void Critical(object message, Boolean isShowStack = SHOW_STACK)
        {
            if (LogLevel.CRITICAL == (CurrentLogLevels & LogLevel.CRITICAL))
                Log(string.Concat(" [CRITICAL]: ", message, '\n', isShowStack ? GetStacksInfo() : ""), LogLevel.CRITICAL);
        }

        /// <summary>
        /// 异常日志。
        /// </summary>
        /// <param name="ex">异常实例。</param>
        public static void Except(Exception ex, object message = null)
        {
            if (LogLevel.EXCEPT == (CurrentLogLevels & LogLevel.EXCEPT))
            {
                Exception innerException = ex;
                while (innerException.InnerException != null)
                {
                    innerException = innerException.InnerException;
                }
                Log(string.Concat(" [EXCEPT]: ", message == null ? "" : message + "\n", ex.Message, innerException.StackTrace), LogLevel.CRITICAL);
            }
        }

        /// <summary>
        /// 获取堆栈信息。
        /// </summary>
        /// <returns></returns>
        private static String GetStacksInfo()
        {
            StringBuilder sb = new StringBuilder();
#if !UNITY_IPHONE
            StackTrace st = new StackTrace();
            var sf = st.GetFrames();
            for (int i = 2; i < sf.Length; i++)
            {
                sb.AppendLine(sf[i].ToString());
            }
#endif
            return sb.ToString();
        }

        /// <summary>
        /// 写日志。
        /// </summary>
        /// <param name="message">日志内容</param>
        private static void Log(string message, LogLevel level, bool writeEditorLog = true)
        {
            var msg = string.Concat(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"), message);
            m_logWriter.WriteLog(msg, level, writeEditorLog);
            //Debugger.Log(0, "TestRPC", message);
        }

        /// <summary>
        /// 获取调用栈信息。
        /// </summary>
        /// <returns>调用栈信息</returns>
        private static String GetStackInfo()
        {
#if UNITY_IPHONE
            return "";
#else
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(2);//[0]为本身的方法 [1]为调用方法
            var method = sf.GetMethod();
            return String.Format("{0}.{1}(): ", method.ReflectedType.Name, method.Name);
#endif
        }

        private static void ProcessExceptionReport(string message, string stackTrace, LogType type)
        {
            var logLevel = LogLevel.DEBUG;
            switch (type)
            {
                case LogType.Assert:
                    logLevel = LogLevel.DEBUG;
                    break;
                case LogType.Error:
                    logLevel = LogLevel.ERROR;
                    break;
                case LogType.Exception:
                    logLevel = LogLevel.EXCEPT;
                    break;
                case LogType.Log:
                    logLevel = LogLevel.DEBUG;
                    break;
                case LogType.Warning:
                    logLevel = LogLevel.WARNING;
                    break;
                default:
                    break;
            }
            if (logLevel == (CurrentLogLevels & logLevel))
                Log(string.Concat(" [SYS_", logLevel, "]: ", message, '\n', stackTrace), logLevel, false);
        }

        private static string getCurrentTime()
        {
            System.DateTime now = System.DateTime.Now;
            return now.ToString("yyyy-MM-dd HH:mm:ss fff", DateTimeFormatInfo.InvariantInfo);
        }

        public static ulong GetTimestamp()
        {
            System.DateTime now = System.DateTime.Now;
            System.DateTime start = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (ulong)(now - start).TotalSeconds;
        }

        /// <summary>
        /// 参数：bug,force,time,serverId
        /// 密钥：md5(serverId + bug + force + time +密钥KEY);
        /// </summary>
        /// <param name="bug"></param>
        /// <returns></returns>
        private static IEnumerator uploadBug(string bug)
        {
            yield return new WaitForEndOfFrame();
            WWWForm form = new WWWForm();
            string serverId = PlayerLogInfo.serverId.ToString();
            string force = IsMustUpload ? "1" : "0";
            string time = GetTimestamp().ToString();
            //string key = Utils.GetResNumber().PackArray();
            string key = SystemConfig.GetCfgInfoUrl("IsUseOldKey") == "1" ?
                SystemConfig.GetCfgInfoUrl(SystemConfig.PHOTO_HEAD_KEY) : Utils.GetResNumber().PackArray();
            string md5 = Utils.CreateMD5(Utils.StringConcat(serverId, force, time, key));
            form.AddField("serverId", serverId);
            form.AddField("bug", bug);
            form.AddField("force", force);
            form.AddField("time", time);
            form.AddField("verify", md5);
            WWW w = new WWW(UploadLogUrl, form);
            yield return w;
            if (w.error != null)
            {
                UnityEngine.Debug.LogError("upload log error:" + w.error);
            }
            else
            {
                Info("upload log:" + w.text);
            }
        }

        public static void BugAlert(string msg)
        {
#if UNITY_ANDROID
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject m_mainActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
            if (m_mainActivity != null)
            {
                m_mainActivity.CallStatic("unityBugAlert", msg);
            }
#endif
        }
    }

    /// <summary>
    /// 日志写入文件管理类。
    /// </summary>
    public class LogWriter
    {
        private string m_logPath = UnityEngine.Application.persistentDataPath + SystemConfig.MiddlePath + "/log/";
        private string m_logFileName = "log_{0}.txt";
        private string m_logFilePath;
        private FileStream m_fs;
        private StreamWriter m_sw;
        //private Action<String, LogLevel, bool> m_logWriter;
        private readonly static object m_locker = new object();
        private bool initFileSuccess = false;
        private Thread m_thread;
        public bool DoWriteLog = true;
        public int LogQueueCount = 50;
        public Queue<LogData> LogQueue = new Queue<LogData>();
        public LogData CurrentLogData;

        public string Content = "";

        public string LogPath
        {
            get { return m_logPath; }
        }

        /// <summary>
        /// 默认构造函数。
        /// </summary>
        public LogWriter()
        {
#if !UNITY_WEBPLAYER
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                m_logPath = SystemConfig.PCPath + "/log/";
            }
            if (!Directory.Exists(m_logPath))
                Directory.CreateDirectory(m_logPath);
            m_logFilePath = String.Concat(m_logPath, String.Format(m_logFileName, DateTime.Today.ToString("yyyyMMdd")));
            try
            {
                //m_logWriter = Write;
                m_fs = new FileStream(m_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                m_sw = new StreamWriter(m_fs);
                initFileSuccess = true;
#endif
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    TimerHeap.AddTimer(10, 20, DoWrite);
                }
                else
                {
                    if (m_thread == null)
                    {
                        m_thread = new Thread(WriteLogHandler);
                        m_thread.Start();
                    }
                }
#if !UNITY_WEBPLAYER
            }
            catch (Exception ex)
            {
                initFileSuccess = false;
                UnityEngine.Debug.LogError("init file error:" + ex.Message);
            }
#endif
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Release()
        {
#if !UNITY_WEBPLAYER
            lock (m_locker)
            {
                if (m_sw != null)
                {
                    m_sw.Close();
                    m_sw.Dispose();
                }
                if (m_fs != null)
                {
                    m_fs.Close();
                    m_fs.Dispose();
                }
                DoWriteLog = false;
                if (m_thread != null)
                    m_thread.Abort();
                m_thread = null;
            }
#endif
        }

        public void UploadTodayLog()
        {
            //lock (m_locker)
            //{
            //    using (var fs = new FileStream(m_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            //    {
            //        using (StreamReader sr = new StreamReader(fs))
            //        {
            //            var content = sr.ReadToEnd();
            //            var fn = Utils.GetFileName(m_logFilePath);//.Replace('/', '\\')
            //            if (MogoWorld.theAccount != null)
            //            {
            //                fn = string.Concat(MogoWorld.theAccount.name, "_", fn);
            //            }
            //            DownloadMgr.Instance.UploadLogFile(fn, content);
            //        }
            //    }
            //}
        }

        /// <summary>
        /// 写日志。
        /// </summary>
        /// <param name="msg">日志内容</param>
        public void WriteLog(string msg, LogLevel level, bool writeEditorLog)
        {
            lock (m_locker)
            {
                if (LogQueue.Count < LogQueueCount)
                    LogQueue.Enqueue(new LogData { Level = level, Msg = msg, WriteEditorLog = writeEditorLog });
            }
            //if (Application.platform == RuntimePlatform.IPhonePlayer)
            //    m_logWriter(msg, level, false);
            //else
            //    m_logWriter.BeginInvoke(msg, level, writeEditorLog, null, null);
#if UNITY_WEBPLAYER
            Content += msg + "\n";
#else
            if (level == LogLevel.ERROR || level == LogLevel.EXCEPT || level == LogLevel.CRITICAL)
            {
                if (level == LogLevel.ERROR) LoggerHelper.ErrorCount++;
                else LoggerHelper.ExceptCount++;
            }
#endif
        }

        private void WriteLogHandler()
        {
            while (DoWriteLog)
            {
                DoWrite();
                Thread.Sleep(10);
            }
        }

        private void DoWrite()
        {
            //struct必须构造了，Write方法才可以访问，Write逻辑不希望放在lock里占用着lock的资源
            lock (m_locker)
            {
                if (LogQueue.Count > 0)
                {
                    CurrentLogData = LogQueue.Dequeue();
                }
            }
            if (CurrentLogData.Level != LogLevel.NONE)
            {
                Write(CurrentLogData.Msg, CurrentLogData.Level, CurrentLogData.WriteEditorLog);
                CurrentLogData.Level = LogLevel.NONE;
            }
        }

        private void Write(string msg, LogLevel level, bool writeEditorLog)
        {
            if (writeEditorLog)
            {
                switch (level)
                {
                    case LogLevel.DEBUG:
                    case LogLevel.INFO:
                        UnityEngine.Debug.Log(msg);
                        break;
                    case LogLevel.WARNING:
                        UnityEngine.Debug.LogWarning(msg);
                        break;
                    case LogLevel.ERROR:
                    case LogLevel.EXCEPT:
                    case LogLevel.CRITICAL:
                        UnityEngine.Debug.LogError(msg);
                        break;
                    default:
                        break;
                }
            }
#if !UNITY_WEBPLAYER
            if (m_sw != null && initFileSuccess)
            {
                try
                {
                    m_sw.WriteLine(msg);
                    m_sw.Flush();
                }
                catch (Exception ex)
                {
                    initFileSuccess = false;
                    UnityEngine.Debug.LogError(ex.Message);
                }
            }
#endif
        }
    }

    public struct LogData
    {
        public string Msg;
        public LogLevel Level;
        public bool WriteEditorLog;
    }
}

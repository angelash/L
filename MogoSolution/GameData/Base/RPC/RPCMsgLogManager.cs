using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mogo.Util;
using UnityEngine;

namespace Mogo.RPC
{
    public class RPCMsgLogManager
    {
        private static readonly Queue<EtyRPCMsgLog> m_msgQueue = new Queue<EtyRPCMsgLog>();
        private static readonly object m_queueLock = new object();
        public static bool IsRecord;
        private static readonly MsgLogManager m_msgLogManager = new MsgLogManager("/log2/");
        private static readonly String SEND = "SEND";
        private static readonly String RECEIVE = "RECV";

        public static void Send(MSGIDType id, params object[] msgs)
        {
            lock (m_queueLock)
                m_msgQueue.Enqueue(new EtyRPCMsgLog {ID = id, IsSend = true, Now = DateTime.Now, Msgs = msgs});
        }

        public static void Receive(MSGIDType id, params object[] msgs)
        {
            lock (m_queueLock)
                m_msgQueue.Enqueue(new EtyRPCMsgLog {ID = id, IsSend = false, Now = DateTime.Now, Msgs = msgs});
        }

        public static void DoRecord()
        {
            var isEmpty = false;
            lock (m_queueLock)
                isEmpty = m_msgQueue.Count == 0;
            var sb = new StringBuilder();
            while (!isEmpty)
            {
                EtyRPCMsgLog ety;
                lock (m_queueLock)
                {
                    ety = m_msgQueue.Dequeue();
                    isEmpty = m_msgQueue.Count == 0;
                }
                sb.Append(ety.Now.ToString("HHmmssfff"));
                sb.AppendFormat(" {0} {1} ", ety.IsSend ? SEND : RECEIVE, ety.ID);
                sb.Append(ety.Msgs.PackArray('|'));
                sb.AppendLine();
                //LoggerHelper.Error(sb.ToString());
            }
            m_msgLogManager.Log(sb.ToString());
        }

        public static void Release()
        {
            m_msgLogManager.Release();
        }
    }

    public class MsgLogManager
    {
        private readonly FileStream m_fs;
        private readonly string m_logFileName = "log_{0}.txt";
        private readonly string m_logFilePath;
        private readonly string m_logPath = Application.persistentDataPath + SystemConfig.MiddlePath;
        private readonly StreamWriter m_sw;

        public MsgLogManager(String path)
        {
#if !UNITY_WEBPLAYER
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                m_logPath = SystemConfig.PCPath;
            }
            m_logPath = String.Concat(m_logPath, path);
            if (!Directory.Exists(m_logPath))
                Directory.CreateDirectory(m_logPath);
            m_logFilePath = String.Concat(m_logPath, String.Format(m_logFileName, DateTime.Today.ToString("yyyyMMdd")));
            try
            {
                m_fs = new FileStream(m_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                m_sw = new StreamWriter(m_fs);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
#endif
        }

        public void Log(string msg)
        {
#if !UNITY_WEBPLAYER
            if (m_sw != null)
            {
                m_sw.Write(msg);
                m_sw.Flush();
            }
#endif
        }

        public void Release()
        {
#if !UNITY_WEBPLAYER
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
#endif
        }
    }

    public struct EtyRPCMsgLog
    {
        public MSGIDType ID;
        public bool IsSend;
        public object[] Msgs;
        public DateTime Now;
    }
}
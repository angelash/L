#region 模块信息
/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：FileAccessManager
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2015.12.7
// 模块描述：文件访问管理器。
//----------------------------------------------------------------*/
#endregion

using System;
using System.IO;

namespace Mogo.Util
{
    /// <summary>
    /// 文件访问管理器。
    /// </summary>
    public class FileAccessManagerEx
    {

        public static void StartSaveFile()
        {
#if !UNITY_WEBPLAYER
            if (SystemSwitch.UseFileSystem)
            {
                MogoFileSystem.Instance.Open();
                MogoFileSystem.Instance.GetAndBackUpIndexInfo();
            }
#endif
        }

        public static void EndSaveFile()
        {
#if !UNITY_WEBPLAYER
            if (SystemSwitch.UseFileSystem)
            {
                MogoFileSystem.Instance.CleanBackUpIndex();
                MogoFileSystem.Instance.Close();
            }
#endif
        }

        public static void SaveBytes(String fileName, byte[] content)
        {
#if !UNITY_WEBPLAYER
            fileName = fileName.Replace('\\', '/');
            if (SystemSwitch.UseFileSystem)
            {
                MemoryStream stream = new MemoryStream(content);
                stream.Seek(0, SeekOrigin.Begin);

                var info = MogoFileSystem.Instance.BeginSaveFile(fileName, content.Length);
                int bytesRead;
                byte[] data = new byte[2048];
                while ((bytesRead = stream.Read(data, 0, data.Length)) > 0)
                {
                    MogoFileSystem.Instance.WriteFile(info, data, 0, bytesRead);
                }
                MogoFileSystem.Instance.EndSaveFile(info);
                MogoFileSystem.Instance.SaveIndexInfo();
            }
            else
            {
                XMLParser.SaveBytes(Path.Combine(SystemConfig.ResourceFolder, fileName), content);
            }
#endif
        }
    }
}
#region 模块信息
/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：MogoFileSystem
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.6.15
// 模块描述：自定义文件系统。
//----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Mogo.Util
{
    /// <summary>
    /// 自定义文件系统。
    /// </summary>
    public abstract class MogoFileSystem
    {
        #region 属性

        /// <summary>
        /// 页大小。
        /// </summary>
        protected const int m_pageSize = 512;
        /// <summary>
        /// 文件名称。
        /// </summary>
        public static string FILE_NAME
        {
            get
            {
                return "pkg";
            }
        }
        protected string BACK_UP
        {
            get
            {
                return "pkg_i_bak";
            }
        }
        /// <summary>
        /// 文件完整路径。
        /// </summary>
        protected string m_fileFullName;
        /// <summary>
        /// 文件索引。
        /// </summary>
        protected Dictionary<string, IndexInfo> m_fileIndexes = new Dictionary<string, IndexInfo>();
        /// <summary>
        /// 删除文件索引。
        /// </summary>
        protected List<IndexInfo> m_deletedIndexes = new List<IndexInfo>();
        /// <summary>
        /// 包信息。
        /// </summary>
        protected PackageInfo m_packageInfo = new PackageInfo();
        /// <summary>
        /// 索引加密钥匙
        /// C#的DES只支持64bits的Key
        /// http://msdn.microsoft.com/en-us/library/system.security.cryptography.des.key(VS.80).aspx
        /// </summary>
        protected byte[] m_number
        {
            get
            {
                return Utils.GetIndexNumber();//这里千万不能明文内容，安卓看到就完蛋了
            }
        }
        protected BitCryto m_bitCryto;
        protected bool m_encodeData = true;
        protected static readonly Object m_locker = new Object();
        protected bool m_isClosed = true;

        public bool IsClosed
        {
            get
            {
                return m_isClosed;
            }
        }

        /// <summary>
        /// 文件完整路径。
        /// </summary>
        public string FileFullName
        {
            get { return m_fileFullName; }
            set { m_fileFullName = value; }
        }

        public Dictionary<string, IndexInfo> FileIndexs
        {
            get { return m_fileIndexes; }
        }

        public List<IndexInfo> DeletedIndexes
        {
            get { return m_deletedIndexes; }
        }

        private static MogoFileSystem m_instance;

        public static MogoFileSystem Instance
        {
            get { return m_instance; }
        }

        #endregion

        #region 构造函数

        static MogoFileSystem()
        {
#if UNITY_WEBPLAYER
            m_instance = new MogoFileSystemForWeb();
#else
            m_instance = new MogoFileSystemForLocal();
#endif
        }

        protected MogoFileSystem()
        {
            var number = new List<short>();
            foreach (var item in m_number)
            {
                number.Add(item);
            }
            m_bitCryto = new BitCryto(number.ToArray());
        }

        #endregion

        #region 公有方法

        public abstract void Init();

        /// <summary>
        /// 打开文件读写。
        /// </summary>
        public abstract void Init(string fileName);

        /// <summary>
        /// 初始化内存读写。
        /// </summary>
        public abstract void Init(byte[] data);

        /// <summary>
        /// 打开文件读写。
        /// </summary>
        public abstract void Open();

        /// <summary>
        /// 保存索引信息。
        /// </summary>
        public abstract void SaveIndexInfo();

        /// <summary>
        /// 获取并备份索引信息。
        /// </summary>
        public abstract void GetAndBackUpIndexInfo();

        /// <summary>
        /// 关闭文件读写。
        /// </summary>
        public abstract void Close();

        /// <summary>
        /// 存储文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        public abstract void SaveFile(string fileFullName);

        public abstract List<String> GetFilesByDirectory(String path);

        public abstract bool IsFileExist(String fileFullName);

        /// <summary>
        /// 获取文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public abstract byte[] LoadFile(string fileFullName);

        public abstract List<KeyValuePair<string, byte[]>> LoadFiles(List<KeyValuePair<string, string>> fileFullNames);

        /// <summary>
        /// 获取文本类型文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public abstract String LoadTextFile(string fileFullName);

        /// <summary>
        /// 删除文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        public abstract void DeleteFile(string fileFullName);

        /// <summary>
        /// 开始存储文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public abstract IndexInfo BeginSaveFile(string fileFullName, long fileSize);

        /// <summary>
        /// 写入文件信息。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public abstract void WriteFile(IndexInfo info, Byte[] buffer, int offset, int count);

        /// <summary>
        /// 存储文件结束。
        /// </summary>
        /// <param name="info"></param>
        public abstract void EndSaveFile(IndexInfo info);

        public abstract void CleanBackUpIndex();

        public abstract void Clear();

        #endregion
    }
#if UNITY_WEBPLAYER

    /// <summary>
    /// 自定义网页文件系统。
    /// </summary>
    public class MogoFileSystemForWeb : MogoFileSystem
    {
        #region 属性

        /// <summary>
        /// 内存读写处理流。
        /// </summary>
        private MemoryStream m_memoryStream;

        #endregion

        #region 公有方法

        public override void Init()
        {
            lock (m_locker)
            {
                if (m_fileFullName == null)
                    m_fileFullName = SystemConfig.ResourceFolder + FILE_NAME;
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                Init(m_fileFullName);
                sw.Stop();
                Close();
            }
        }

        /// <summary>
        /// 打开文件读写。
        /// </summary>
        public override void Init(string fileName)
        {
            LoggerHelper.Error("Error Invoke");
        }

        /// <summary>
        /// 初始化内存读写。
        /// </summary>
        public override void Init(byte[] data)
        {
            if (m_memoryStream == null)
            {
                m_memoryStream = new MemoryStream(data);
                GetIndexInfo(data.Length);
            }

        }

        /// <summary>
        /// 打开文件读写。
        /// </summary>
        public override void Open()
        {
            // 此模式不用操作
        }

        /// <summary>
        /// 保存索引信息。
        /// </summary>
        public override void SaveIndexInfo()
        {
            lock (m_locker)
            {
                uint indexPosition = (uint)m_memoryStream.Position;
                var indexInfos = new List<Byte>();
                foreach (var item in m_fileIndexes)
                {
                    indexInfos.AddRange(item.Value.GetEncodeData());
                }
                foreach (var item in m_deletedIndexes)
                {
                    indexInfos.AddRange(item.GetEncodeData());
                }
                var indexInfo = indexInfos.ToArray();
                if (indexInfo.Length > 0)
                    indexInfo = DESCrypto.Encrypt(indexInfo, m_number);//加密索引数据

                m_memoryStream.Position = m_packageInfo.IndexOffset;//将文件偏移重置为索引偏移起始位置
                m_memoryStream.Write(indexInfo, 0, indexInfo.Length);
                m_memoryStream.Flush();
                SavePackageInfo(m_memoryStream);
                m_memoryStream.Position = indexPosition;
            }
        }

        /// <summary>
        /// 获取并备份索引信息。
        /// </summary>
        public override void GetAndBackUpIndexInfo()
        {
            GetIndexInfo(m_memoryStream.Length, true);
        }

        /// <summary>
        /// 关闭文件读写。
        /// </summary>
        public override void Close()
        {
            if (m_memoryStream != null)
            {
                m_memoryStream.Close();
                m_memoryStream.Dispose();
                m_memoryStream = null;
                m_isClosed = true;
            }
        }

        /// <summary>
        /// 存储文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        public override void SaveFile(string fileFullName)
        {
            // 此模式不用操作
        }

        public override List<String> GetFilesByDirectory(String path)
        {
            var result = new List<String>();
            path = path.PathNormalize();
            foreach (var item in m_fileIndexes)
            {
                if (item.Value.Path.PathNormalize() == path)
                    result.Add(item.Value.Path);
            }
            return result;
        }

        public override bool IsFileExist(String fileFullName)
        {
            fileFullName = fileFullName.PathNormalize();
            return m_fileIndexes.ContainsKey(fileFullName);
        }

        /// <summary>
        /// 获取文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public override byte[] LoadFile(string fileFullName)
        {
            Open();
            Byte[] result;
            lock (m_locker)
            {
                fileFullName = fileFullName.PathNormalize();
                if (m_memoryStream != null && m_fileIndexes.ContainsKey(fileFullName))
                {
                    result = DoLoadFile(fileFullName);
                    //result = Utils.DecompressBytes(result);
                }
                else
                    result = null;
            }
            return result;
        }

        public override List<KeyValuePair<string, byte[]>> LoadFiles(List<KeyValuePair<string, string>> fileFullNames)
        {
            Open();
            List<KeyValuePair<string, byte[]>> result = new List<KeyValuePair<string, byte[]>>();
            lock (m_locker)
            {
                foreach (var fileFullName in fileFullNames)
                {
                    var path = fileFullName.Value.PathNormalize();
                    if (m_memoryStream != null && m_fileIndexes.ContainsKey(path))
                    {
                        var fileData = DoLoadFile(path);
                        result.Add(new KeyValuePair<string, byte[]>(fileFullName.Key, fileData));
                    }
                    else
                    {
                        DriverLib.Invoke(() => { LoggerHelper.Error("File not exist in MogoFileSystem: " + FileFullName); });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取文本类型文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public override String LoadTextFile(string fileFullName)
        {
            Open();
            var content = LoadFile(fileFullName);
            if (content != null)
                return System.Text.Encoding.UTF8.GetString(content);
            else
                return String.Empty;
        }

        /// <summary>
        /// 删除文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        public override void DeleteFile(string fileFullName)
        {
            lock (m_locker)
            {
                fileFullName = fileFullName.PathNormalize();
                if (m_fileIndexes.ContainsKey(fileFullName))
                {
                    var info = m_fileIndexes[fileFullName];
                    info.Deleted = true;
                    info.Id = "";
                    m_fileIndexes.Remove(fileFullName);
                    m_deletedIndexes.Add(info);
                }
            }
        }

        /// <summary>
        /// 开始存储文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public override IndexInfo BeginSaveFile(string fileFullName, long fileSize)
        {
            IndexInfo info;
            lock (m_locker)
            {
                fileFullName = fileFullName.PathNormalize();
                if (m_fileIndexes.ContainsKey(fileFullName))//文件已存在
                {
                    var fileInfo = m_fileIndexes[fileFullName];
                    if (fileSize > fileInfo.PageLength)//新文件大小超出预留页大小
                    {
                        DeleteFile(fileFullName);

                        info = new IndexInfo() { Id = fileFullName, FileName = fileInfo.FileName, Path = fileInfo.Path, Offset = m_packageInfo.IndexOffset, FileState = FileState.New };
                        m_memoryStream.Position = m_packageInfo.IndexOffset;
                        m_fileIndexes[fileFullName] = info;
                    }
                    else//新文件大小没有超出预留页大小
                    {
                        info = fileInfo;
                        info.Length = 0;
                        info.FileState = FileState.Modify;
                        m_memoryStream.Position = info.Offset;
                    }
                }
                else//文件不存在，在索引最后新建
                {
                    info = new IndexInfo()
                    {
                        Id = fileFullName,
                        FileName = Path.GetFileName(fileFullName),
                        Path = Path.GetDirectoryName(fileFullName),
                        Offset = m_packageInfo.IndexOffset,
                        FileState = FileState.New
                    };
                    m_memoryStream.Position = m_packageInfo.IndexOffset;
                    m_fileIndexes[fileFullName] = info;
                }
            }
            return info;
        }

        /// <summary>
        /// 写入文件信息。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void WriteFile(IndexInfo info, Byte[] buffer, int offset, int count)
        {
            info.Length += (uint)count;
            if (m_encodeData)
            {
                m_bitCryto.Reset();
                int len = buffer.Length;
                for (int i = 0; i < len; ++i)
                {
                    buffer[i] = m_bitCryto.Encode(buffer[i]);
                }
            }
            m_memoryStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// 存储文件结束。
        /// </summary>
        /// <param name="info"></param>
        public override void EndSaveFile(IndexInfo info)
        {
            lock (m_locker)
            {
                var leftSize = info.Length % m_pageSize;
                if (leftSize != 0)
                {
                    var empty = m_pageSize - leftSize;
                    var emptyData = new Byte[empty];
                    m_memoryStream.Write(emptyData, 0, emptyData.Length);
                    info.PageLength = info.Length + empty;
                }
                else
                    info.PageLength = info.Length;

                m_memoryStream.Flush();
                if (info.FileState == FileState.New)//修改状态不用修改索引偏移
                    m_packageInfo.IndexOffset = (uint)m_memoryStream.Position;
            }
        }

        public override void CleanBackUpIndex()
        {
            // 此模式不用操作
        }

        public override void Clear()
        {
            m_fileIndexes.Clear();
            m_deletedIndexes.Clear();
            m_packageInfo.IndexOffset = 0;
            Close();
            UnityEngine.Caching.CleanCache();
        }

        #endregion

        #region 私有方法

        private byte[] DoLoadFile(string fileFullName)
        {
            var info = m_fileIndexes[fileFullName];
            Byte[] result = new Byte[info.Length];
            m_memoryStream.Position = info.Offset;
            m_memoryStream.Read(result, 0, result.Length);
            if (m_encodeData)
            {
                m_bitCryto.Reset();
                int len = result.Length;
                for (int i = 0; i < len; ++i)
                {
                    result[i] = m_bitCryto.Decode(result[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取索引信息。
        /// </summary>
        /// <param name="fileSize">文件包大小。</param>
        private void GetIndexInfo(long fileSize)
        {
            var useBackUp = CheckBackUpIndex();
            if (!useBackUp)
                GetIndexInfo(fileSize, false);
        }

        /// <summary>
        /// 获取索引信息。
        /// </summary>
        /// <param name="fileSize">文件包大小。</param>
        /// <param name="needBackUpIndex">是否备份索引信息。</param>
        private void GetIndexInfo(long fileSize, bool needBackUpIndex)
        {
            lock (m_locker)
            {
                m_packageInfo = GetPackageInfo(m_memoryStream, fileSize);
                if (m_packageInfo == null || m_packageInfo.IndexOffset == 0)
                    return;
                var indexSize = (int)(fileSize - PackageInfo.GetPackageSize() - m_packageInfo.IndexOffset);//计算索引信息大小
                var indexData = new Byte[indexSize];
                m_memoryStream.Position = m_packageInfo.IndexOffset;
                m_memoryStream.Read(indexData, 0, indexSize);//获取索引信息
                LoadIndexInfo(indexData, needBackUpIndex);
            }
        }

        private bool CheckBackUpIndex()
        {
            // 此模式不用操作
            return false;
        }

        private void LoadIndexInfo(Byte[] indexData, bool needBackUpIndex)
        {
            if (indexData.Length > 0)
            {
                if (needBackUpIndex)
                    BackupIndexInfo(indexData);
                indexData = DESCrypto.Decrypt(indexData, m_number);//解密索引数据
            }

            m_deletedIndexes.Clear();
            m_fileIndexes.Clear();
            var index = 0;
            while (index < indexData.Length)
            {
                var info = IndexInfo.Decode(indexData, ref index);
                if (info.Deleted)
                    m_deletedIndexes.Add(info);
                else
                    m_fileIndexes[info.Id.PathNormalize()] = info;
            }

            m_memoryStream.Position = m_packageInfo.IndexOffset;//重置回文件信息结尾处
        }

        private void BackupIndexInfo(byte[] indexData)
        {
            // 此模式不用操作
        }

        /// <summary>
        /// 获取包信息。
        /// </summary>
        /// <param name="fileSize"></param>
        private PackageInfo GetPackageInfo(MemoryStream fileStream, long fileSize)
        {
            var packageInfo = new PackageInfo();
            if (fileSize < PackageInfo.GetPackageSize())
                return new PackageInfo();

            fileStream.Position = fileSize - PackageInfo.GetPackageSize();//索引信息起始位置存放在文件结尾处。
            var lengthData = new Byte[PackageInfo.GetPackageSize()];
            fileStream.Read(lengthData, 0, PackageInfo.GetPackageSize());
            var index = 0;
            packageInfo.IndexOffset = EncodeDecoder.DecodeUInt32(lengthData, ref index);//获取索引信息起始位置
            return packageInfo;
        }

        private void SavePackageInfo(MemoryStream fileStream)
        {
            var indexPositionData = EncodeDecoder.EncodeUInt32(m_packageInfo.IndexOffset);
            fileStream.Write(indexPositionData, 0, indexPositionData.Length);
            fileStream.Flush();
        }

        #endregion
    }
    #else
    /// <summary>
    /// 自定义本地文件系统。
    /// </summary>
    public class MogoFileSystemForLocal : MogoFileSystem
    {
        #region 属性

        /// <summary>
        /// 文件读写处理流。
        /// </summary>
        private FileStream m_fileStream;

        #endregion

        #region 公有方法

        public override void Init()
        {
            lock (m_locker)
            {
                if (m_fileFullName == null)
                    m_fileFullName = SystemConfig.ResourceFolder + FILE_NAME;
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                Init(m_fileFullName);
                sw.Stop();
                Close();
            }
        }

        /// <summary>
        /// 打开文件读写。
        /// </summary>
        public override void Init(string fileName)
        {
            if (m_fileStream != null)
                return;
            if (!File.Exists(fileName))
            {
                var dir = Path.GetDirectoryName(fileName);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                m_fileStream = File.Create(fileName);
            }
            else
            {
                var fileInfo = new FileInfo(fileName);
                var fileSize = fileInfo.Length;

                m_fileStream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                GetIndexInfo(fileSize);
            }
            m_isClosed = false;
        }

        /// <summary>
        /// 初始化内存读写。
        /// </summary>
        public override void Init(byte[] data)
        {
            LoggerHelper.Error("Error Invoke");
        }

        /// <summary>
        /// 打开文件读写。
        /// </summary>
        public override void Open()
        {
            if (m_fileStream == null)
                m_fileStream = File.Open(m_fileFullName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        /// <summary>
        /// 保存索引信息。
        /// </summary>
        public override void SaveIndexInfo()
        {
            lock (m_locker)
            {
                uint indexPosition = (uint)m_fileStream.Position;
                var indexInfos = new List<Byte>();
                foreach (var item in m_fileIndexes)
                {
                    indexInfos.AddRange(item.Value.GetEncodeData());
                }
                foreach (var item in m_deletedIndexes)
                {
                    indexInfos.AddRange(item.GetEncodeData());
                }
                var indexInfo = indexInfos.ToArray();
                if (indexInfo.Length > 0)
                    indexInfo = DESCrypto.Encrypt(indexInfo, m_number);//加密索引数据

                m_fileStream.Position = m_packageInfo.IndexOffset;//将文件偏移重置为索引偏移起始位置
                m_fileStream.Write(indexInfo, 0, indexInfo.Length);
                m_fileStream.Flush();
                SavePackageInfo(m_fileStream);
                m_fileStream.Position = indexPosition;
            }
        }

        /// <summary>
        /// 获取并备份索引信息。
        /// </summary>
        public override void GetAndBackUpIndexInfo()
        {
            var fileInfo = new FileInfo(m_fileFullName);
            var fileSize = fileInfo.Length;

            GetIndexInfo(fileSize, true);
        }

        /// <summary>
        /// 关闭文件读写。
        /// </summary>
        public override void Close()
        {
            if (m_fileStream != null)
            {
                m_fileStream.Close();
                m_fileStream.Dispose();
                m_fileStream = null;
                m_isClosed = true;
            }
        }

        /// <summary>
        /// 存储文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        public override void SaveFile(string fileFullName)
        {
            if (!File.Exists(fileFullName))
                return;
            var fileInfo = new FileInfo(fileFullName);
            var info = BeginSaveFile(fileFullName, fileInfo.Length);

            byte[] data = new byte[2048];
            using (FileStream input = File.OpenRead(fileFullName))
            {
                int bytesRead;
                while ((bytesRead = input.Read(data, 0, data.Length)) > 0)
                {
                    WriteFile(info, data, 0, bytesRead);
                }
            }

            EndSaveFile(info);
        }

        public override List<String> GetFilesByDirectory(String path)
        {
            var result = new List<String>();
            path = path.PathNormalize();
            foreach (var item in m_fileIndexes)
            {
                if (item.Value.Path.PathNormalize() == path)
                    result.Add(item.Value.Path);
            }
            return result;
        }

        public override bool IsFileExist(String fileFullName)
        {
            fileFullName = fileFullName.PathNormalize();
            return m_fileIndexes.ContainsKey(fileFullName);
        }

        /// <summary>
        /// 获取文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public override byte[] LoadFile(string fileFullName)
        {
            Open();
            Byte[] result;
            lock (m_locker)
            {
                fileFullName = fileFullName.PathNormalize();
                if (m_fileStream != null && m_fileIndexes.ContainsKey(fileFullName))
                {
                    result = DoLoadFile(fileFullName);
                    //result = Utils.DecompressBytes(result);
                }
                else
                    result = null;
            }
            return result;
        }

        public override List<KeyValuePair<string, byte[]>> LoadFiles(List<KeyValuePair<string, string>> fileFullNames)
        {
            Open();
            List<KeyValuePair<string, byte[]>> result = new List<KeyValuePair<string, byte[]>>();
            lock (m_locker)
            {
                foreach (var fileFullName in fileFullNames)
                {
                    var path = fileFullName.Value.PathNormalize();
                    if (m_fileStream != null && m_fileIndexes.ContainsKey(path))
                    {
                        var fileData = DoLoadFile(path);
                        result.Add(new KeyValuePair<string, byte[]>(fileFullName.Key, fileData));
                    }
                    else
                    {
                        DriverLib.Invoke(() => { LoggerHelper.Error("File not exist in MogoFileSystem: " + FileFullName); });
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取文本类型文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <returns></returns>
        public override String LoadTextFile(string fileFullName)
        {
            Open();
            var content = LoadFile(fileFullName);
            if (content != null)
                return System.Text.Encoding.UTF8.GetString(content);
            else
                return String.Empty;
        }

        /// <summary>
        /// 删除文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        public override void DeleteFile(string fileFullName)
        {
            lock (m_locker)
            {
                fileFullName = fileFullName.PathNormalize();
                if (m_fileIndexes.ContainsKey(fileFullName))
                {
                    var info = m_fileIndexes[fileFullName];
                    info.Deleted = true;
                    info.Id = "";
                    m_fileIndexes.Remove(fileFullName);
                    m_deletedIndexes.Add(info);
                }
            }
        }

        /// <summary>
        /// 开始存储文件。
        /// </summary>
        /// <param name="fileFullName"></param>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public override IndexInfo BeginSaveFile(string fileFullName, long fileSize)
        {
            IndexInfo info;
            lock (m_locker)
            {
                fileFullName = fileFullName.PathNormalize();
                if (m_fileIndexes.ContainsKey(fileFullName))//文件已存在
                {
                    var fileInfo = m_fileIndexes[fileFullName];
                    if (fileSize > fileInfo.PageLength)//新文件大小超出预留页大小
                    {
                        DeleteFile(fileFullName);

                        info = new IndexInfo() { Id = fileFullName, FileName = fileInfo.FileName, Path = fileInfo.Path, Offset = m_packageInfo.IndexOffset, FileState = FileState.New };
                        m_fileStream.Position = m_packageInfo.IndexOffset;
                        m_fileIndexes[fileFullName] = info;
                    }
                    else//新文件大小没有超出预留页大小
                    {
                        info = fileInfo;
                        info.Length = 0;
                        info.FileState = FileState.Modify;
                        m_fileStream.Position = info.Offset;
                    }
                }
                else//文件不存在，在索引最后新建
                {
                    info = new IndexInfo()
                    {
                        Id = fileFullName,
                        FileName = Path.GetFileName(fileFullName),
                        Path = Path.GetDirectoryName(fileFullName),
                        Offset = m_packageInfo.IndexOffset,
                        FileState = FileState.New
                    };
                    m_fileStream.Position = m_packageInfo.IndexOffset;
                    m_fileIndexes[fileFullName] = info;
                }
            }
            return info;
        }

        /// <summary>
        /// 写入文件信息。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void WriteFile(IndexInfo info, Byte[] buffer, int offset, int count)
        {
            info.Length += (uint)count;
            if (m_encodeData)
            {
                m_bitCryto.Reset();
                int len = buffer.Length;
                for (int i = 0; i < len; ++i)
                {
                    buffer[i] = m_bitCryto.Encode(buffer[i]);
                }
            }
            m_fileStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// 存储文件结束。
        /// </summary>
        /// <param name="info"></param>
        public override void EndSaveFile(IndexInfo info)
        {
            lock (m_locker)
            {
                var leftSize = info.Length % m_pageSize;
                if (leftSize != 0)
                {
                    var empty = m_pageSize - leftSize;
                    var emptyData = new Byte[empty];
                    m_fileStream.Write(emptyData, 0, emptyData.Length);
                    info.PageLength = info.Length + empty;
                }
                else
                    info.PageLength = info.Length;

                m_fileStream.Flush();
                if (info.FileState == FileState.New)//修改状态不用修改索引偏移
                    m_packageInfo.IndexOffset = (uint)m_fileStream.Position;
            }
        }

        public override void CleanBackUpIndex()
        {
            var path = Path.GetDirectoryName(m_fileFullName);
            var fileName = Path.Combine(path, BACK_UP).Replace("\\", "/");
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        public override void Clear()
        {
            m_fileIndexes.Clear();
            m_deletedIndexes.Clear();
            m_packageInfo.IndexOffset = 0;
            Close();
            UnityEngine.Caching.CleanCache();
        }

        #endregion

        #region 私有方法

        private byte[] DoLoadFile(string fileFullName)
        {
            var info = m_fileIndexes[fileFullName];
            Byte[] result = new Byte[info.Length];
            m_fileStream.Position = info.Offset;
            m_fileStream.Read(result, 0, result.Length);
            if (m_encodeData)
            {
                m_bitCryto.Reset();
                int len = result.Length;
                for (int i = 0; i < len; ++i)
                {
                    result[i] = m_bitCryto.Decode(result[i]);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取索引信息。
        /// </summary>
        /// <param name="fileSize">文件包大小。</param>
        private void GetIndexInfo(long fileSize)
        {
            var useBackUp = CheckBackUpIndex();
            if (!useBackUp)
                GetIndexInfo(fileSize, false);
        }

        /// <summary>
        /// 获取索引信息。
        /// </summary>
        /// <param name="fileSize">文件包大小。</param>
        /// <param name="needBackUpIndex">是否备份索引信息。</param>
        private void GetIndexInfo(long fileSize, bool needBackUpIndex)
        {
            lock (m_locker)
            {
                m_packageInfo = GetPackageInfo(m_fileStream, fileSize);
                if (m_packageInfo == null || m_packageInfo.IndexOffset == 0)
                    return;
                var indexSize = (int)(fileSize - PackageInfo.GetPackageSize() - m_packageInfo.IndexOffset);//计算索引信息大小
                var indexData = new Byte[indexSize];
                m_fileStream.Position = m_packageInfo.IndexOffset;
                m_fileStream.Read(indexData, 0, indexSize);//获取索引信息
                LoadIndexInfo(indexData, needBackUpIndex);
            }
        }

        private bool CheckBackUpIndex()
        {
            var path = Path.GetDirectoryName(m_fileFullName);
            var fileName = Path.Combine(path, BACK_UP).Replace("\\", "/");
            if (File.Exists(fileName))
            {
                var fileInfo = new FileInfo(fileName);
                var fileSize = fileInfo.Length;
                var fileStream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                m_packageInfo = GetPackageInfo(fileStream, fileSize);

                if (m_packageInfo == null || m_packageInfo.IndexOffset == 0)
                    return true;
                var indexSize = (int)(fileSize - PackageInfo.GetPackageSize());//计算索引信息大小
                var indexData = new Byte[indexSize];
                fileStream.Position = 0;
                fileStream.Read(indexData, 0, indexSize);//获取索引信息
                LoadIndexInfo(indexData, false);
                return true;
            }
            else
                return false;
        }

        private void LoadIndexInfo(Byte[] indexData, bool needBackUpIndex)
        {
            if (indexData.Length > 0)
            {
                if (needBackUpIndex)
                    BackupIndexInfo(indexData);
                indexData = DESCrypto.Decrypt(indexData, m_number);//解密索引数据
            }

            m_deletedIndexes.Clear();
            m_fileIndexes.Clear();
            var index = 0;
            while (index < indexData.Length)
            {
                var info = IndexInfo.Decode(indexData, ref index);
                if (info.Deleted)
                    m_deletedIndexes.Add(info);
                else
                    m_fileIndexes[info.Id.PathNormalize()] = info;
            }

            m_fileStream.Position = m_packageInfo.IndexOffset;//重置回文件信息结尾处
        }

        private void BackupIndexInfo(byte[] indexData)
        {
            var path = Path.GetDirectoryName(m_fileFullName);
            var fileName = Path.Combine(path, BACK_UP).Replace("\\", "/");
            if (File.Exists(fileName))
                File.Delete(fileName);
            using (var fileStream = File.Create(fileName))
            {
                fileStream.Write(indexData, 0, indexData.Length);
                fileStream.Flush();
                SavePackageInfo(fileStream);
            }
        }

        /// <summary>
        /// 获取包信息。
        /// </summary>
        /// <param name="fileSize"></param>
        private PackageInfo GetPackageInfo(FileStream fileStream, long fileSize)
        {
            var packageInfo = new PackageInfo();
            if (fileSize < PackageInfo.GetPackageSize())
                return new PackageInfo();

            fileStream.Position = fileSize - PackageInfo.GetPackageSize();//索引信息起始位置存放在文件结尾处。
            var lengthData = new Byte[PackageInfo.GetPackageSize()];
            fileStream.Read(lengthData, 0, PackageInfo.GetPackageSize());
            var index = 0;
            packageInfo.IndexOffset = EncodeDecoder.DecodeUInt32(lengthData, ref index);//获取索引信息起始位置
            return packageInfo;
        }

        private void SavePackageInfo(FileStream fileStream)
        {
            var indexPositionData = EncodeDecoder.EncodeUInt32(m_packageInfo.IndexOffset);
            fileStream.Write(indexPositionData, 0, indexPositionData.Length);
            fileStream.Flush();
        }

        #endregion
    }
#endif

    /// <summary>
    /// 索引实体。
    /// </summary>
    public class IndexInfo
    {
        /// <summary>
        /// 索引标识。
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 文件名称。
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件路径。
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 文件存储索引偏移量。
        /// </summary>
        public uint Offset { get; set; }
        /// <summary>
        /// 文件大小。
        /// </summary>
        public uint Length { get; set; }
        /// <summary>
        /// 文件占用大小。
        /// </summary>
        public uint PageLength { get; set; }
        /// <summary>
        /// 是否已删除。
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// 文件是否修改。
        /// </summary>
        public FileState FileState { get; set; }

        public Byte[] GetEncodeData()
        {
            return Encode(this);
        }

        public static byte[] Encode(IndexInfo info)
        {
            var result = new List<byte>();
            result.AddRange(EncodeDecoder.EncodeString(info.Id));
            result.AddRange(EncodeDecoder.EncodeString(info.FileName));
            result.AddRange(EncodeDecoder.EncodeString(info.Path));
            result.AddRange(EncodeDecoder.EncodeUInt32(info.Offset));
            result.AddRange(EncodeDecoder.EncodeUInt32(info.Length));
            result.AddRange(EncodeDecoder.EncodeUInt32(info.PageLength));
            result.AddRange(EncodeDecoder.EncodeBoolean(info.Deleted));
            return result.ToArray();
        }

        public static IndexInfo Decode(byte[] data, ref int offset)
        {
            var info = new IndexInfo();
            info.Id = EncodeDecoder.DecodeString(data, ref offset);
            info.FileName = EncodeDecoder.DecodeString(data, ref offset);
            info.Path = EncodeDecoder.DecodeString(data, ref offset);
            info.Offset = EncodeDecoder.DecodeUInt32(data, ref offset);
            info.Length = EncodeDecoder.DecodeUInt32(data, ref offset);
            info.PageLength = EncodeDecoder.DecodeUInt32(data, ref offset);
            info.Deleted = EncodeDecoder.DecodeBoolean(data, ref offset);
            return info;
        }
    }

    /// <summary>
    /// 文件编辑状态。
    /// </summary>
    public enum FileState
    {
        /// <summary>
        /// 默认状态。
        /// </summary>
        Default,
        /// <summary>
        /// 新建状态。
        /// </summary>
        New,
        /// <summary>
        /// 修改状态。
        /// </summary>
        Modify
    }

    /// <summary>
    /// 包信息实体。
    /// </summary>
    public class PackageInfo
    {
        private static int m_packageSize = -1;

        /// <summary>
        /// 索引偏移量。
        /// </summary>
        public uint IndexOffset { get; set; }

        static PackageInfo()
        {
            m_packageSize = 4;// Marshal.SizeOf(typeof(UInt32));
        }

        public static int GetPackageSize()
        {
            return m_packageSize;
        }
    }

    public class EncodeDecoder
    {
        private static Encoding m_encoding = Encoding.UTF8;

        private static int m_uintLength = 4;//Marshal.SizeOf(typeof(UInt32));
        private static int m_uint16Length = 2;//Marshal.SizeOf(typeof(UInt16));
        private static int m_boolLength = 1;
        public static uint DecodeUInt32(byte[] data, ref Int32 index)
        {
            Byte[] result = new Byte[m_uintLength];
            Buffer.BlockCopy(data, index, result, 0, m_uintLength);
            //Array.Reverse(result);
            index += m_uintLength;

            return BitConverter.ToUInt32(result, 0);
        }
        public static byte[] EncodeUInt32(uint vValue)
        {
            var result = BitConverter.GetBytes(vValue);
            //Array.Reverse(result);
            return result;
        }

        public static byte[] EncodeBoolean(bool vValue)
        {
            var result = BitConverter.GetBytes(vValue);
            //Array.Reverse(result);
            return result;
        }

        public static bool DecodeBoolean(byte[] data, ref Int32 index)
        {
            Byte[] result = new Byte[m_boolLength];
            //Array.Reverse(result);
            Buffer.BlockCopy(data, index, result, 0, m_boolLength);
            index += m_boolLength;

            return BitConverter.ToBoolean(result, 0);
        }

        public static UInt16 DecodeUInt16(byte[] data, ref Int32 index)
        {
            Byte[] result = new Byte[m_uint16Length];
            Buffer.BlockCopy(data, index, result, 0, m_uint16Length);
            //Array.Reverse(result);
            index += m_uint16Length;

            return BitConverter.ToUInt16(result, 0);
        }

        public static byte[] EncodeString(String vValue)
        {
            String value = (String)vValue;
            //byte[] encodeValues = m_encoding.GetBytes(value);
            //Array.Reverse(encodeValues);
            Encoder ec = m_encoding.GetEncoder();//获取字符编码
            Char[] charArray = value.ToCharArray();
            Int32 length = ec.GetByteCount(charArray, 0, charArray.Length, false);//获取字符串转换为二进制数组后的长度，用于申请存放空间
            Byte[] encodeValues = new Byte[length];//申请存放空间
            ec.GetBytes(charArray, 0, charArray.Length, encodeValues, 0, true);//将字符串按照特定字符编码转换为二进制数组

            return Mogo.RPC.Utils.FillLengthHead(encodeValues);
        }

        public static String DecodeString(byte[] data, ref Int32 index)
        {
            Byte[] strData = CutLengthHead(data, ref index);
            return m_encoding.GetString(strData);
        }

        /// <summary>
        /// 去掉数据长度信息，返回对应数据的二进制数组，并进行相应的索引偏移。
        /// </summary>
        /// <param name="srcData">源数据</param>
        /// <param name="index">索引引用</param>
        /// <returns>对应数据的二进制数组</returns>
        protected static Byte[] CutLengthHead(Byte[] srcData, ref Int32 index)
        {
            Int32 length = (Int32)DecodeUInt16(srcData, ref index);
            Byte[] result = new Byte[length];
            Buffer.BlockCopy(srcData, index, result, 0, length);
            ////Array.Reverse(result);
            index += length;

            return result;
        }

    }
}

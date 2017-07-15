#region 模块信息
/*----------------------------------------------------------------
// Copyright (C) 2013 广州，爱游
//
// 模块名：VersionManager
// 创建者：Ash Tang
// 修改者列表：
// 创建日期：2013.5.27
// 模块描述：版本管理。
//----------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using UnityEngine;
using Mogo.Util;

/// <summary>
/// 版本管理。
/// </summary>
public class VersionManager
{
    //public VersionCodeInfo ServerVersion { get; private set; }
    //public VersionCodeInfo LocalVersion { get; private set; }
    public VersionManagerInfo ServerVersion { get; private set; }
    public VersionManagerInfo LocalVersion { get; private set; }

    public string defContent = "";
    public byte[] defContentBytes { get; private set; }

    public static VersionManager Instance { get; set; }

    //是否平台的更新下载,只更新apk
    private VersionManager() { }

    static VersionManager()
    {
        Instance = new VersionManager();
    }

    public void Init()
    {
        AddListeners();
    }

    public void AddListeners()
    {
        EventDispatcher.AddEventListener<string>(VersionEvent.AddMD5Content, AddMD5Content);
        EventDispatcher.AddEventListener(VersionEvent.GetContentMD5, GetContentMD5);
    }

    public void RemoveListeners()
    {
        EventDispatcher.RemoveEventListener<string>(VersionEvent.AddMD5Content, AddMD5Content);
        EventDispatcher.RemoveEventListener(VersionEvent.GetContentMD5, GetContentMD5);
    }

    /// <summary>
    /// 获取总的Content
    /// </summary>
    /// <param name="newContent">要计算的Content</param>
    private void AddMD5Content(string newContent)
    {
        defContent += newContent;
    }

    /// <summary>
    /// 获取MD5
    /// </summary>
    private void GetContentMD5()
    {
        defContentBytes = Mogo.Util.Utils.CreateMD5(Encoding.UTF8.GetBytes(defContent));
    }

    public void LoadLocalVersion()
    {
#if !UNITY_WEBPLAYER
        if (File.Exists(SystemConfig.VersionPath))
        {
            var ver = Utils.LoadFile(SystemConfig.VersionPath);
            LocalVersion = GetVersionInXML(ver);
            PlayerLogInfo.LocalSResVersion = LocalVersion.ResourceVersion;
            var programVer = Resources.Load(SystemConfig.VERSION_URL_KEY) as TextAsset;
            if (programVer && !String.IsNullOrEmpty(programVer.text))
                LocalVersion.ProgramVersionInfo = GetVersionInXML(programVer.text).ProgramVersionInfo;
            LoggerHelper.Info("program version : " + LocalVersion.ProgramVersion + " resource version :" + LocalVersion.ResourceVersion
                + " first resource version : " + LocalVersion.FirstResourceVersion);
        }
        else
        {
#endif
            LocalVersion = new VersionManagerInfo();
#if !UNITY_WEBPLAYER
            LoggerHelper.Info("cannot find local version");
            //var ver = Resources.Load(SystemConfig.VERSION_URL_KEY) as TextAsset;
            //if (ver != null)
            //    XMLParser.SaveText(SystemConfig.VersionPath, ver.text);
        }
#endif
    }

    public void ModalCheckNetwork(Action check)
    {
        DriverLib.Invoke(() =>
        {
            //无网络的情况不判断了
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                //3G网络，弹窗口，确定下载还是退出
                System.Collections.Generic.Dictionary<int, DefaultLanguageData> languageData = DefaultUI.dataMap;
                ForwardLoadingMsgBoxLib.Instance.ShowMsgBox(languageData[6].content, languageData[7].content, languageData[5].content, (confirm) =>
                {
                    if (confirm)
                    {
                        ForwardLoadingMsgBoxLib.Instance.Hide();
                        check();
                    }
                    else
                    {
                        ForwardLoadingMsgBoxLib.Instance.Hide();
                        Application.Quit();
                    }
                });
            }
            else
            {
                check();
            }
        });
    }
    public void BeforeCheck(Action<UpdateType> AsynResult, Action OnError)
    {
        ServerVersion = new VersionManagerInfo();

        var props = typeof(VersionManagerInfo).GetProperties();
        try
        {
            foreach (var prop in props)
            {
                if (prop != null)
                {
                    var target = SystemConfig.GetCfgInfoUrl(prop.Name);
                    if (!string.IsNullOrEmpty(target))
                    {
                        var value = Utils.GetValue(target, prop.PropertyType);
                        if (value != null)
                            prop.SetValue(ServerVersion, value, null);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LoggerHelper.Error("Get ServerVersion error: " + ex.Message);
        }
        PlayerLogInfo.ServerResVersion = LocalVersion.ResourceVersionInfo.ToString();
        Mogo.Util.LoggerHelper.Debug("服务器程序版本: " + ServerVersion.ProgramVersionInfo);
        Mogo.Util.LoggerHelper.Debug("服务器资源版本: " + ServerVersion.ResourceVersionInfo);
        Mogo.Util.LoggerHelper.Debug("服务器首包资源版本: " + ServerVersion.FirstResourceVersionInfo);
        Mogo.Util.LoggerHelper.Debug("服务器完整包资源版本: " + ServerVersion.FullResourceVersionInfo);
        Mogo.Util.LoggerHelper.Debug("服务器包地址: " + ServerVersion.PackageUrl);
        Mogo.Util.LoggerHelper.Debug("服务器Apk地址: " + ServerVersion.ApkUrl);
        Mogo.Util.LoggerHelper.Debug("服务器Small Apk地址: " + ServerVersion.FirstApkUrl);
        Mogo.Util.LoggerHelper.Debug("服务器md5地址: " + ServerVersion.PackageMd5List);
        Mogo.Util.LoggerHelper.Debug("导出开关: " + ServerVersion.ExportSwitch);
        Mogo.Util.LoggerHelper.Debug("平台更新开关: " + ServerVersion.IsPlatformUpdate);
        var compareFullProgramVersion = ServerVersion.FullResourceVersionInfo.Compare(LocalVersion.FullResourceVersionInfo) > 0;//服务首包资源版本号比本地版本号大
        var compareProgramVersion = ServerVersion.ProgramVersionInfo.Compare(LocalVersion.ProgramVersionInfo) > 0;//服务程序版本号比本地版本号大
        if (compareFullProgramVersion)
        {
            var compareFirstProgramVersion = ServerVersion.FirstResourceVersionInfo.Compare(LocalVersion.FirstResourceVersionInfo) > 0;//服务首包资源版本号比本地版本号大
            AsynResult.BeginInvoke((compareProgramVersion || compareFirstProgramVersion) ? UpdateType.FirstRes : UpdateType.None, null, null);
        }
        else
        {
            var compareResourceVersion = ServerVersion.ResourceVersionInfo.Compare(LocalVersion.ResourceVersionInfo) > 0;//服务器资源版本比本地高
            LoggerHelper.Debug("compareResourceVersion: " + compareResourceVersion);
            AsynResult.BeginInvoke((compareProgramVersion || compareResourceVersion) ? UpdateType.NormalRes : UpdateType.None, null, null);
        }
    }
    public bool CheckAndDownload(Action<bool> fileDecompress, Action<int, int, string> taskProgress, Action<int, long, long> progress, Action finished, Action<Exception> error)
    {
        //更新apk
        if (ServerVersion.ProgramVersionInfo.Compare(LocalVersion.ProgramVersionInfo) > 0)
        {
#if !UNITY_IPHONE && !APPSTORE
            if (ServerVersion.IsSkipApkUpdate)//针对使用平台自动更新，跳过不必要的检查
            {
                LoggerHelper.Debug("跳过Apk更新");
                if (finished != null)
                    finished();
                return true;
            }

            LoggerHelper.Debug("服务器apk版本高，下载apk");
            var fileUrl = ServerVersion.ApkUrl;
            if (ServerVersion.IsOpenUrl)//增加跳转更新处理
            {
                OpenUrl(fileUrl);
                return true;
            }
            //   /sdcard/
            string apkPath = SystemConfig.ResourceFolder;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    apkPath = Application.persistentDataPath + SystemConfig.MiddlePath + "/temp/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    apkPath = Application.persistentDataPath + SystemConfig.MiddlePath + "/temp/";
                    break;
                default:
                    LoggerHelper.Error("Did not define the apkPath of " + Application.platform.ToString());
                    break;
            }
            if (!Directory.Exists(apkPath))
                Directory.CreateDirectory(apkPath);
            var localFile = String.Concat(apkPath, Utils.GetFileName(fileUrl));
            Action onFinish = () => { };
            if (!VersionManager.Instance.ServerVersion.IsPlatformUpdate)
                AsynDownloadApk(taskProgress, localFile, fileUrl, progress, onFinish, error);
            else
            {
                SetPlatformUpdateCallback();
                //如果使用平台更新,先导出后下载
                ExportStreamingAssetWhenDownloadApk(PlatformUpdate);
            }
#else
			OpenUrl(ServerVersion.ApkUrl);
#endif
            return true;
        }

        //更新资源
        if (ServerVersion.ResourceVersionInfo.Compare(LocalVersion.ResourceVersionInfo) > 0)//服务资源版本号比本地版本号大
        {
            Mogo.Util.LoggerHelper.Debug("服务器资源版本号比本地版本号大");
            AsynDownloadUpdatePackage(fileDecompress, ServerVersion.ResourceVersionInfo, LocalVersion.ResourceVersionInfo, ServerVersion.PackageMD5Dic, ServerVersion.PackageUrl,
              ServerVersion.PackageMd5List, taskProgress, progress, finished, error);
            return true;
        }

        if (finished != null)
            finished();
        return false;
    }

    public void OpenUrl(string url)
    {
        ForwardLoadingMsgBoxLib.Instance.ShowMsgBox(
            DefaultUI.dataMap[2002].content,
            DefaultUI.dataMap[7].content,
            DefaultUI.dataMap[2001].content,
            (b) =>
            {
                if (b)
                {
                    Application.OpenURL(url);
                }
                Application.Quit();
            });
    }

    //设置平台更新的回调
    void SetPlatformUpdateCallback()
    {
#if UNITY_ANDROID
        LoggerHelper.Info("Init PlatformUpateCallback");
        DriverLib.Instance.gameObject.AddComponent("PlatformUpdateCallback");
        var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var mainActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        mainActivity.Call("setUpdateCallBack", "Driver");
#endif
    }

    /// <summary>
    /// 检查版本。
    /// </summary>
    public void CheckVersion(Action<bool> fileDecompress, Action<int, int, string> taskProgress, Action<int, long, long> progress, Action finished, Action<Exception> error)
    {
        BeforeCheck((result) =>
        {
            LoggerHelper.Debug(result);
            if (result == UpdateType.NormalRes)
            {
                ModalCheckNetwork(() => CheckAndDownload(fileDecompress, taskProgress, progress, finished, error));
            }
            else if (result == UpdateType.FirstRes)
            {
                CheckAndDownloadFirstPkg(fileDecompress, taskProgress, progress, () =>
                {
                    //大小包资源下载完后，使用FullResourceVersion作为完整包标记
                    LocalVersion.FullResourceVersionInfo = ServerVersion.FullResourceVersionInfo;
                    SaveVersion(LocalVersion);
                    //继续检查资源版本更新
                    CheckVersion(fileDecompress, taskProgress, progress, finished, error);
                }, error);
            }
            else
            {
                LoggerHelper.Debug("不需要更新apk和pkg");
                if (finished != null)
                    finished();
            }
        },
            () => { error(new Exception("download version file time out.")); });
    }
    //使用平台更新下载apk时导出资源，先导出资源再下载apk
    internal void ExportStreamingAssetWhenDownloadApk(Action finished)
    {
        ResourceIndexInfo.Instance.Init(Application.streamingAssetsPath + "/ResourceIndexInfo.txt", () =>
        {
            LoggerHelper.Debug("资源索引信息:" + ResourceIndexInfo.Instance.GetLeftFilePathes().Length);
            if (ResourceIndexInfo.Instance.GetLeftFilePathes().Length != 0)
            {
                //下载apk时导出资源
                var go = new StreamingAssetManager
                {
                    AllFinished = finished
                };
                go.UpdateApkExport();
            }
            else
            {
                LoggerHelper.Debug("没有streamingAssets,不需要导出");
                finished();
            }
        });
    }
    /// <summary>
    /// 获取更新包名称。
    /// </summary>
    /// <param name="currentVersion"></param>
    /// <param name="newVersion"></param>
    /// <returns></returns>
    public string GetPackageName(string currentVersion, string newVersion)
    {
        return string.Concat("package", currentVersion, "-", newVersion, ".pkg");
    }

    public bool CheckAndDownloadFirstPkg(Action<bool> fileDecompress, Action<int, int, string> taskProgress, Action<int, long, long> progress, Action finished, Action<Exception> error)
    {
        //更新apk
        if (ServerVersion.ProgramVersionInfo.Compare(LocalVersion.ProgramVersionInfo) > 0)
        {
            LoggerHelper.Debug("服务器apk版本高，下载apk");
            var fileUrl = ServerVersion.FirstApkUrl;
            if (ServerVersion.IsFirstPkgOpenUrl)//增加跳转更新处理
            {
                OpenUrl(fileUrl);
                return true;
            }
            string apkPath = SystemConfig.ResourceFolder;
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    apkPath = Application.persistentDataPath + SystemConfig.MiddlePath + "/temp/";
                    break;
                case RuntimePlatform.IPhonePlayer:
                    apkPath = Application.persistentDataPath + SystemConfig.MiddlePath + "/temp/";
                    break;
                default:
                    LoggerHelper.Error("Did not define the apkPath of " + Application.platform.ToString());
                    break;
            }
            if (!Directory.Exists(apkPath))
                Directory.CreateDirectory(apkPath);
            var localFile = String.Concat(apkPath, Utils.GetFileName(fileUrl));
            if (!VersionManager.Instance.ServerVersion.IsPlatformUpdate)
                AsynDownloadApk(taskProgress, localFile, fileUrl, progress, () => { }, error);
            else
            {
                PlatformUpdate();
            }
            return true;
        }

        //更新资源
        if (ServerVersion.FirstResourceVersionInfo.Compare(LocalVersion.FirstResourceVersionInfo) > 0)//服务资源版本号比本地版本号大
        {
            Mogo.Util.LoggerHelper.Debug("服务器资源版本号比本地版本号大");
            AsynDownloadUpdatePackage(fileDecompress, ServerVersion.FirstResourceVersionInfo, LocalVersion.FirstResourceVersionInfo,
                ServerVersion.FirstPackageMD5Dic, ServerVersion.FirstPackageUrl, ServerVersion.FirstPackageMd5List,
                taskProgress, progress, finished, error, true);
            return true;
        }

        if (finished != null)
            finished();
        return false;
    }

    private void AsynDownloadApkNoExport(Action<int, int, string> taskProgress, string fileName, string url, string md5, Action<int, long, long> progress, Action finished, Action<Exception> error)
    {
        var task = new DownloadTask
        {
            FileName = fileName,
            Url = url,
            Finished = finished,
            Error = error,
            TotalProgress = progress,
            MD5 = md5
        };
        LoggerHelper.Info("down load apk & md5: " + url + " " + md5);
        DownloadMgr.Instance.tasks = new List<DownloadTask> { task }; ;
        //添加taskProgress的回调
        DownloadMgr.Instance.TaskProgress = (total, current, filename) =>
        {
            if (taskProgress != null)
                taskProgress(total, current, filename);
        };
        DownloadMgr.Instance.AllDownloadFinished = () =>
        {
            DriverLib.Invoke(() =>
            {
                if (RuntimePlatform.IPhonePlayer == Application.platform)
                {
                    Action<bool> InstallIpa = (confirm) =>
                    {
                        if (confirm)
                        {
#if UNITY_IPHONE
							//IOSPlugins.InstallIpa(fileName);
#endif
                        }
                        else
                            Application.Quit();
                    };
                    ForwardLoadingMsgBoxLib.Instance.ShowMsgBox(DefaultUI.dataMap[11].content,
                                                         DefaultUI.dataMap[7].content,
                                                         DefaultUI.dataMap[12].content,
                                                         InstallIpa);
                }
                else
                {
                    InstallApk(fileName);
                }
            });
            if (finished != null)
                finished();
            LoggerHelper.Debug("apk安装成功");
        };
        DownloadMgr.Instance.CheckDownLoadList();
    }

    private void AsynDownloadApk(Action<int, int, string> taskProgress, string fileName, string url, Action<int, long, long> progress, Action finished, Action<Exception> error)
    {
        var task = new DownloadTask
        {
            FileName = fileName,
            Url = url,
            Finished = finished,
            Error = error,
            TotalProgress = progress,
            MD5 = ServerVersion.ApkMd5
        };
        LoggerHelper.Info("down load apk & md5: " + url + " " + ServerVersion.ApkMd5);
        var tasks = new List<DownloadTask> { task };
        DownloadMgr.Instance.tasks = tasks;
        //添加taskProgress的回调
        Action<int, int, string> TaskProgress = (total, current, filename) =>
        {
            if (taskProgress != null)
                taskProgress(total, current, filename);
        };
        DownloadMgr.Instance.TaskProgress = TaskProgress;
        StreamingAssetManager go = null;
        ResourceIndexInfo.Instance.Init(Application.streamingAssetsPath + "/ResourceIndexInfo.txt", () =>
        {
            LoggerHelper.Info("资源索引信息:" + ResourceIndexInfo.Instance.GetLeftFilePathes().Length);
            if (ResourceIndexInfo.Instance.GetLeftFilePathes().Length == 0)
            {
                go = new StreamingAssetManager();
                go.UpdateProgress = false;
                go.ApkFinished = true;
            }
            else
            {
                //下载apk时导出资源
                go = new StreamingAssetManager { UpdateProgress = false };
                go.AllFinished = () =>
                {
                    LoggerHelper.Debug("打开资源导出完成的标识11ApkFinished");
                    go.ApkFinished = true;
                };
            }
        });
        DownloadMgr.Instance.AllDownloadFinished = () =>
        {
            LoggerHelper.Info("APK download finish, wait for export finish:" + fileName);
            if (go != null)
            {
                go.UpdateProgress = true;
                LoggerHelper.Debug("打开导出进度显示:" + go.ApkFinished);
                //先判断资源导出是否完成，再安装apk,没完成则等待完成
                while (!go.ApkFinished)
                {
                    System.Threading.Thread.Sleep(500);
                }
                LoggerHelper.Info("APK and export download finish.");
                go = null;
            }
            DriverLib.Invoke(() =>
            {
                if (RuntimePlatform.IPhonePlayer == Application.platform)
                {
                    Action<bool> InstallIpa = (confirm) =>
                    {
                        if (confirm)
                        {
#if UNITY_IPHONE
							//IOSPlugins.InstallIpa(fileName);
#endif
                        }
                        else
                            Application.Quit();
                    };
                    ForwardLoadingMsgBoxLib.Instance.ShowMsgBox(DefaultUI.dataMap[11].content,
                                                         DefaultUI.dataMap[7].content,
                                                         DefaultUI.dataMap[12].content,
                                                         InstallIpa);
                }
                else
                {
                    InstallApk(fileName);
                }
            });
            if (finished != null)
                finished();
            LoggerHelper.Debug("apk安装成功");
        };
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        while (go == null)
        {
            System.Threading.Thread.Sleep(50);
            if (stopwatch.ElapsedMilliseconds > 3000)
                break;
        }
        stopwatch.Stop();
        //开始下载apk同时导出资源文件
        if (go != null && !go.ApkFinished)
        {
            LoggerHelper.Debug("apk下载同时导出资源");
            go.UpdateApkExport();
        }
        DownloadMgr.Instance.CheckDownLoadList();

    }

    private void InstallApk(string apkPath)
    {
        LoggerHelper.Info("Call Install apk: " + apkPath);
        //Application.OpenURL(apkPath);//OpenURL容易闪退
        //Application.Quit();
#if UNITY_ANDROID
        var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var mainActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        mainActivity.Call("InstallApk", apkPath);
        //Application.Quit();
        TimerHeap.AddTimer(1000, 0, Application.Quit);
#endif
    }

    public void PlatformUpdate()
    {
#if UNITY_ANDROID
        LoggerHelper.Debug("安卓上更新apk");
        var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var mainActivity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        mainActivity.Call("updateVersion");
#endif
    }

    public VersionManagerInfo GetVersionInXML(string xml)
    {
        try
        {
            var children = XMLParser.LoadXML(xml);
            if (children != null && children.Children != null && children.Children.Count != 0)
            {
                var result = new VersionManagerInfo();
                var props = typeof(VersionManagerInfo).GetProperties();
                foreach (SecurityElement item in children.Children)
                {
                    try
                    {
                        foreach (var prop in props)
                        {
                            if (prop != null && prop.Name == item.Tag && !string.IsNullOrEmpty(item.Text))
                            {
                                var value = Utils.GetValue(item.Text, prop.PropertyType);
                                if (value != null)
                                    prop.SetValue(result, value, null);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerHelper.Error("GetVersionInXML error: " + item.Tag + "\n" + ex.Message);
                    }
                }
                return result;
            }
            else
                return new VersionManagerInfo();
        }
        catch (Exception ex)
        {
            LoggerHelper.Except(ex);
            return new VersionManagerInfo();
        }
    }

    public void SaveVersion(VersionManagerInfo version)
    {
        LoggerHelper.Debug("call SaveVersion");
        var props = typeof(VersionManagerInfo).GetProperties();
        var root = new System.Security.SecurityElement("root");
        foreach (var item in props)
        {
            root.AddChild(new System.Security.SecurityElement(item.Name, item.GetGetMethod().Invoke(version, null) as String));
        }
        XMLParser.SaveText(SystemConfig.VersionPath, root.ToString());
    }

    public void AsynDownloadUpdatePackage(Action<bool> fileDecompress, VersionCodeInfo serverVersion, VersionCodeInfo localVersion,
                                        Dictionary<String, String> packageMD5Dic, String packageUrl, string packageMd5List,
                                        Action<int, int, string> taskProgress, Action<int, long, long> progress, Action finished, Action<Exception> error,
        bool isFirstRes = false, bool forFullPkg = false)
    {
        LoggerHelper.Debug("下载包列表");

        DownloadPackageInfoList(packageMd5List, (packageInfoList) =>
        {
            var downloadList = (from packageInfo in packageInfoList
                                where packageInfo.LowVersion.Compare(localVersion) >= 0 && packageInfo.HighVersion.Compare(serverVersion) <= 0
                                select new KeyValuePair<string, string>(packageInfo.HighVersion.ToString(), packageInfo.Name)).ToList();
            if (downloadList.Count != 0)
            {
                LoggerHelper.Debug("开始下载包列表");
                DownDownloadPackageList(fileDecompress, packageUrl, downloadList, packageMD5Dic, taskProgress, progress, finished, error, isFirstRes, forFullPkg);
            }
            else
            {
                LoggerHelper.Debug("更新包数目为0");
                if (finished != null)
                    finished();
            }

        }, () => { error(new Exception("DownloadPackageInfoList error.")); }, isFirstRes);
    }
    /// <summary>
    /// 下载补丁包
    /// </summary>
    /// <param name="fileDecompress">文件下载完成后正在解压回调</param>
    /// <param name="packageUrl">补丁包所在文件夹地址</param>
    /// <param name="downloadList">需要下载的文件列表，key: 版本号，value: 文件名</param>
    /// <param name="packageMD5Dic">文件对应md5列表，key: 文件名，value: md5</param>
    /// <param name="taskProgress">下载任务回调</param>
    /// <param name="progress">单个下载任务的进度回调</param>
    /// <param name="finished">下载完成回调</param>
    /// <param name="error">下载出错回调</param>
    /// <param name="isFirstRes">是否为首包资源下载</param>
    /// <param name="forFullPkg">是否为首包更新完整包资源下载</param>
    private void DownDownloadPackageList(Action<bool> fileDecompress, string packageUrl, List<KeyValuePair<String, String>> downloadList,
                                        Dictionary<String, String> packageMD5Dic,
                                        Action<int, int, string> taskProgress, Action<int, long, long> progress, Action finished, Action<Exception> error,
                                        bool isFirstRes = false, bool forFullPkg = false)
    {
        var allTask = new List<DownloadTask>();
        //收集downloadlist，生成所有下载任务
        for (int i = 0; i < downloadList.Count; i++)
        {
            LoggerHelper.Debug("收集数据包，生成任务:" + i);
            KeyValuePair<String, String> kvp = downloadList[i];
            LoggerHelper.Debug("下载文件名：" + kvp.Value);
            String localFile = String.Concat(SystemConfig.ResourceFolder, kvp.Value);

            Action OnDownloadFinished = () =>
            {
                LoggerHelper.Debug("下载完成，进入完成回调");
                LoggerHelper.Debug("本地文件：" + File.Exists(localFile));
                if (File.Exists(localFile))
                    FileAccessManager.DecompressFile(localFile);
                LoggerHelper.Debug("解压完成，保存版本信息到version.xml: " + isFirstRes);
                if (File.Exists(localFile))
                    File.Delete(localFile);
                if (isFirstRes)
                {
                    if (forFullPkg)
                        LocalVersion.FullResourceVersionInfo = new VersionCodeInfo(kvp.Key);
                    else
                        LocalVersion.FirstResourceVersionInfo = new VersionCodeInfo(kvp.Key);
                }
                else
                    LocalVersion.ResourceVersionInfo = new VersionCodeInfo(kvp.Key);
                SaveVersion(LocalVersion);
                //if (taskProgress != null)
                //    taskProgress(downloadList.Count, i + 1);
            };
            String fileUrl = String.Concat(packageUrl, kvp.Value);
            LoggerHelper.Debug("fileUrl：" + fileUrl);
            var task = new DownloadTask
            {
                FileName = localFile,
                Url = fileUrl,
                Finished = OnDownloadFinished,
                Error = error,
                TotalProgress = progress
            };
            //task.Error = erroract;
            string fileNoEXtension = kvp.Value;// task.FileName.LastIndexOf('/') != 0 ? task.FileName.Substring(task.FileName.LastIndexOf('/') + 1) : task.FileName;
            LoggerHelper.Debug(task.FileName + "::fileNoEXtension::" + fileNoEXtension);
            if (packageMD5Dic.ContainsKey(fileNoEXtension))
            {
                task.MD5 = packageMD5Dic[fileNoEXtension];
                allTask.Add(task);
            }
            else
            {
                error(new Exception("down pkg not exit :" + fileNoEXtension));
                return;
            }
        }
        //全部下载完成的回调
        Action AllFinished = () =>
        {
            LoggerHelper.Debug("更新包全部下载完成");
            finished();
        };
        //添加taskProgress的回调
        Action<int, int, string> TaskProgress = (total, current, filename) =>
        {
            if (taskProgress != null)
                taskProgress(total, current, filename);
        };
        //添加文件解压的回调函数
        Action<bool> filedecompress = (decompress) =>
        {
            if (fileDecompress != null)
                fileDecompress(decompress);
        };
        //所有任务收集好了,开启下载
        LoggerHelper.Debug("所有任务收集好了,开启下载");
        DownloadMgr.Instance.tasks = allTask;
        DownloadMgr.Instance.AllDownloadFinished = AllFinished;
        DownloadMgr.Instance.TaskProgress = TaskProgress;
        DownloadMgr.Instance.FileDecompress = filedecompress;
        DownloadMgr.Instance.CheckDownLoadList();
    }

    //获得包信息列表
    private void DownloadPackageInfoList(string packageMd5List, Action<List<PackageInfo>> AsynResult, Action OnError, bool isFirstRes = false)
    {
        DownloadMgr.Instance.AsynDownLoadText(packageMd5List, (content) =>
        {
            bool res;
            if (isFirstRes)//是否为首包资源
                res = ServerVersion.ReadFirstResourceList(content);
            else
                res = ServerVersion.ReadResourceList(content);
            if (res)
            {
                if (isFirstRes)
                    AsynResult(ServerVersion.FirstPackageInfoList);
                else
                    AsynResult(ServerVersion.PackageInfoList);
            }
            else
            {
                if (OnError != null)
                    OnError();
            }
        }, OnError);
    }
}

public class VersionManagerInfo
{
    /// <summary>
    /// 服务器程序版本
    /// </summary>
    public VersionCodeInfo ProgramVersionInfo;
    public VersionCodeInfo ResourceVersionInfo;
    /// <summary>
    /// 服务器首包资源版本
    /// </summary>
    public VersionCodeInfo FirstResourceVersionInfo;
    /// <summary>
    /// 服务器完整包资源版本
    /// </summary>
    public VersionCodeInfo FullResourceVersionInfo;
    /// <summary>
    /// 服务器程序版本
    /// </summary>
    public string ProgramVersion
    {
        get
        {
            return ProgramVersionInfo.ToString();
        }
        set
        {
            ProgramVersionInfo = new VersionCodeInfo(value);
        }
    }
    /// <summary>
    /// 服务器资源版本
    /// </summary>
    public string ResourceVersion
    {
        get
        {
            return ResourceVersionInfo.ToString();
        }
        set
        {
            ResourceVersionInfo = new VersionCodeInfo(value);
        }
    }
    /// <summary>
    /// 服务器首包资源版本
    /// </summary>
    public string FirstResourceVersion
    {
        get
        {
            return FirstResourceVersionInfo.ToString();
        }
        set
        {
            FirstResourceVersionInfo = new VersionCodeInfo(value);
        }
    }
    /// <summary>
    /// 服务器完整包资源版本
    /// </summary>
    public string FullResourceVersion
    {
        get
        {
            return FullResourceVersionInfo.ToString();
        }
        set
        {
            FullResourceVersionInfo = new VersionCodeInfo(value);
        }
    }
    /// <summary>
    /// 服务器包地址
    /// </summary>
    public String PackageUrl { get; set; }
    /// <summary>
    /// 服务器首包地址
    /// </summary>
    public String FirstPackageUrl { get; set; }
    /// <summary>
    /// 服务器Apk地址
    /// </summary>
    public String ApkUrl { get; set; }
    /// <summary>
    /// apk md5
    /// </summary>
    public String ApkMd5 { get; set; }
    /// <summary>
    /// 服务器小包Apk地址
    /// </summary>
    public String FirstApkUrl { get; set; }
    /// <summary>
    /// 小包apk md5
    /// </summary>
    public String FirstApkMd5 { get; set; }
    /// <summary>
    /// 资源包列表
    /// </summary>
    public String PackageMd5List { get; set; }//package的md5码
    /// <summary>
    /// 首包资源包列表
    /// </summary>
    public String FirstPackageMd5List { get; set; }//package的md5码
    /// <summary>
    /// 导出开关
    /// </summary>
    public bool ExportSwitch { get; set; }
    /// <summary>
    /// 平台更新开关
    /// </summary>
    public bool IsPlatformUpdate { get; set; }
    /// <summary>
    /// 完整包跳转更新开关
    /// </summary>
    public bool IsOpenUrl { get; set; }
    /// <summary>
    /// 小包跳转更新开关
    /// </summary>
    public bool IsFirstPkgOpenUrl { get; set; }
    /// <summary>
    /// 是否跳过Apk更新
    /// </summary>
    public bool IsSkipApkUpdate { get; set; }
    //package包对应的md5码
    public Dictionary<String, String> PackageMD5Dic = new Dictionary<string, string>();
    public List<PackageInfo> PackageInfoList = new List<PackageInfo>();
    public Dictionary<String, String> FirstPackageMD5Dic = new Dictionary<string, string>();
    public List<PackageInfo> FirstPackageInfoList = new List<PackageInfo>();
    public VersionManagerInfo()
    {
        ProgramVersionInfo = new VersionCodeInfo("0.0.0.1");
        ResourceVersionInfo = new VersionCodeInfo("0.0.0.0");
        FirstResourceVersionInfo = new VersionCodeInfo("0.0.0.0");
        FullResourceVersionInfo = new VersionCodeInfo("0.0.0.0");
        PackageUrl = String.Empty;
        ApkUrl = String.Empty;
        ExportSwitch = false;
        IsPlatformUpdate = true;
        IsOpenUrl = false;
    }

    public bool ReadResourceList(String packageMD5Content)
    {
        return ReadMd5FromXML(packageMD5Content, ref PackageMD5Dic, ref PackageInfoList);
    }

    public bool ReadFirstResourceList(String packageMD5Content)
    {
        return ReadMd5FromXML(packageMD5Content, ref FirstPackageMD5Dic, ref FirstPackageInfoList);
    }

    public bool ReadMd5FromXML(String packageMD5Content, ref Dictionary<String, String> packageMD5Dic, ref List<PackageInfo> packageInfoList)
    {
        var xml = XMLParser.LoadXML(packageMD5Content);
        if (xml == null)
        {
            return false;
        }
        packageInfoList.Clear();
        packageMD5Dic.Clear();
        foreach (SecurityElement item in xml.Children)
        {
            try
            {
                var info = new PackageInfo();
                var packagename = item.Attribute("n");
                //包名
                info.Name = packagename;
                var version = packagename.Substring(7, packagename.Length - 11);
                var firstversion = version.Substring(0, version.IndexOf('-'));
                //低版本
                info.LowVersion = new VersionCodeInfo(firstversion);
                var endversion = version.Substring(version.IndexOf('-') + 1);
                //高版本
                info.HighVersion = new VersionCodeInfo(endversion);
                //Md5值
                info.Md5 = item.Text;
                packageInfoList.Add(info);
                packageMD5Dic[info.Name] = info.Md5;
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex);
            }
        }
        return true;
    }

    public override string ToString()
    {
        return ResourceVersionInfo.ToString();
    }
}

/// <summary>
/// 从包列表取出来的单个包信息
/// </summary>
public class PackageInfo
{
    //包名
    public string Name;
    //低版本
    public VersionCodeInfo LowVersion;
    //高版本
    public VersionCodeInfo HighVersion;
    //md5值
    public string Md5;
}

/// <summary>
/// 版本号。
/// </summary>
public class VersionCodeInfo
{
    private List<int> m_tags = new List<int>();

    /// <summary>
    /// 构造函数。
    /// </summary>
    /// <param name="version">版本号字符串。</param>
    public VersionCodeInfo(String version)
    {
        if (string.IsNullOrEmpty(version))
            return;
        var versions = version.Split('.');
        for (int i = 0; i < versions.Length; i++)
        {
            int v;
            if (int.TryParse(versions[i], out v))
                m_tags.Add(v);
            else
                m_tags.Add(v);
        }
    }

    /// <summary>
    /// 获取比目前版本高一个版本的字符串。
    /// </summary>
    /// <returns></returns>
    public string GetUpperVersion()
    {
        var lastTag = m_tags[m_tags.Count - 1] + 1;
        var sb = new StringBuilder();
        for (int i = 0; i < m_tags.Count - 1; i++)
        {
            sb.AppendFormat("{0}.", m_tags[i]);
        }
        sb.Append(lastTag);
        return sb.ToString();
    }

    /// <summary>
    /// 获取比目前版本低一个版本的字符串。
    /// </summary>
    /// <returns></returns>
    public string GetLowerVersion()
    {
        var lastTag = m_tags[m_tags.Count - 1] - 1;
        var sb = new StringBuilder();
        for (int i = 0; i < m_tags.Count - 1; i++)
        {
            sb.AppendFormat("{0}.", m_tags[i]);
        }
        sb.Append(lastTag);
        return sb.ToString();
    }

    /// <summary>
    /// 返回无小数点版本号。
    /// </summary>
    /// <returns></returns>
    public string ToShortString()
    {
        var sb = new StringBuilder();
        foreach (var item in m_tags)
        {
            sb.Append(item);
        }
        return sb.ToString();
    }

    public int ToInt()
    {
        int result = 0;
        for (int i = 0; i < m_tags.Count; i++)
        {
            result += m_tags[i] * (int)Math.Pow(10, (3 - i) * 2);
        }
        return result;
    }

    /// <summary>
    /// 比较版本号，自己比参数大，返回1，比参数小，返回-1，相等返回0。
    /// </summary>
    /// <param name="info">比较版本号。</param>
    /// <returns>自己比参数大，返回1，比参数小，返回-1，相等返回0。</returns>
    public int Compare(VersionCodeInfo info)
    {
        var count = this.m_tags.Count < info.m_tags.Count ? this.m_tags.Count : info.m_tags.Count;
        for (int i = 0; i < count; i++)
        {
            if (this.m_tags[i] == info.m_tags[i])
                continue;
            else
                return this.m_tags[i] > info.m_tags[i] ? 1 : -1;
        }
        return 0;
    }

    /// <summary>
    /// 获取版本号字符串。
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var item in m_tags)
        {
            sb.AppendFormat("{0}.", item);
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }
}

public enum UpdateType
{
    None,
    NormalRes,
    FirstRes
}

public class CheckTimeout
{
    public void AsynIsNetworkTimeout(Action<bool> AsynResult)
    {
        string networkUrl;
        if (Application.platform != RuntimePlatform.WindowsWebPlayer && File.Exists(SystemConfig.CfgPath))
        {
            SystemConfig.LoadCfgInfo((result) =>
            {
                networkUrl = SystemConfig.GetCfgInfoUrl(SystemConfig.SERVER_LIST_URL_KEY);
                LoggerHelper.Info("cfg exist. " + result + " networkUrl: " + networkUrl);
                TryAsynDownloadText(networkUrl, AsynResult);
            });
        }
        else
        {
            networkUrl = Utils.LoadResource(Utils.GetFileNameWithoutExtention(SystemConfig.CFG_FILE));
            TryAsynDownloadText(networkUrl, AsynResult);
        }
    }

    private void TryAsynDownloadText(string url, Action<bool> AsynResult)
    {
        DownloadMgr.Instance.AsynDownLoadText(url, (text) =>
        {
            if (!String.IsNullOrEmpty(text))
                AsynResult(true);
            else
                AsynResult(false);
        }, () =>
        {
            AsynResult(false);
        });
    }
}
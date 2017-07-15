using Mogo.Util;
using System;
using System.IO;

public class SystemSwitch
{
    private static Boolean m_releaseMode = false;
    private static Boolean m_destroyResource = false;
    private static Boolean m_useFileSystem = false;
    private static Boolean m_useHmf = false;
    private static Boolean m_destroyAllUI = false;
    private static Boolean m_usePlatformSDK = false;
    private static Boolean m_enableDeleteCharacter = false;
    private static Boolean m_pcSdkInEditor = false;
    private static Boolean m_isShowDebug = false;
    private static Boolean m_isRecordRes = false;
    private static Boolean m_isDevBuild = false;

    public static Boolean IsDevBuild
    {
        get { return m_isDevBuild; }
        set { m_isDevBuild = value; }
    }

    public static Boolean IsRecordRes
    {
        get { return SystemSwitch.m_isRecordRes; }
        set { SystemSwitch.m_isRecordRes = value; }
    }

    public static Boolean IsShowDebug
    {
        get { return m_isShowDebug; }
        set { m_isShowDebug = value; }
    }

    public static Boolean PcSdkInEditor
    {
        get { return m_pcSdkInEditor; }
        set { m_pcSdkInEditor = value; }
    }
    /// <summary>
    /// 是否为发布模式。
    /// </summary>
    public static Boolean ReleaseMode
    {
        get { return m_releaseMode; }
        set { m_releaseMode = value; }
    }

    public static Boolean DestroyResource
    {
        get { return m_destroyResource; }
        set { m_destroyResource = value; }
    }

    public static Boolean UseFileSystem
    {
        get { return m_useFileSystem; }
        set { m_useFileSystem = value; }
    }

    public static Boolean UseHmf
    {
        get { return m_useHmf; }
        set { m_useHmf = value; }
    }

    /// <summary>
    /// 控制是否消耗UI资源。（使用MFUIResourceManager框架的GameObject由框架控制，原有UI会受此变量控制是否Destroy GameObject）
    /// </summary>
    public static Boolean DestroyAllUI
    {
        get { return m_destroyAllUI; }
        set { m_destroyAllUI = value; }
    }

    public static Boolean UsePlatformSDK
    {
        get { return m_usePlatformSDK; }
        set { m_usePlatformSDK = value; }
    }

    /// <summary>
    /// 是否可以删除角色
    /// 0:不能删除角色,1:可以删除角色
    /// </summary>
    public static Boolean EnableDeleteCharacter
    {
        get { return m_enableDeleteCharacter; }
        set { m_enableDeleteCharacter = value; }
    }

    //public readonly static Boolean ReleaseMode = false;
    //public readonly static Boolean DestroyResource = false;
    //public readonly static Boolean UseFileSystem = false;
    //public readonly static Boolean UseHmf = false;
    ////public readonly static Boolean UseStreamingAssets = false;
    //public readonly static Boolean DestroyAllUI = false;
    //public readonly static Boolean UsePlatformSDK = true;

    public static void InitSystemSwitch()
    {
        String content;
#if !UNITY_WEBPLAYER
        if (File.Exists(SystemConfig.SystemSwitchPath))
            content = Utils.LoadFile(SystemConfig.SystemSwitchPath);
        else
#endif
        content = Utils.LoadResource(Utils.GetFileNameWithoutExtention(SystemConfig.SystemSwitchPath));

        if (!String.IsNullOrEmpty(content))
        {
            try
            {
                var xml = XMLParser.LoadXML(content);
                var props = typeof(SystemSwitch).GetProperties();
                foreach (System.Security.SecurityElement item in xml.Children)
                {
                    foreach (var prop in props)
                    {
                        if (item.Tag == prop.Name)
                        {
                            prop.GetSetMethod().Invoke(null, new object[] { Utils.GetValue(item.Text, typeof(bool)) });
                            //prop.SetValue(null, Utils.GetValue(item.Text, typeof(bool)), null);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Except(ex);
            }
        }
    }
}
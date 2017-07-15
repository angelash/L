using System;


public class DownloadUnit
{
	public String m_RemoteFullPath=String.Empty;
	public String m_LocalCachePath=String.Empty;
	public String m_LocalDestination=String.Empty;
	public String m_MD5=String.Empty;
	public DownloadUnit(String remotePath,String cachePath,String destinationPath,String md5)
	{
		m_RemoteFullPath=remotePath;
		m_LocalCachePath=cachePath;
		m_LocalDestination=destinationPath;
		m_MD5=md5;
	}
}

public interface IDownloadManager
{
    //Sync download file and return the contents of the file
    void    SyncDownload(String url,System.Action<String> callback);
    //Sync download file and save it to localpath
    void    SyncDownloadFileAndSaveAs(String url, String localPath);
    //Async download file and return the contents of the file
    void    AsyncDownload(String url,System.Action callback);
    //Async download file and save it to localpath
    void    AsyncDownloadFileAndSaveAs(String url, String localPath);
    //Async download file and save it to ある　ところ,that support つずける doanload
    void    AdvancedAsyncDownloadFileAndSaveAs(DownloadUnit [] downloadLists,Action callback);
}
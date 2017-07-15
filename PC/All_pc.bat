:找到注册表中unity的位置
for /f "delims=" %%i in ('REG.EXE QUERY "HKEY_CLASSES_ROOT\com.unity3d.kharma\DefaultIcon" /VE ') do set InstallDir="%%i"
set InstallDir=%InstallDir:~22,-11%
echo Unity3d的安装路径：%InstallDir%

cd /d %~dp0..\
C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe MogoSolution\Common\Common.csproj /property:Configuration="UNITY_STANDALONE_WIN"
ping 127.0.0.1 -n 1

cd /d %~dp0..\
C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe MogoSolution\LoaderLib\LoaderLib.csproj /property:Configuration="UNITY_STANDALONE_WIN"
ping 127.0.0.1 -n 1

cd /d %~dp0..\
C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe MogoSolution\NGUI\NGUI.csproj /property:Configuration="UNITY_STANDALONE_WIN"
ping 127.0.0.1 -n 1


pause
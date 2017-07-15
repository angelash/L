

cd /d %~dp0..\
C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe MogoSolution\LoaderLib\LoaderLib.csproj /property:Configuration="UNITY_WEBPLAYER"
ping 127.0.0.1 -n 1

cd /d %~dp0..\
C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe MogoSolution\NGUI\NGUI.csproj /property:Configuration="UNITY_WEBPLAYER"
ping 127.0.0.1 -n 1

cd /d %~dp0..\
C:\\Windows\\Microsoft.NET\\Framework\\v4.0.30319\\msbuild.exe MogoSolution\Common\Common.csproj /property:Configuration="UNITY_WEBPLAYER"
ping 127.0.0.1 -n 1

pause
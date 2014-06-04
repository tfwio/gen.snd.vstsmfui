set mpath=%windir%\microsoft.net\framework\v4.0.30319;
set path=%mpath%;%path%
set msb=%mpath%\msbuild
msbuild svninfo.proj /t:build
pause
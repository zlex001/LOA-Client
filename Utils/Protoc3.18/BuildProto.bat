@echo off

set "PROTOC_EXE=%cd%\ProtocTool\protoc.exe"
set "WORK_DIR=%cd%\Protos"

set "CS_OUT_PATH=%cd%\Output\Csharp"
set "LUA_OUT_PATH=%cd%\Output\Lua"
set "JAVA_OUT_PATH=%cd%\Output\Java"

rd /s /Q %cd%\Output\

md %CS_OUT_PATH%
md %LUA_OUT_PATH%
md %JAVA_OUT_PATH%

for /f "delims=" %%i in ('dir /b Protos "Protos/*.proto"') do (
   echo gen Protocols/%%i...
   %PROTOC_EXE%  --proto_path="%WORK_DIR%" --csharp_out="%CS_OUT_PATH%" "%WORK_DIR%\%%i"
   %PROTOC_EXE%  --proto_path="%WORK_DIR%" --java_out="%JAVA_OUT_PATH%" "%WORK_DIR%\%%i"
   %PROTOC_EXE%  --proto_path="%WORK_DIR%" -o "%LUA_OUT_PATH%\%%i" "%WORK_DIR%\%%i"
)

cd %LUA_OUT_PATH%
ren *.proto *.bytes

echo finish... 

pause

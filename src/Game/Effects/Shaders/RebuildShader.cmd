@echo off

set name=%1
set nameWithoutExt=%name:~0,-3%
set skipVulcan=

if "%2"==""  set skipVulcan=true
if "%2"==true set skipVulcan=true

cd /d %~dp0\..\..\
dotnet tool run mgfxc %name% %nameWithoutExt%.ogl.mgfxo /profile:OpenGL /Debug
dotnet tool run mgfxc %name% %nameWithoutExt%.dx11.mgfxo /profile:DirectX_11 /Debug

if defined skipVulcan goto :create-resources

dotnet tool run mgfxc %name% %nameWithoutExt%.vulkan.mgfxo /profile:Vulkan /Debug

:create-resources
dotnet tool run resource-creator . -f *.mgfxo -o Effects\Shaders.resources
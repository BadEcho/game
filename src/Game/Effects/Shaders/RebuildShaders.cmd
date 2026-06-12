@echo off

cd /d %~dp0
dotnet tool run mgfxc StandardEffect.fx StandardEffect.ogl.mgfxo /profile:OpenGL
dotnet tool run mgfxc StandardEffect.fx StandardEffect.dx11.mgfxo /profile:DirectX_11
dotnet tool run mgfxc DistanceFieldFontEffect.fx DistanceFieldFontEffect.ogl.mgfxo /profile:OpenGL
dotnet tool run mgfxc DistanceFieldFontEffect.fx DistanceFieldFontEffect.dx11.mgfxo /profile:DirectX_11
dotnet tool run mgfxc PointLightEffect.fx PointLightEffect.ogl.mgfxo /profile:OpenGL
dotnet tool run mgfxc PointLightEffect.fx PointLightEffect.dx11.mgfxo /profile:DirectX_11
dotnet tool run mgfxc CompositeEffect.fx CompositeEffect.ogl.mgfxo /profile:OpenGL
dotnet tool run mgfxc CompositeEffect.fx CompositeEffect.dx11.mgfxo /profile:DirectX_11
dotnet tool run resource-creator . -f *.mgfxo -o ..\Shaders
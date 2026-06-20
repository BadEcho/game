@echo off

cd /d %~dp0
dotnet tool run mgfxc StandardEffect.fx StandardEffect.ogl.mgfxo /profile:OpenGL /Debug
dotnet tool run mgfxc StandardEffect.fx StandardEffect.dx11.mgfxo /profile:DirectX_11 /Debug
dotnet tool run mgfxc StandardEffect.fx StandardEffect.vulkan.mgfxo /profile:Vulkan 
dotnet tool run mgfxc DistanceFieldFontEffect.fx DistanceFieldFontEffect.ogl.mgfxo /profile:OpenGL /Debug
dotnet tool run mgfxc DistanceFieldFontEffect.fx DistanceFieldFontEffect.dx11.mgfxo /profile:DirectX_11 /Debug
dotnet tool run mgfxc DistanceFieldFontEffect.fx DistanceFieldFontEffect.vulkan.mgfxo /profile:Vulkan 
dotnet tool run mgfxc PointLightEffect.fx PointLightEffect.ogl.mgfxo /profile:OpenGL /Debug
dotnet tool run mgfxc PointLightEffect.fx PointLightEffect.dx11.mgfxo /profile:DirectX_11 /Debug
dotnet tool run mgfxc PointLightEffect.fx PointLightEffect.vulkan.mgfxo /profile:Vulkan 
dotnet tool run mgfxc ShadowHullEffect.fx ShadowHullEffect.ogl.mgfxo /profile:OpenGL /Debug
dotnet tool run mgfxc ShadowHullEffect.fx ShadowHullEffect.dx11.mgfxo /profile:DirectX_11 /Debug
dotnet tool run mgfxc ShadowHullEffect.fx ShadowHullEffect.vulkan.mgfxo /profile:Vulkan 
dotnet tool run mgfxc ShadowEffect.fx ShadowEffect.ogl.mgfxo /profile:OpenGL /Debug
dotnet tool run mgfxc ShadowEffect.fx ShadowEffect.dx11.mgfxo /profile:DirectX_11 /Debug
dotnet tool run mgfxc ShadowEffect.fx ShadowEffect.vulkan.mgfxo /profile:Vulkan 
dotnet tool run mgfxc CompositeEffect.fx CompositeEffect.ogl.mgfxo /profile:OpenGL /Debug
dotnet tool run mgfxc CompositeEffect.fx CompositeEffect.dx11.mgfxo /profile:DirectX_11 /Debug
dotnet tool run mgfxc CompositeEffect.fx CompositeEffect.vulkan.mgfxo /profile:Vulkan 
dotnet tool run resource-creator . -f *.mgfxo -o ..\Shaders
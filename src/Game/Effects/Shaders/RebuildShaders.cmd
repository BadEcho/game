@echo off

cd /d %~dp0
mgfxc StandardEffect.fx StandardEffect.ogl.mgfxo /profile:OpenGL
mgfxc StandardEffect.fx StandardEffect.dx11.mgfxo /profile:DirectX_11
mgfxc DistanceFieldFontEffect.fx DistanceFieldFontEffect.ogl.mgfxo /profile:OpenGL
mgfxc DistanceFieldFontEffect.fx DistanceFieldFontEffect.dx11.mgfxo /profile:DirectX_11
mgfxc PointLightEffect.fx PointLightEffect.ogl.mgfxo /profile:OpenGL
mgfxc PointLightEffect.fx PointLightEffect.dx11.mgfxo /profile:DirectX_11
mgfxc CompositeEffect.fx CompositeEffect.ogl.mgfxo /profile:OpenGL
mgfxc CompositeEffect.fx CompositeEffect.dx11.mgfxo /profile:DirectX_11
resource-creator . -f *.mgfxo -o ..\Shaders
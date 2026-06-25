// -----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2026 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using BadEcho.Game.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Effects;

/// <summary>
/// Provides a multiphase renderer that breaks up the drawing of sprites into several different phases (diffuse, lighting, etc.),
/// producing a composite image at the end that's drawn to the screen.
/// </summary>
public sealed class DeferredRenderer
{
    private readonly GraphicsDevice _device;
    
    private readonly CompositeEffect _compositeEffect;

    private DepthStencilState _stencilWrite;

    private DepthStencilState _stencilTest;
    private DepthStencilState _stencilShadowExclude;


    private BlendState _shadowBlendState;
    private RasterizerState _lightRasterizerState = new RasterizerState()
                                                    {
                                                        CullMode = CullMode.None,
                                                        ScissorTestEnable = true
                                                    };

    public DeferredRenderer(GraphicsDevice device)
    {

        _device = device;
        
        _rectangle = new Texture2D(_device, 1, 1, false, SurfaceFormat.Color);
        _rectangle.SetData([
            //new Color(0x80, 0x80, 0xff, 0xff)
            Color.White
        ]);
        _compositeEffect = new CompositeEffect(device);

        Rectangle viewportBounds = device.Viewport.Bounds;

        BackgroundBuffer = new RenderTarget2D(device,
                                           viewportBounds.Width,
                                           viewportBounds.Height,
                                           false,
                                           SurfaceFormat.Color,
                                           DepthFormat.None
            );

        ColorBuffer = new RenderTarget2D(device,
                                           viewportBounds.Width,
                                           viewportBounds.Height,
                                           false,
                                           SurfaceFormat.Color,
                                           DepthFormat.None);

        LightBuffer = new RenderTarget2D(device,
                                         viewportBounds.Width,
                                         viewportBounds.Height,
                                         false,
                                         SurfaceFormat.Color,
                                         DepthFormat.Depth24Stencil8);

        NormalBuffer = new RenderTarget2D(device,
                                          viewportBounds.Width,
                                          viewportBounds.Height,
                                          false,
                                          SurfaceFormat.Color,
                                          DepthFormat.None);

        _stencilWrite = new DepthStencilState
                        {
                            // instruct MonoGame to use the stencil buffer
                            StencilEnable = true,

                            // instruct every fragment to interact with the stencil buffer
                            StencilFunction = CompareFunction.LessEqual,

                            // every operation will replace the current value in the stencil buffer
                            //  with whatever value is in the ReferenceStencil variable
                            StencilPass = StencilOperation.IncrementSaturation,

                            // this is the value that will be written into the stencil buffer
                            ReferenceStencil = 1,

                            // ignore depth from the stencil buffer write/reads  
                            DepthBufferEnable = false
                        };

        _stencilTest = new DepthStencilState
                       {
                           // instruct MonoGame to use the stencil buffer
                           StencilEnable = true,

                           // instruct only fragments that have a current value EQUAl to the
                           //  ReferenceStencil value to interact
                           StencilFunction = CompareFunction.GreaterEqual,

                           // shadow hulls wrote `1`, so `0` means "not" shadow. 
                           ReferenceStencil = 1,

                           // do not change the value of the stencil buffer. KEEP the current value.
                           StencilPass = StencilOperation.Keep,

                           // ignore depth from the stencil buffer write/reads
                           DepthBufferEnable = false
                       };


        _shadowBlendState = new BlendState
                            {
                                ColorWriteChannels = ColorWriteChannels.None
                            };

        _stencilShadowExclude = new DepthStencilState
                                {
                                    // instruct MonoGame to use the stencil buffer
                                    StencilEnable = true,

                                    // in the setup, always set the pixel to '0'
                                    StencilFunction = CompareFunction.Always,

                                    // Write a '0' anywhere we don't want a shadow to appear
                                    ReferenceStencil = 0,

                                    // Overwrite the current value
                                    StencilPass = StencilOperation.Replace,

                                    // ignore depth from the stencil buffer write/reads
                                    DepthBufferEnable = false
                                };

    }

    public RenderTarget2D ColorBuffer
    { get; }

    public RenderTarget2D NormalBuffer
    { get; }

    public RenderTarget2D LightBuffer
    { get; }

    public RenderTarget2D BackgroundBuffer
    {
        get;
    }

    public DepthStencilState StencilWrite
        => _stencilWrite;

    public DepthStencilState StencilTest
        => _stencilTest;


    public void StartBackgroundPhase()
    {
        _device.SetRenderTarget(BackgroundBuffer);
        _device.Clear(Color.Transparent);
    }

    public StandardEffect StartDiffusePhase(RenderStates renderStates)
    {
        _device.SetRenderTarget(ColorBuffer);

        var effect = new  StandardEffect(_device);

        effect.Alpha = renderStates.Alpha ?? 1.0f;
        effect.MatrixTransform = renderStates.MatrixTransform;

        //spriteBatch.Begin();
        //spriteBatch.Draw(BackgroundBuffer, BackgroundBuffer.Bounds, Color.White);
        //spriteBatch.End();

        return effect;
    }

    public StandardEffect StartDiffusePhase(RenderStates renderStates, Texture2D normalAtlas)
    {
        _device.SetRenderTargets(
            new RenderTargetBinding[]
            {
                new RenderTargetBinding(ColorBuffer),
                new RenderTargetBinding(NormalBuffer)
            }
        );

        _device.Clear(Color.Transparent);

        var effect = new StandardEffect(_device) { NormalBuffer = normalAtlas };

        effect.Alpha = renderStates.Alpha ?? 1.0f;
        effect.MatrixTransform = renderStates.MatrixTransform;

        return effect;
    }

    public void StartLightPhase()
    {
        _device.SetRenderTarget(LightBuffer);
        _device.Clear(Color.Black);
    }

    public void DrawLights(SpriteBatch spriteBatch, PointLightEffect effect, ShadowEffect shadowEffect, RenderStates renderStates, IEnumerable<PointLight> lights, Sprite sprite,
                           Action<BlendState, DepthStencilState> prepareStencil)
    {
        _device.SetRenderTarget(LightBuffer);
        _device.Clear(Color.Black);

        foreach (var light in lights)
        {
            int diameter = light.Radius * 2;

            var destination = new Rectangle((int)(light.Position.X - light.Radius), (int)(light.Position.Y - light.Radius), diameter, diameter);

            //            var screenSize = new Vector2(_device.Viewport.Width, _device.Viewport.Height);
            var screenSize = new Vector2(LightBuffer.Width, LightBuffer.Height);


            _device.Clear(ClearOptions.Stencil, Color.Black, 0, 1);
            prepareStencil?.Invoke(_shadowBlendState, _stencilShadowExclude);

            //shadowEffect.ScreenSize = screenSize;
            //shadowEffect.MatrixTransform = renderStates.MatrixTransform ?? Matrix.Identity;
            //shadowEffect.LightPosition = light.Position;
            ////shadowHull.ShadowFadeStart = 0.00f;
            ////shadowHull.ShadowFadeEnd = 0.005f;

            spriteBatch.Begin(
                depthStencilState: _stencilWrite,

                effect: shadowEffect, blendState: _shadowBlendState, rasterizerState:
                RasterizerState.CullNone
                //_lightRasterizerState
                );

            sprite.Draw(spriteBatch);

            spriteBatch.End();
            //foreach (var shadow in shadows)
            //{
            //    for (int i = 0; i < shadow.Points.Count; i++)
            //    {
            //        var a = shadow.Position + shadow.Points[i];
            //        var b = shadow.Position + shadow.Points[(i + 1) % shadow.Points.Count];

            //        var aToB = (b - a) / screenSize;
            //        var packed = PointLight.PackVector2_SNorm(aToB);
            //        spriteBatch.Draw(_rectangle, a, packed);
            //    }
            //}

            //spriteBatch.End();

            spriteBatch.Begin(effect: effect,
                              depthStencilState:  _stencilTest,
                              blendState: BlendState.Additive);
            
            spriteBatch.Draw(NormalBuffer, destination, light.Color);

            spriteBatch.End();
        }
    }

    public void DrawComposite(SpriteBatch spriteBatch, float ambientLight)
    {
        Rectangle viewportBounds = _device.Viewport.Bounds;

        _compositeEffect.AmbientLight = ambientLight;
        _compositeEffect.LightBuffer = LightBuffer;
        _compositeEffect.BoxBlurStride = 0.05f;

        spriteBatch.Begin(effect: _compositeEffect);
        spriteBatch.Draw(ColorBuffer, viewportBounds, Color.White);
        spriteBatch.End();
    }

    public void Finish()
    {

        //Color[] p = new Color[NormalBuffer.Width * NormalBuffer.Height];

        //NormalBuffer.GetData<Color>(p);
        _device.SetRenderTarget(null);
    }
    private readonly Texture2D _rectangle;

    public void DebugDraw(SpriteBatch s)
    {
        var viewportBounds = _device.Viewport.Bounds;

        // the debug view for the color buffer lives in the top-left.
        var colorBorderRect = new Rectangle(
            x: viewportBounds.X,
            y: viewportBounds.Y,
            width: viewportBounds.Width / 2,
            height: viewportBounds.Height / 2);

        // shrink the color rect by 8 pixels
        var colorRect = colorBorderRect;
        colorRect.Inflate(-8, -8);


        // the debug view for the light buffer lives in the top-right.
        var lightBorderRect = new Rectangle(
            x: viewportBounds.Width / 2,
            y: viewportBounds.Y,
            width: viewportBounds.Width / 2,
            height: viewportBounds.Height / 2);

        // shrink the light rect by 8 pixels
        var lightRect = lightBorderRect;
        lightRect.Inflate(-8, -8);

        // the debug view for the normal buffer lives in the top-right.
        var normalBorderRect = new Rectangle(
            x: viewportBounds.X,
            y: viewportBounds.Height / 2,
            width: viewportBounds.Width / 2,
            height: viewportBounds.Height / 2);

        // shrink the normal rect by 8 pixels
        var normalRect = normalBorderRect;
        normalRect.Inflate(-8, -8);


        s.Begin();

        // draw a debug border
        s.Draw(_rectangle, colorBorderRect, Color.MonoGameOrange);

        // draw the color buffer
        s.Draw(ColorBuffer, colorRect, Color.White);

        //draw a debug border
        s.Draw(_rectangle, lightBorderRect, Color.CornflowerBlue);

        // draw the light buffer
        s.Draw(LightBuffer, lightRect, Color.White);


        // draw a debug border
        s.Draw(_rectangle, normalBorderRect, Color.MintCream);

        // draw the normal buffer
        s.Draw(NormalBuffer, normalRect, Color.White);

        s.End();
    }
}

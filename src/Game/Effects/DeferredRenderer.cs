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

        DiffuseBuffer = new RenderTarget2D(device,
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
                                         DepthFormat.None);

        NormalBuffer = new RenderTarget2D(device,
                                          viewportBounds.Width,
                                          viewportBounds.Height,
                                          false,
                                          SurfaceFormat.Color,
                                          DepthFormat.None);

    }

    public RenderTarget2D DiffuseBuffer
    { get; }

    public RenderTarget2D NormalBuffer
    { get; }

    public RenderTarget2D LightBuffer
    { get; }

    public void StartDiffusePhase()
    {
        _device.SetRenderTargets(
            new RenderTargetBinding[]
            {
                new RenderTargetBinding(DiffuseBuffer),
                new RenderTargetBinding(NormalBuffer)
            }
        );
        _device.Clear(Color.Transparent);
    }

    public void StartLightPhase()
    {
        _device.SetRenderTarget(LightBuffer);
        _device.Clear(Color.Black);
    }

    public void DrawComposite(SpriteBatch spriteBatch, float ambientLight)
    {
        Rectangle viewportBounds = _device.Viewport.Bounds;

        _compositeEffect.AmbientLight = ambientLight;
        _compositeEffect.LightBuffer = LightBuffer;

        spriteBatch.Begin(effect: _compositeEffect);
        spriteBatch.Draw(DiffuseBuffer, viewportBounds, Color.White);
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
        s.Draw(DiffuseBuffer, colorRect, Color.White);

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

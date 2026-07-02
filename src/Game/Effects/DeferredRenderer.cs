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

using BadEcho.Game.Lighting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Effects;

/// <summary>
/// Provides a multiphase renderer that breaks up the drawing of sprites into several different phases (color, lighting, etc.),
/// producing a composite image at the end that's drawn to the screen.
/// </summary>
public sealed class DeferredRenderer : IDisposable
{
    private readonly DepthStencilState _stencilWrite =
        new()
        {
            StencilEnable = true,
            // Only fragments that aren't excluded from shadowing (i.e., greater than 0) will be
            // interacted with.
            StencilFunction = CompareFunction.GreaterEqual,
            // Every operation will increase the value up to a maximum of 255.
            StencilPass = StencilOperation.IncrementSaturation,
            ReferenceStencil = 1,
            DepthBufferEnable = false
        };

    private readonly DepthStencilState _stencilTest =
        new()
        {
            StencilEnable = true,
            // The new pixel value needs to be greater or equal to the reference stencil 
            // in order to pass the test.
            StencilFunction = CompareFunction.GreaterEqual,
            ReferenceStencil = 1,
            StencilPass = StencilOperation.Keep,
            DepthBufferEnable = false
        };

    private readonly DepthStencilState _stencilShadowExclude =
        new()
        {
            StencilEnable = true,
            // Pixel is always set to 0, excluding it from shadowing.
            StencilFunction = CompareFunction.Always,
            ReferenceStencil = 0,
            StencilPass = StencilOperation.Replace,
            DepthBufferEnable = false
        };

    private readonly BlendState _shadowBlendState =
        new()
        {   // Shadow related draws write only to the stencil buffer.
            ColorWriteChannels = ColorWriteChannels.None
        };

    private readonly RasterizerState _lightRasterizerState =
        new()
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

    private readonly GraphicsDevice _device;

    private readonly StandardEffect _standardEffect;
    private readonly PointLightEffect _pointLightEffect;
    private readonly ShadowEffect _shadowEffect;
    private readonly CompositeEffect _compositeEffect;

    private readonly RenderTarget2D _colorBuffer;
    private readonly RenderTarget2D _normalBuffer;
    private readonly RenderTarget2D _lightBuffer;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredRenderer"/> class.
    /// </summary>
    /// <param name="device">The graphics device used for rendering.</param>
    public DeferredRenderer(GraphicsDevice device)
    {
        Require.NotNull(device, nameof(device));

        _device = device;
        
        _standardEffect = new StandardEffect(device);
        _pointLightEffect = new PointLightEffect(device);
        _shadowEffect = new ShadowEffect(device);
        _compositeEffect = new CompositeEffect(device);

        Rectangle viewportBounds = device.Viewport.Bounds;

        _colorBuffer = new RenderTarget2D(device,
                                           viewportBounds.Width,
                                           viewportBounds.Height,
                                           false,
                                           SurfaceFormat.Color,
                                           DepthFormat.None);

        _lightBuffer = new RenderTarget2D(device,
                                         viewportBounds.Width,
                                         viewportBounds.Height,
                                         false,
                                         SurfaceFormat.Color,
                                         DepthFormat.Depth24Stencil8);

        _normalBuffer = new RenderTarget2D(device,
                                          viewportBounds.Width,
                                          viewportBounds.Height,
                                          false,
                                          SurfaceFormat.Color,
                                          DepthFormat.None);
    }

    /// <summary>
    /// Starts rendering to the color buffer.
    /// </summary>
    /// <param name="renderStates">Device render states to use when drawing.</param>
    /// <returns>The effect to use when starting a new sprite batch.</returns>
    public StandardEffect StartColorPhase(RenderStates renderStates)
    {
        Require.NotNull(renderStates, nameof(renderStates));

        _device.SetRenderTarget(_colorBuffer);
        _device.Clear(Color.Transparent);

        _standardEffect.NormalBuffer = null;
        _standardEffect.Alpha = renderStates.Alpha ?? 1.0f;
        _standardEffect.MatrixTransform = renderStates.MatrixTransform;

        return _standardEffect;
    }

    /// <summary>
    /// Starts simultaneous rendering to the color and normal buffers.
    /// </summary>
    /// <param name="renderStates">Device render states to use when drawing.</param>
    /// <param name="normalAtlas">The atlas containing normals for the sprites that will be drawn.</param>
    /// <returns>The effect to use when starting a new sprite batch.</returns>
    public StandardEffect StartColorPhase(RenderStates renderStates, Texture2D normalAtlas)
    {
        Require.NotNull(renderStates, nameof(renderStates));

        _device.SetRenderTargets(new RenderTargetBinding(_colorBuffer),
                                 new RenderTargetBinding(_normalBuffer));

        _device.Clear(Color.Transparent);

        _standardEffect.NormalBuffer = normalAtlas;
        _standardEffect.Alpha = renderStates.Alpha ?? 1.0f;
        _standardEffect.MatrixTransform = renderStates.MatrixTransform;

        return _standardEffect;
    }

    /// <summary>
    /// Starts rendering to the normal buffer.
    /// </summary>
    /// <param name="renderStates">Device render states to use when drawing.</param>
    /// <returns>The effect to use when starting a new sprite batch.</returns>
    public StandardEffect StartNormalPhase(RenderStates renderStates)
    {
        Require.NotNull(renderStates, nameof(renderStates));

        _device.SetRenderTarget(_normalBuffer);
        _device.Clear(Color.Transparent);

        _standardEffect.NormalBuffer = null;
        _standardEffect.Alpha = renderStates.Alpha ?? 1.0f;
        _standardEffect.MatrixTransform = renderStates.MatrixTransform;

        return _standardEffect;
    }

    /// <summary>
    /// Draws shadows and lights to the light buffer.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to use to draw the shadows and lights.</param>
    /// <param name="renderStates">Device render states to use when drawing.</param>
    /// <param name="lights">The light sources to render.</param>
    /// <param name="shadowCasters">The sprites to cast shadows when hit by the light sources.</param>
    public void DrawLights(SpriteBatch spriteBatch, RenderStates renderStates, IEnumerable<ILight> lights, IEnumerable<Sprite> shadowCasters)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));
        Require.NotNull(renderStates, nameof(renderStates));
        Require.NotNull(lights, nameof(lights));
        Require.NotNull(shadowCasters, nameof(shadowCasters));

        shadowCasters = shadowCasters.ToList();

        _device.SetRenderTarget(_lightBuffer);
        _device.Clear(Color.Black);

        _pointLightEffect.LightBrightness = 0.8f;
        _pointLightEffect.LightSharpness = 0.1f;

        _pointLightEffect.MatrixTransform = renderStates.MatrixTransform;

        foreach (var light in lights)
        {
            _device.Clear(ClearOptions.Stencil, Color.Black, 0, 1);
            
            _shadowEffect.LightPosition = light.Position;
            _shadowEffect.MatrixTransform = renderStates.MatrixTransform;
            
            spriteBatch.Begin(renderStates with
                              {
                                  BlendState = _shadowBlendState,
                                  DepthStencilState = _stencilShadowExclude
                              });

            foreach (var shadowCaster in shadowCasters)
            {
                shadowCaster.Draw(spriteBatch);
            }

            spriteBatch.End();

            spriteBatch.Begin(depthStencilState: _stencilWrite,
                              effect: _shadowEffect,
                              blendState: _shadowBlendState,
                              rasterizerState: _lightRasterizerState);


            foreach (var shadowCaster in shadowCasters)
            {
                var shadowOrigin = shadowCaster.Bounds.Center.Add(new SizeF(0, shadowCaster.Bounds.Size.Height / 2 - 2.05f));
                var shadowOriginColor = PackShadowOrigin(shadowOrigin);

                shadowCaster.Draw(spriteBatch, shadowOriginColor);
            }

            spriteBatch.End();

            spriteBatch.Begin(effect: _pointLightEffect,
                              depthStencilState:  _stencilTest,
                              blendState: BlendState.Additive);

            light.Draw(spriteBatch, _normalBuffer);

            spriteBatch.End();
        }
    }

    /// <summary>
    /// Draws a composite image comprising the color, light, and other buffers to the screen.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch to use to draw the image.</param>
    /// <param name="ambientLight">The amount of ambient light to apply to the final image.</param>
    public void DrawComposite(SpriteBatch spriteBatch, float ambientLight)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));

        Rectangle viewportBounds = _device.Viewport.Bounds;

        _compositeEffect.AmbientLight = ambientLight;
        _compositeEffect.LightBuffer = _lightBuffer;
        _compositeEffect.BoxBlurStride = 0.05f;

        spriteBatch.Begin(effect: _compositeEffect);
        spriteBatch.Draw(_colorBuffer, viewportBounds, Color.White);
        spriteBatch.End();
    }

    /// <summary>
    /// Finishes the current rendering phase.
    /// </summary>
    public void Finish() 
        => _device.SetRenderTarget(null);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _colorBuffer.Dispose();
        _normalBuffer.Dispose();
        _lightBuffer.Dispose();

        _standardEffect.Dispose();
        _pointLightEffect.Dispose();
        _shadowEffect.Dispose();
        _compositeEffect.Dispose();

        _lightRasterizerState.Dispose();
        _shadowBlendState.Dispose();

        _stencilWrite.Dispose();
        _stencilTest.Dispose();
        _stencilShadowExclude.Dispose();

        _disposed = true;
    }

    private static Color PackShadowOrigin(PointF origin)
    {
        // Pack the X and Y screen coordinates as 16-bit values: R,G encode X and B,A encode Y.
        ushort x = (ushort) Math.Clamp((int) Math.Round(origin.X), 0, 65535);
        ushort y = (ushort) Math.Clamp((int) Math.Round(origin.Y), 0, 65535);

        return new Color((byte) (x >> 8), (byte) (x & 0xFF), (byte) (y >> 8), (byte) (y & 0xFF));
    }
}

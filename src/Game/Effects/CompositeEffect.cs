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

using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Effects;

/// <summary>
/// Provides an effect that will combine various off-screen textures to create the final screen render.
/// </summary>
public sealed class CompositeEffect : Effect
{
    private EffectParameter _ambientLightParam;
    private EffectParameter _lightBufferParam;

    private EffectParameter _screenSizeParam;

    private EffectParameter _boxBlurStrideParam;
    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeEffect"/> class.
    /// </summary>
    /// <param name="device">The graphics device used for rendering.</param>
    public CompositeEffect(GraphicsDevice device)
        : base(device, Shaders.CompositeEffect)
    {
        CacheEffectParameters();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeEffect"/> class.
    /// </summary>
    /// <param name="cloneSource">The <see cref="CompositeEffect"/> instance to clone.</param>
    private CompositeEffect(CompositeEffect cloneSource)
        : base(cloneSource)
    {
        CacheEffectParameters();
    }

    /// <summary>
    /// Gets or sets strength of the blur that affects the number of nearby pixels the filter moves across.
    /// </summary>
    /// <remarks>The blur will affect the compositing of the light buffer, specifically the shadow data found in it.</remarks>
    public float BoxBlurStride
    {
        get => _boxBlurStrideParam.GetValueSingle();
        set => _boxBlurStrideParam.SetValue(value);
    }

    /// <summary>
    /// Gets or sets the general amount of light to apply to the screen.
    /// </summary>
    public float AmbientLight
    {
        get => _ambientLightParam.GetValueSingle();
        set => _ambientLightParam.SetValue(value);
    }

    /// <summary>
    /// Gets or sets the texture light sources are drawn onto.
    /// </summary>
    public Texture2D LightBuffer
    {
        get => _lightBufferParam.GetValueTexture2D();
        set => _lightBufferParam.SetValue(value);
    }

    /// <inheritdoc/>
    public override Effect Clone()
        => new CompositeEffect(this);

    /// <summary>
    /// The size of the screen, used by the shader to calculate the size of a pixel.
    /// </summary>
    private Vector2 ScreenSize
        => _screenSizeParam.GetValueVector2();

    /// <inheritdoc/>
    protected override void OnApply()
    {
        base.OnApply();

        Viewport viewport = GraphicsDevice.Viewport;

        _screenSizeParam.SetValue(new Vector2(viewport.Width, viewport.Height));
    }

    [MemberNotNull(nameof(_ambientLightParam),
                   nameof(_lightBufferParam), 
                   nameof(_screenSizeParam),
                   nameof(_boxBlurStrideParam))]
    private void CacheEffectParameters()
    {
        _ambientLightParam = Parameters[nameof(AmbientLight)];
        _lightBufferParam = Parameters[nameof(LightBuffer)];
        _screenSizeParam = Parameters[nameof(ScreenSize)];
        _boxBlurStrideParam = Parameters[nameof(BoxBlurStride)];
    }
}

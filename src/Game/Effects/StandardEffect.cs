// -----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2025 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Effects;

/// <summary>
/// Provides a <see cref="SpriteBatch"/> effect that can be used in first phase diffuse layer rendering.
/// </summary>
public sealed class StandardEffect : OrthographicEffect
{
    private EffectParameter _alphaParam;
    private EffectParameter _normalBufferParam;

    /// <summary>
    /// Initializes a new instance of the <see cref="StandardEffect"/> class.
    /// </summary>
    /// <param name="device">The graphics device used for sprite rendering.</param>
    public StandardEffect(GraphicsDevice device)
        : base(device, Shaders.StandardEffect)
    {
        CacheEffectParameters();
        
        Alpha = 1f;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StandardEffect"/> class.
    /// </summary>
    /// <param name="cloneSource">The <see cref="StandardEffect"/> instance to clone.</param>
    private StandardEffect(StandardEffect cloneSource)
        : base(cloneSource)
    {
        CacheEffectParameters();
    }

    /// <summary>
    /// Gets or sets the transparency of the material being rendered.
    /// </summary>
    public float Alpha
    {
        get => _alphaParam.GetValueSingle();
        set => _alphaParam.SetValue(value);
    }

    /// <summary>
    /// Gets or sets the texture that contains the normal map of the main, diffuse texture being rendered, which is
    /// used to change how much the lighting affects each pixel depending on the "normal" of the surface at the given pixel.
    /// </summary>
    public Texture2D? NormalBuffer
    {
        get => _normalBufferParam.GetValueTexture2D();
        set => _normalBufferParam.SetValue(value);
    }

    /// <summary>
    /// Creates a clone of the current <see cref="StandardEffect"/> instance.
    /// </summary>
    /// <returns>A cloned <see cref="Effect"/> instance of this.</returns>
    public override Effect Clone()
        => new StandardEffect(this);

    [MemberNotNull(nameof(_alphaParam), nameof(_normalBufferParam))]
    private void CacheEffectParameters()
    {
        _alphaParam = Parameters[nameof(Alpha)];
        _normalBufferParam = Parameters[nameof(NormalBuffer)];
    }
}
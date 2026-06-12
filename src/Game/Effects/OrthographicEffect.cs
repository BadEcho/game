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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.CodeAnalysis;

namespace BadEcho.Game.Effects;

/// <summary>
/// Provides an effect that uses orthographic projection.
/// </summary>
public abstract class OrthographicEffect : Effect
{
    private EffectParameter _matrixParam;
    private Viewport _lastViewport;
    private Matrix? _lastTransform;
    private Matrix _viewProjection;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrthographicEffect"/> class.
    /// </summary>
    /// <param name="device">The graphics device used for sprite rendering.</param>
    /// <param name="effectCode">The compiled shader </param>
    protected OrthographicEffect(GraphicsDevice device, byte[] effectCode)
        : base(device, effectCode)
    {
        CacheEffectParameters();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrthographicEffect"/> class.
    /// </summary>
    /// <param name="cloneSource">The <see cref="Effect"/> instance to clone.</param>
    protected OrthographicEffect(Effect cloneSource)
        : base(cloneSource)
    {
        CacheEffectParameters();
    }

    /// <summary>
    /// Gets or sets an optional matrix to apply to position, rotation, scale, and depth data.
    /// </summary>
    public Matrix? MatrixTransform
    { get; set; }

    /// <inheritdoc/>
    protected override void OnApply()
    {
        base.OnApply();

        // Apply an orthogonal projection akin to how MonoGame converts world-space coords to clip-space for sprite batches.
        Viewport viewport = GraphicsDevice.Viewport;

        bool parametersChanged =
            viewport.Width != _lastViewport.Width || viewport.Height != _lastViewport.Height || _lastTransform != MatrixTransform;

        if (parametersChanged)
        {
            _viewProjection = (MatrixTransform ?? Matrix.Identity)
                .MultiplyBy2DProjection(viewport.Bounds.Size, GraphicsDevice.UseHalfPixelOffset);

            _lastViewport = viewport;
            _lastTransform = MatrixTransform;
        }

        _matrixParam.SetValue(_viewProjection);
    }

    [MemberNotNull(nameof(_matrixParam))]
    private void CacheEffectParameters()
    {
        _matrixParam = Parameters[nameof(MatrixTransform)];
    }
}

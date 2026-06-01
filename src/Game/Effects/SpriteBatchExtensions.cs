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

using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Effects;

/// <summary>
/// Provides a set of static methods intended to aid in matters related to sprite batches.
/// </summary>
public static class SpriteBatchExtensions
{
    /// <summary>
    /// Begins a new sprite and text batch with the specified render states.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch" /> instance to begin the new batch with.</param>
    /// <param name="renderStates">The device render states to use with the new batch.</param>
    public static void Begin(this SpriteBatch spriteBatch, RenderStates renderStates)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));
        Require.NotNull(renderStates, nameof(renderStates));

        spriteBatch.Begin(renderStates.SortMode,
                          renderStates.BlendState,
                          renderStates.SamplerState,
                          renderStates.DepthStencilState,
                          renderStates.RasterizerState,
                          renderStates.Effect,
                          renderStates.MatrixTransform);
    }
}
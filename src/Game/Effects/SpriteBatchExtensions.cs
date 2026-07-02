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
    /// <param name="spriteBatch">A <see cref="SpriteBatch" /> instance.</param>
    extension(SpriteBatch spriteBatch)
    {
        /// <summary>
        /// Begins a new sprite and text batch with the specified render states.
        /// </summary>
        /// <param name="renderStates">The device render states to use with the new batch.</param>
        public void Begin(RenderStates renderStates)
        {
            Require.NotNull(spriteBatch, nameof(spriteBatch));
            Require.NotNull(renderStates, nameof(renderStates));

            var effect = new StandardEffect(spriteBatch.GraphicsDevice)
                         {
                             Alpha = renderStates.Alpha ?? 1.0f,
                             MatrixTransform = renderStates.MatrixTransform
                         };

            spriteBatch.Begin(renderStates.SortMode,
                              renderStates.BlendState,
                              renderStates.SamplerState,
                              renderStates.DepthStencilState,
                              renderStates.RasterizerState,
                              effect,
                              renderStates.MatrixTransform);
        }

        /// <summary>
        /// Begins a new sprite and text batch with the specified render states.
        /// </summary>
        /// <param name="renderStates">The device render states to use with the new batch.</param>
        /// <param name="effect">An effect to override the default sprite effect with.</param>
        public void Begin(RenderStates renderStates, Effect effect)
        {
            Require.NotNull(spriteBatch, nameof(spriteBatch));
            Require.NotNull(renderStates, nameof(renderStates));
            Require.NotNull(effect, nameof(effect));

            spriteBatch.Begin(renderStates.SortMode,
                              renderStates.BlendState,
                              renderStates.SamplerState,
                              renderStates.DepthStencilState,
                              renderStates.RasterizerState,
                              effect,
                              renderStates.MatrixTransform);
        }
    }
}
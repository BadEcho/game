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

namespace BadEcho.Game.Effects;

/// <summary>
/// Provides device render states to use when drawing with a new sprite batch.
/// </summary>
/// <param name="SortMode">The drawing order for sprite and text drawing.</param>
/// <param name="BlendState">State of the blending.</param>
/// <param name="SamplerState">State of the sampler.</param>
/// <param name="RasterizerState">State of the rasterization.</param>
/// <param name="Effect">An effect to override the default sprite effect with.</param>
/// <param name="MatrixTransform">A matrix used to transform the sprite geometry.</param>
/// <param name="DepthStencilState">State of the depth-stencil buffer.</param>
public sealed record RenderStates(SpriteSortMode SortMode,
                                  BlendState? BlendState = null,
                                  SamplerState? SamplerState = null,
                                  RasterizerState? RasterizerState = null,
                                  StandardEffect? Effect = null,
                                  Matrix? MatrixTransform = null,
                                  DepthStencilState? DepthStencilState = null);
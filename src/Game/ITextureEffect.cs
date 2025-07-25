﻿// -----------------------------------------------------------------------
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

using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game;

/// <summary>
/// Defines shaders that support mapping a texture onto vertex data.
/// </summary>
public interface ITextureEffect : IStandardEffect
{
    /// <summary>
    /// Gets or sets the texture to be applied by this effect.
    /// </summary>
    Texture2D Texture { get; set; }
}

﻿//-----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2023 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Xna.Framework.Content;

namespace BadEcho.Game.Fonts;

/// <summary>
/// Provides a reader of raw multi-channel signed distance field font content from the content pipeline.
/// </summary>
public sealed class DistanceFieldFontReader : ContentTypeReader<DistanceFieldFont>
{
    /// <inheritdoc/>
    protected override DistanceFieldFont Read(ContentReader input, DistanceFieldFont existingInstance)
        => throw new NotImplementedException();
}

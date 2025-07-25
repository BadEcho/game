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

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game;

/// <summary>
/// Provides a reader of raw sprite sheet content from the content pipeline.
/// </summary>
public sealed class SpriteSheetReader : ContentTypeReader<SpriteSheet>
{
    /// <inheritdoc />
    protected override SpriteSheet Read(ContentReader input, SpriteSheet existingInstance)
    {
        Require.NotNull(input, nameof(input));

        var texture = input.ReadExternalReference<Texture2D>();
        var columnCount = input.ReadInt32();
        var rowCount = input.ReadInt32();
        var initialFrame = input.ReadInt32();

        var spriteSheet = new SpriteSheet(texture, columnCount, rowCount);

        spriteSheet.AddAnimation(
            new SpriteAnimationSequence(string.Empty, initialFrame, initialFrame, TimeSpan.Zero));

        var animationsToRead = input.ReadInt32();

        while (animationsToRead > 0)
        {
            var name = input.ReadString();
            var startFrame = input.ReadInt32();
            var endFrame = input.ReadInt32();
            var duration = input.ReadObject<TimeSpan>();
            var animation = new SpriteAnimationSequence(name, startFrame, endFrame, duration);

            spriteSheet.AddAnimation(animation);
            animationsToRead--;
        }

        return spriteSheet;
    }
}

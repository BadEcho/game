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

namespace BadEcho.Game;

/// <summary>
/// Provides an adaptive connection to a render-target surface with its output scaled to fit the surface while also
/// maintaining a specific aspect ratio.
/// </summary>
public sealed class AspectMaintainedViewportConnector : ViewportConnector
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AspectMaintainedViewportConnector"/> class.
    /// </summary>
    /// <param name="device">The graphics device to use for drawing onto the render-target surface.</param>
    /// <param name="virtualSize">The base resolution of content drawn onto the surface.</param>
    public AspectMaintainedViewportConnector(GraphicsDevice device, Size virtualSize) 
        : base(device, virtualSize)
    { }

    /// <inheritdoc/>
    public override Viewport Viewport
    {
        get
        {
            int bufferWidth = Device.PresentationParameters.BackBufferWidth;
            int bufferHeight = Device.PresentationParameters.BackBufferHeight;

            float aspectRatio = (float) VirtualSize.Width / VirtualSize.Height;
            // Give the width of our back buffer, we'll calculate what the height needs to be in order for our
            // aspect ratio to stay the same.
            int width = bufferWidth;
            int height = (int) (width / aspectRatio +
                                0.5f); // We add 0.5 to facilitate rounding, as the int cast will truncate.

            // If the height exceeds the available height of the back buffer, we'll have to work with the width instead.
            if (height > bufferHeight)
            {
                height = bufferHeight;
                width = (int) (height * aspectRatio + 0.5f);
            }

            // We then calculate offsets for our viewport such that we're centered on our scaled content.
            int x = (bufferWidth - width) / 2;
            int y = (bufferHeight - height) / 2;

            return new Viewport(x, y, width, height);
        }
    }
}

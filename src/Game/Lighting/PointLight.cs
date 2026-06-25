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

using BadEcho.Game.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Lighting;

/// <summary>
/// Provides a light source that emits light from a point. 
/// </summary>
public sealed class PointLight
{
    private readonly GraphicsDevice _device;
    private readonly Texture2D _pixel;

    public PointLight(GraphicsDevice device)
    {
        _device = device;
        Rectangle bounds = device.Viewport.Bounds;

        ShadowBuffer = new RenderTarget2D(device,
                                          bounds.Width,
                                          bounds.Height,
                                          false,
                                          SurfaceFormat.Color,
                                          DepthFormat.None,
                                          0,
                                          RenderTargetUsage.PreserveContents);

        _pixel = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
        _pixel.SetData([Color.White]);
    }

    /// <summary>
    /// Gets or sets the position of the point light.
    /// </summary>
    public Vector2 Position
    { get; set; }

    /// <summary>
    /// Gets or sets the color of the light.
    /// </summary>
    public Color Color
    { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the radius of the light.
    /// </summary>
    public int Radius
    { get; set; } = 250;

    public RenderTarget2D ShadowBuffer
    { get; set; }

    //public void DrawShadows(SpriteBatch spriteBatch, IEnumerable<ShadowCaster> shadows)//, ShadowEffect shadow)
    //{
    //    _device.SetRenderTarget(ShadowBuffer);
    //    _device.Clear(Color.White);

    //    shadow.LightPosition = Position;
    //    var screenSize = new Vector2(ShadowBuffer.Width, ShadowBuffer.Height);

    //    spriteBatch.Begin(effect: shadow, rasterizerState: RasterizerState.CullNone);

    //    foreach (var shadow in shadows)
    //    {
    //        for (int i = 0; i < shadow.Points.Count; i++)
    //        {
    //            var a = shadow.Position + shadow.Points[i];
    //            var b = shadow.Position + shadow.Points[(i + 1) % shadow.Points.Count];

    //            //var positionA = shadow.LineSegmentA;
    //            var aToB = (b - a) / screenSize;
    //            var packed = PackVector2_SNorm(aToB);

    //            spriteBatch.Draw(_pixel, a, packed);
    //        }
    //    }

    //    spriteBatch.End();
    //}

    /// <summary>
    /// Draws the light using the provided active sprite batch against the provided normal buffer.
    /// </summary>
    /// <param name="spriteBatch">An active sprite batch.</param>
    /// <param name="normalBuffer">A buffer containing normals to draw the light against.</param>
    public void Draw(SpriteBatch spriteBatch, Texture2D normalBuffer)
    {
        int diameter = Radius * 2;

        var destination = new Rectangle((int) (Position.X - Radius), (int) (Position.Y - Radius), diameter, diameter);

        spriteBatch.Draw(normalBuffer, destination, Color);
    }
}

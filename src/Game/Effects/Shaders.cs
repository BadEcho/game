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

using BadEcho.Extensions;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;
using System.Resources;
using BadEcho.Game.Properties;

namespace BadEcho.Game.Effects;

/// <summary>
/// Provides access to shader effect resources.
/// </summary>
public static class Shaders
{
#if OPENGL
    private const string PLATFORM_EXTENSION = "ogl";
#else
    private const string PLATFORM_EXTENSION = "dx11";
#endif

    private static readonly ResourceManager _Manager = new("BadEcho.Game.Effects.Shaders",
                                                           typeof(Shaders).Assembly);
    /// <summary>
    /// Gets the data for a <see cref="SpriteBatch"/> effect that can be used in first phase diffuse layer rendering.
    /// </summary>
    public static byte[] StandardEffect
        => GetStreamBytes($"{nameof(StandardEffect)}.{PLATFORM_EXTENSION}");

    /// <summary>
    /// Gets the data for an effect required to correctly render multi-channel signed distance field font text.
    /// </summary>
    public static byte[] DistanceFieldFontEffect
        => GetStreamBytes($"{nameof(DistanceFieldFontEffect)}.{PLATFORM_EXTENSION}");

    /// <summary>
    /// Gets the data for an effect that emits illumination from a point light.
    /// </summary>
    public static byte[] PointLightEffect
        => GetStreamBytes($"{nameof(PointLightEffect)}.{PLATFORM_EXTENSION}");
    
    /// <summary>
    /// Gets the data for an effect that combines various off-screen textures to create the final screen render.
    /// </summary>
    public static byte[] CompositeEffect
        => GetStreamBytes($"{nameof(CompositeEffect)}.{PLATFORM_EXTENSION}");

    /// <summary>
    /// Gets the data for an effect that casts a shadow hull.
    /// </summary>
    public static byte[] ShadowHullEffect
        => GetStreamBytes($"{nameof(ShadowHullEffect)}.{PLATFORM_EXTENSION}");

    private static byte[] GetStreamBytes(string name)
    {
        UnmanagedMemoryStream stream
            = _Manager.GetStream(name, CultureInfo.InvariantCulture)
              ?? throw new BadImageFormatException(Strings.EffectMissingResource.InvariantFormat(name));

        return stream.ToArray();
    }
}
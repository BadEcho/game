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

namespace BadEcho.Game.Pipeline.Areas;

/// <summary>
/// Provides the raw data for an area asset.
/// </summary>
public sealed class AreaContent : ContentItem<AreaAsset>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AreaContent"/> class.
    /// </summary>
    /// <param name="asset">The configuration data for the area.</param>
    public AreaContent(AreaAsset asset) 
        : base(asset)
    {  }
}

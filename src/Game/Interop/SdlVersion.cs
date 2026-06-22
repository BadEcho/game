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

namespace BadEcho.Game.Interop;

/// <summary>
/// Represents information about the version of SDL in use.
/// </summary>
/// <suppressions>
/// ReSharper disable InconsistentNaming
/// </suppressions>
internal struct SdlVersion
{
    /// <summary>
    /// The major version.
    /// </summary>
    public byte major;
    /// <summary>
    /// The minor version.
    /// </summary>
    public byte minor;
    /// <summary>
    /// The patch/update version.
    /// </summary>
    public byte patch;
}

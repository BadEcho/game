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

using BadEcho.Game.Scenes;
using BadEcho.Game.UI;

namespace BadEcho.Game.Tests;

internal sealed class TestScreenScene : ScreenScene
{
    public TestScreenScene(Microsoft.Xna.Framework.Game game) 
        : base(game)
    { }

    protected override IPanel LoadControls(SceneManager manager)
        => new StackPanel();
}

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

namespace BadEcho.Game.UI;

/// <summary>
/// Provides a base class for a layout parent control, responsible for positioning child controls on a rendering surface.
/// </summary>
public abstract class Panel<TSelf> : Control<TSelf>, IPanel
    where TSelf : Panel<TSelf>
{
    private readonly List<IControl> _children = [];

    /// <inheritdoc/>
    public override IInputHandler? InputHandler
    {
        get => base.InputHandler;
        set
        {
            if (InputHandler != value) 
                _children.ForEach(c => c.InputHandler = value);

            base.InputHandler = value;
        }
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<IControl> Children
        => _children;

    /// <inheritdoc/>
    public void AddChild<T>(T child)
        where T : Control<T>
    {
        Require.NotNull(child, nameof(child));

        _children.Add(child);

        child.Parent = this;
        child.InputHandler = InputHandler;
        InvalidateMeasure();
    }

    /// <inheritdoc/>
    public void RemoveChild<T>(T child)
        where T : Control<T>
    {
        Require.NotNull(child, nameof(child));

        if (!_children.Remove(child))
            return;
        
        if (InputHandler is { FocusedElement: IArrangeable focusedElement })
        {
            IArrangeable? current = focusedElement;

            while (current != null)
            {
                if (ReferenceEquals(current, child))
                {
                    InputHandler.ClearFocus();
                    break;
                }

                current = (current as IControl)?.Parent;
            }
        }

        child.Parent = null;
        child.InputHandler = null;
        InvalidateMeasure();
    }

    /// <inheritdoc/>
    public override void UpdateInput()
    {
        base.UpdateInput();

        IEnumerable<IControl> activeChildren = Children.Where(c => c.IsEnabled);

        foreach (var activeChild in activeChildren)
        {
            activeChild.UpdateInput();
        }
    }

    /// <inheritdoc/>
    protected override void DrawCore(SpriteBatch spriteBatch)
    {
        foreach (IControl control in Children.Where(c => c.IsVisible))
        {
            control.Draw(spriteBatch);
        }
    }

    /// <inheritdoc/>
    protected override void DrawPrimitivesCore(Matrix? transform, float alpha)
    {
        foreach (IControl control in Children.Where(c => c.IsVisible))
        {
            control.DrawPrimitives(transform, alpha);
        }
    }
}

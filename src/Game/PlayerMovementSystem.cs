// -----------------------------------------------------------------------
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BadEcho.Game;

/// <summary>
/// Provides a system for the player to exert control over an entity's movement.
/// </summary>
public sealed class PlayerMovementSystem : Component
{
    // TODO: These will be set via a hot pluggable config module in the future. This entire class will have a major refactor.
    private const Keys MOVEMENT_LEFT = Keys.A;
    private const Keys MOVEMENT_UP = Keys.W;
    private const Keys MOVEMENT_RIGHT = Keys.D;
    private const Keys MOVEMENT_DOWN = Keys.S;
    // TODO: Will be configurable based on entity.
    private const float VELOCITY_MAX = 120f;
    private const float VELOCITY_MIN = -120f;
    private const float ACCELERATION = 5;
    private const float DECELERATION = 15f;

    /// <inheritdoc/>
    public override bool Update(IEntity entity, GameUpdateTime time)
    {
        Require.NotNull(entity, nameof(entity));
        
        var deltaTime = (float) time.ElapsedGameTime.TotalSeconds;

        float xTarget = CalculateTargetVelocity(MOVEMENT_RIGHT, MOVEMENT_LEFT);
        float yTarget= CalculateTargetVelocity(MOVEMENT_DOWN, MOVEMENT_UP);

        Vector2 updatedVelocity = new Vector2(xTarget, yTarget);
        
        float change = ACCELERATION;
        
        if (updatedVelocity == Vector2.Zero)
            change = DECELERATION;
        
        updatedVelocity = Vector2.Lerp(entity.Velocity, updatedVelocity, change * deltaTime);
        
        if (updatedVelocity.LengthSquared() - 0.5f < 0)
            updatedVelocity = Vector2.Zero;

        entity.Velocity = updatedVelocity;

        return true;
    }
    
    private static float CalculateTargetVelocity(Keys positiveDirectionKey, Keys negativeDirectionKey)
    {
        KeyboardState keyboardState = Keyboard.GetState();

        bool positiveMovement = keyboardState.IsKeyDown(positiveDirectionKey);
        bool negativeMovement = keyboardState.IsKeyDown(negativeDirectionKey);

        if (!positiveMovement && !negativeMovement)
            return 0;

        var targetVelocity = positiveMovement ? VELOCITY_MAX : VELOCITY_MIN;

        return targetVelocity;
    }
}

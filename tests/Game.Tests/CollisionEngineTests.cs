// -----------------------------------------------------------------------
// <copyright>
//		Created by Matt Weber <matt@badecho.com>
//		Copyright @ 2026 Bad Echo LLC. All rights reserved.
//
//		Bad Echo Technologies are licensed under a
//		GNU Affero General Public License v3.0.
//
//		See accompanying file LICENSE.md or a copy at:
//		https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

using Xunit;

namespace BadEcho.Game.Tests;

public class CollisionEngineTests
{
    [Fact]
    public void Update_ColliderMovesAcrossQuadrants_DoesNotCollideWithItself()
    {
        var engine = new CollisionEngine(new RectangleF(0, 0, 1000, 1000));

        // Enough stationary colliders to force the collision tree to split.
        for (int i = 0; i < 40; i++)
        {
            engine.Register(new ColliderStub(new RectangleF(600 + i * 8, 900, 4, 4)));
        }

        var mover = new ColliderStub(new RectangleF(480, 10, 16, 16));
        engine.Register(mover);

        // Straddle the vertical split line between the top-left and top-right quadrants.
        mover.Area = new RectangleF(495, 10, 16, 16);
        mover.IsDirty = true;
        engine.Update();

        // Move fully into the top-right quadrant.
        mover.Area = new RectangleF(520, 10, 16, 16);
        engine.Update();

        Assert.Empty(mover.Collisions);
    }

    private sealed class ColliderStub : Collider
    {
        public ColliderStub(RectangleF area)
        {
            Area = area;
        }

        public RectangleF Area
        { get; set; }

        public List<Collider> Collisions
        { get; } = [];

        public override IShape Bounds
            => Area;

        public override bool ResolveCollision(Collider collision)
        {
            Collisions.Add(collision);

            return true;
        }
    }
}
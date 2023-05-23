﻿//-----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2023 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game;

/// <summary>
/// Provides an emitter of particles, used to reproduce the visuals exhibited by highly chaotic systems, natural phenomena, or processes
/// caused by chemical reactions.
/// </summary>
public sealed class ParticleSystem
{
    private readonly List<Particle> _particles
        = new();

    private readonly Random _random
        = new();

    private readonly List<Texture2D> _textures;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystem"/> class.
    /// </summary>
    /// <param name="textures">A sequence of the textures that particles generated by this system will use.</param>
    /// <param name="emitterLocation">The location from where particles generated by the system are emitted.</param>
    public ParticleSystem(IEnumerable<Texture2D> textures, Vector2 emitterLocation)
    {
        Require.NotNull(textures, nameof(textures));

        _textures = textures.ToList();

        EmitterLocation = emitterLocation;
    }

    /// <summary>
    /// Gets the location from where particles generated by the system are emitted.
    /// </summary>
    public Vector2 EmitterLocation
    { get; set; }

    /// <summary>
    /// Gets or sets the size of the particle batches generated during the system's <see cref="Update"/> ticks.
    /// </summary>
    public int BatchSize
    { get; set; } = 10;

    /// <summary>
    /// Gets the lower boundary of generated particle lifetimes in ticks.
    /// </summary>
    public int MinimumTimeToLive
    { get; set; } = 20;

    /// <summary>
    /// Gets the upper boundary of generated particle lifetimes in ticks.
    /// </summary>
    public int MaximumTimeToLive
    { get; set; } = 40;

    /// <summary>
    /// Generates a new batch of particles and updates the lifetimes of those already existing.
    /// </summary>
    /// <param name="time">The game timing configuration and state for this update.</param>
    public void Update(GameUpdateTime time)
    {
        for (int i = 0; i < BatchSize; i++)
        {
            _particles.Add(GenerateParticle());
        }

        for (int i = 0; i < _particles.Count; i++)
        {
            _particles[i].Update(time);

            if (_particles[i].TimeToLive <= 0)
            {
                _particles.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// Draws generated particles to the screen.
    /// </summary>
    /// <param name="spriteBatch">The <see cref="SpriteBatch"/> instance to use to draw the particles.</param>
    public void Draw(SpriteBatch spriteBatch)
    {
        Require.NotNull(spriteBatch, nameof(spriteBatch));

        spriteBatch.Begin();

        foreach (var particle in _particles)
        {
            particle.Draw(spriteBatch);
        }

        spriteBatch.End();
    }

    private Particle GenerateParticle()
    {
        var texture = _textures[_random.Next(_textures.Count)];

        var velocity = new Vector2(1f * (_random.NextSingle() * 2 - 1),
                                   1f * (_random.NextSingle() * 2 - 1));

        float angularVelocity = 0.1f * (_random.NextSingle() * 2 - 1);

        var sprite
            = new Sprite(texture)
              {
                  Position = EmitterLocation,
                  Velocity = velocity,
                  AngularVelocity = angularVelocity
              };

        var color = new Color(_random.NextSingle(),
                              _random.NextSingle(),
                              _random.NextSingle());

        float size = _random.NextSingle();
        int timeToLive = MinimumTimeToLive + _random.Next(MaximumTimeToLive);

        return new Particle(sprite, color, size, timeToLive);
    }
}

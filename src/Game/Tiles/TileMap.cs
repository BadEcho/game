﻿// -----------------------------------------------------------------------
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

using BadEcho.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BadEcho.Game.Tiles;

/// <summary>
/// Provides a piece of the game's world, or map of a level, built out of rectangular-shaped tile images, among
/// other kinds of content.
/// </summary>
/// <remarks>
/// Tile maps supported by the Bad Echo Game Framework are based on the TMX map format, used by various popular tile map editors such
/// as Tiled.
/// </remarks>
public sealed class TileMap : Extensible, IModelRenderer
{
    private readonly Dictionary<Layer, IEnumerable<IPrimitiveModel>> _layerModelMap = [];
    private readonly Dictionary<TileSet, int> _tileSetFirstIdMap = [];
    private readonly List<AnimatedTileModelData> _animatedTiles = [];
    private readonly List<TileSet> _tileSets = [];
    private readonly List<Layer> _layers = [];
    private readonly GraphicsDevice _device;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TileMap"/> class.
    /// </summary>
    /// <param name="device">The graphics device to use when rendering the map's models.</param>
    /// <param name="name">The name of the tile map.</param>
    /// <param name="size">The tile map's size, measured in tiles.</param>
    /// <param name="tileSize">The size of the tiles used in the tile map.</param>
    /// <param name="customProperties">The tile map's custom properties.</param>
    public TileMap(GraphicsDevice device,
                   string name,
                   Size size,
                   Size tileSize,
                   CustomProperties customProperties)
        : base(customProperties)
    {
        Require.NotNull(device, nameof(device));

        _device = device;

        Name = name;
        Size = size;
        TileSize = tileSize;
    }

    /// <summary>
    /// Gets the name of this tile map.
    /// </summary>
    /// <remarks>
    /// The TMX map format lacks a name field for a particular tile map; the name used here is essentially based on its
    /// identifying name as a game asset (i.e., the name of the file containing the map data).
    /// </remarks>
    public string Name
    { get; }

    /// <summary>
    /// Gets this tile map's size, measured in tiles.
    /// </summary>
    /// <remarks>
    /// The dimensions of the tile map is determined using tiles (whose sizes are known) as opposed to simply pixels. A 64x64 size,
    /// for example, indicates a map that is a 64 tiles wide by 64 tiles hide, with a total area of 4,096 tiles.
    /// </remarks>
    public SizeF Size
    { get; }

    /// <summary>
    /// Gets the size of the tiles used in this map.
    /// </summary>
    /// <remarks>
    /// Maps defined using the TMX map format use a uniform size for all the tiles that they comprise. The tile size defined for the
    /// map essentially, then, defines the size of tiles used by all its constituent layers.
    /// </remarks>
    public Size TileSize
    { get; }

    /// <summary>
    /// Gets the orientation of this tile map.
    /// </summary>
    public MapOrientation Orientation
    { get; init; }

    /// <summary>
    /// Gets the order in which tiles on this map are rendered.
    /// </summary>
    public TileRenderOrder RenderOrder
    { get; init; }

    /// <summary>
    /// Gets the background color of the tile map.
    /// </summary>
    public Color BackgroundColor
    { get; init; } = Color.Transparent;

    /// <summary>
    /// Gets the collection of tile sets this map sources its tile images from.
    /// </summary>
    public IReadOnlyCollection<TileSet> TileSets
        => _tileSets;

    /// <summary>
    /// Gets the collection of map layers that compose this tile map.
    /// </summary>
    public IReadOnlyCollection<Layer> Layers
        => _layers;

    /// <summary>
    /// Adds a tile set to this map so its tile data may be sourced when drawing the individual tiles appearing on this map.
    /// </summary>
    /// <param name="tileSet">The tile set to add to this map.</param>
    /// <param name="firstId">
    /// The starting value to use when mapping global identifiers mapped to the tiles in this tile set.
    /// </param>
    public void AddTileSet(TileSet tileSet, int firstId)
    {
        Require.NotNull(tileSet, nameof(tileSet));

        _tileSets.Add(tileSet);
        _tileSetFirstIdMap.Add(tileSet, firstId);
    }
    
    /// <summary>
    /// Adds a layer to this map so that its content is rendered when this map is drawn.
    /// </summary>
    /// <param name="layer">The layer to add to this map.</param>
    public void AddLayer(Layer layer)
    {
        Require.NotNull(layer, nameof(layer));

        _layers.Add(layer);
    }

    /// <summary>
    /// Advances the state of any animated tiles on this tile map.
    /// </summary>
    /// <param name="time">The game timing configuration and state for this update.</param>
    public void Update(GameUpdateTime time)
    {
        foreach (AnimatedTileModelData tileData in _animatedTiles)
        {
            tileData.Update(time);
        }

        foreach (var layer in Layers)
        {
            foreach (var animatedModel in _layerModelMap[layer].OfType<DynamicModel>())
            {
                animatedModel.UpdateVertices();
            }
        }
    }

    /// <inheritdoc/>
    public void Draw()
        => Draw(Matrix.Identity);

    /// <inheritdoc/>
    public void Draw(Matrix view)
        => Draw(view, 1.0f);

    /// <inheritdoc/>
    public void Draw(Matrix view, float alpha)
    {
        if (_layerModelMap.Count == 0)
            CreateModels();

        var projection = Matrix.Identity.MultiplyBy2DProjection(_device.Viewport.Bounds.Size);

        foreach (var layer in Layers)
        {
            Matrix world = Matrix.Identity;

            world.Translation = new Vector3(layer.Offset, 0);

            var effect = new BasicEffect(_device)
                         {
                             World = world,
                             View = view,
                             Projection = projection,
                             Alpha = alpha
                         };

            foreach (var layerModel in _layerModelMap[layer])
            {
                layerModel.Draw(effect);
            }
        }
    }

    /// <summary>
    /// Generates all the models required to render this tile map when it is being submitted for drawing.
    /// </summary>
    public void CreateModels()
    {
        ClearModels();

        foreach (var layer in _layers)
        {
            switch (layer)
            {
                case ImageLayer imageLayer:
                    CreateLayerModels(imageLayer);
                    break;

                case TileLayer tileLayer:
                    CreateLayerModels(tileLayer);
                    break;
            }
        }
    }

    /// <summary>
    /// Converts this tile map into a sequence of space-occupying colliders for every tile belonging to a tile layer
    /// marked as collidable.
    /// </summary>
    /// <returns>
    /// A sequence of <see cref="Collider"/> instances for every tile in this map that belongs to a tile layer with
    /// the <see cref="KnownProperties.Collidable"/> custom property set to <c>true</c>.
    /// </returns>
    public IEnumerable<Collider> ToCollidableMap()
        => Layers.OfType<TileLayer>()
                 .Where(l => l.CustomProperties.Booleans
                              .TryGetValue(KnownProperties.Collidable, out bool collidable) && collidable)
                 .SelectMany(l => l.ToCollidableLayer());

    private void ClearModels()
    {
        IEnumerable<IPrimitiveModel> models = _layerModelMap.SelectMany(lm => lm.Value);

        foreach (var model in models)
        {
            model.Dispose();
        }

        _layerModelMap.Clear();
    }

    private void CreateLayerModels(ImageLayer layer)
    {
        var imageData = new QuadTextureModelData();

        imageData.AddTexture(layer.Image.Bounds, layer.Image.Bounds, layer.Position);

        _layerModelMap.Add(layer,
                           new StaticModel(_device, layer.Image, imageData).AsEnumerable());
    }

    private void CreateLayerModels(TileLayer layer)
    {
        var layerModels = new List<IPrimitiveModel>();
        
        // Layer tile data must be processed tile set by tile set, as they provide the actual textures that individual
        // tiles are sourced from.
        foreach (TileSet tileSet in TileSets)
        {
            int firstId = _tileSetFirstIdMap[tileSet];
            
            // Extract the tiles in the layer we're adding that belong to the current tile set. Even if the tile set was
            // designed to be based on multiple images, our content pipeline extension will have generated a packed texture
            // comprising all tile-specific texture data. This allows us to render all the tile set's tiles with as little
            // as one model. We only need to differentiate between animated and non-animated tiles, since they use different
            // modeling data.
            var tilesByIsAnimated = layer.GetRange(firstId, tileSet.LastId + firstId)
                                         .GroupBy(t => tileSet.IsTileAnimated(t.Id - firstId));
                                      
            foreach (var tilesAndIsAnimated in tilesByIsAnimated)
            {
                IEnumerable<IPrimitiveModel> tileModels = tilesAndIsAnimated.Key
                    ? CreateAnimatedTileModels(tileSet, tilesAndIsAnimated)
                    : CreateTileModels(tileSet, tilesAndIsAnimated);

                layerModels.AddRange(tileModels);
            }
        }

        _layerModelMap.Add(layer, layerModels);
    }

    private List<IPrimitiveModel> CreateTileModels(TileSet tileSet, IEnumerable<Tile> tiles)
    {
        var tileModels = new List<IPrimitiveModel>();
        var tileData = new QuadTextureModelData();
        int firstId = _tileSetFirstIdMap[tileSet];
        Texture2D texture = tileSet.Texture;

        foreach (Tile tile in tiles)
        {   
            int localId = tile.Id - firstId;
            Vector2 position = GetTilePosition(tile);
            Rectangle sourceArea = tileSet.GetTileSourceArea(localId);
            
            tileData.AddTexture(texture.Bounds, sourceArea, position);

            if (tileData.VertexCount + 4 > ushort.MaxValue)
            {
                tileModels.Add(new StaticModel(_device, texture, tileData));
                    
                tileData = new QuadTextureModelData();
            }
        }

        if (tileData.VertexCount > 0)
            tileModels.Add(new StaticModel(_device, texture, tileData));

        return tileModels;
    }

    private List<IPrimitiveModel> CreateAnimatedTileModels(TileSet tileSet, IEnumerable<Tile> tiles)
    {
        var tileModels = new List<IPrimitiveModel>();
        var tileData = new AnimatedTileModelData();
        int firstId = _tileSetFirstIdMap[tileSet];
        Texture2D texture = tileSet.Texture;

        foreach (Tile tile in tiles)
        {
            int localId = tile.Id - firstId;
            Vector2 position = GetTilePosition(tile);
            Rectangle sourceArea = tileSet.GetTileSourceArea(localId);
            
            tileData.AddTexture(texture.Bounds, sourceArea, position);
            tileData.Animations.Add(new TileAnimation(tileSet, localId));

            if (tileData.VertexCount + 4 > ushort.MaxValue)
            {
                tileModels.Add(new DynamicModel(_device, texture, tileData));
                _animatedTiles.Add(tileData);

                tileData = new AnimatedTileModelData();
            }
        }

        if (tileData.VertexCount > 0)
        {
            tileModels.Add(new DynamicModel(_device, texture, tileData));
            _animatedTiles.Add(tileData);
        }

        return tileModels;
    }

    private Vector2 GetTilePosition(Tile tile)
    {
        return Orientation switch
        {
            MapOrientation.Orthogonal => new Vector2(tile.Column * TileSize.Width, tile.Row * TileSize.Height),
            // TODO: Add support for other orientations.
            _ => throw new NotSupportedException()
        };
    }
}

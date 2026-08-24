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

using BadEcho.Extensions;
using BadEcho.Game.Pipeline.Properties;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace BadEcho.Game.Pipeline;

/// <summary>
/// Provides typed raw data for a game asset.
/// </summary>
/// <typeparam name="T">The type of asset data described by the content.</typeparam>
public abstract class ContentItem<T> : ContentItem, IContentItem
{
    private readonly Dictionary<string, Reference> _references = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentItem{T}"/> class.
    /// </summary>
    /// <param name="asset">The configuration data for the game asset.</param>
    protected ContentItem(T asset)
        => Asset = asset;

    /// <summary>
    /// Gets the configuration data for the game asset.
    /// </summary>
    public T Asset 
    { get; }

    /// <inheritdoc/>
    public void AddReference<TContent>(ContentProcessorContext context, 
                                       string sourcePath, 
                                       OpaqueDataDictionary processorParameters)
    {
        AddReference<TContent>(context, sourcePath, processorParameters, string.Empty);
    }

    /// <inheritdoc/>
    public void AddReference<TContent>(ContentProcessorContext context, 
                                       string sourcePath, 
                                       OpaqueDataDictionary processorParameters,
                                       string outputPath)
    {
        Require.NotNull(context, nameof(context));
        Require.NotNull(sourcePath, nameof(sourcePath));
        Require.NotNull(processorParameters, nameof(processorParameters));

        if (_references.TryGetValue(NormalizeReferenceKey(sourcePath), out Reference? existingReference))
        {   // Referencing the same asset more than once (such as two actors sharing a sprite sheet)
            // is normal and fine; referencing it with different build settings is not.
            bool conflictsWithReference =
                existingReference.ContentType != typeof(TContent)
                || !string.Equals(existingReference.OutputPath, outputPath, StringComparison.OrdinalIgnoreCase)
                || !AreParametersEquivalent(existingReference.ProcessorParameters, processorParameters);

            if (conflictsWithReference)
                throw new PipelineException(Strings.ConflictingReferenceInContentItem.InvariantFormat(sourcePath));

            return;
        }

        var sourceAsset = new ExternalReference<TContent>(sourcePath);

#pragma warning disable CS0618
        // I want to let the new content builder pipeline to mature a bit. Changing this to call the new, non-obsolete method
        // results in the originally obsolete method being called under the hood anyway. Not interested at the moment.
        var reference =
            context.BuildAsset<TContent, TContent>(sourceAsset,
                                                   string.Empty,
                                                   processorParameters,
                                                   string.Empty,
                                                   outputPath);

        _references.Add(NormalizeReferenceKey(sourcePath),
                        new Reference(typeof(TContent), processorParameters, outputPath, reference));
    }

    /// <inheritdoc/>
    public ExternalReference<TContent> GetReference<TContent>(string filename)
    {
        Require.NotNull(filename, nameof(filename));

        if (!_references.TryGetValue(NormalizeReferenceKey(filename), out Reference? reference))
            throw new ArgumentException(Strings.NoReferenceInContentItem.InvariantFormat(filename), nameof(filename));

        return (ExternalReference<TContent>) reference.ContentItem;
    }

    private static string NormalizeReferenceKey(string path)
    {   // Separators are unified and the comparer is case-insensitive because the same physical file is reachable by
        // more than one spelling of its MGCB-relative path, and each spelling has to land on the same reference. Only
        // the key is normalized; the path handed to the content build stays exactly as it was authored.
        return path.Replace('\\', '/');
    }

    private static bool AreParametersEquivalent(OpaqueDataDictionary first, OpaqueDataDictionary second)
    {
        if (first.Count != second.Count)
            return false;

        foreach (KeyValuePair<string, object> parameter in first)
        {
            if (!second.TryGetValue(parameter.Key, out object? value) || !Equals(parameter.Value, value))
                return false;
        }

        return true;
    }

    private sealed record Reference(
        Type ContentType,
        OpaqueDataDictionary ProcessorParameters,
        string OutputPath,
        ContentItem ContentItem);
}

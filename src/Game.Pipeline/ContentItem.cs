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

using System.Reflection;
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
                                       IContentProcessor processor)
    {
        AddReference<TContent>(context, sourcePath, processor, string.Empty);
    }

    /// <inheritdoc/>
    public void AddReference<TContent>(ContentProcessorContext context, 
                                       string sourcePath,
                                       IContentProcessor processor,
                                       string outputPath)
    {
        Require.NotNull(context, nameof(context));
        Require.NotNull(sourcePath, nameof(sourcePath));
        Require.NotNull(processor, nameof(processor));

        Dictionary<string, object?> processorParameters = ReadParameters(processor);

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

        string sourceAssetExtension = Path.GetExtension(sourcePath);

        IContentImporter importer = this.LoadImporter(sourceAssetExtension);

        var sourceAsset = new ExternalReference<TContent>(sourcePath);
        var reference =
            context.BuildAsset<TContent, TContent>(sourceAsset,
                                                   // Passing null here is entirely valid when working with the new
                                                   // content builder projects, as doing so will cause it to fall back to
                                                   // the default importer for the file type.
                                                   // Unfortunately, when working with the legacy pipeline (which has its own
                                                   // ContentProcessorContext implementation), passing null here will lead to
                                                   // an exception...and because calling the overload that accepts an importer
                                                   // name is obsolete (and will cause errors with content builder projects),
                                                   // we annoyingly need to supply an importer here.
                                                   importer,
                                                   processor,
                                                   outputPath);

        // There's currently a bug with the new content builder projects. If an absolute path is specified
        // for the asset being built, the content builder will normalize the filename's path separators
        // to forward slashes (if, and only if, outputPath has a value). Unfortunately, this runs head-on
        // into sketchy validation logic in the content writer, which checks whether the reference's filename
        // is within the root directory (and it does this with a literal string.StartsWith() comparison
        // ...not exactly a kosher way to compare two paths).

        // We can't simply force all references to use backslashes as path separators, as that will cause errors
        // if we're using the legacy pipeline instead of a new content builder project.
        // So, the "hack" to support both of these paths is to defer to using the same separators as
        // the context's output directory. A content builder project's context will use backslashes, and the legacy
        // pipeline's context will use forward slashes.
        reference.Filename = context.OutputDirectory.Contains('\\', StringComparison.OrdinalIgnoreCase)
            ? reference.Filename.Replace('/', '\\')
            : reference.Filename.Replace('\\', '/');
        
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

    private static Dictionary<string, object?> ReadParameters(IContentProcessor processor)
    {
        Dictionary<string, object?> parameters = [];
        PropertyInfo[] properties = processor.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

        foreach (var property in properties.Where(p => p is { CanRead: true, CanWrite: true }))
        {
            parameters.Add(property.Name, property.GetValue(processor));
        }

        return parameters;
    }

    private static bool AreParametersEquivalent(Dictionary<string, object?> first, Dictionary<string, object?> second)
    {
        if (first.Count != second.Count)
            return false;

        foreach (KeyValuePair<string, object?> parameter in first)
        {
            if (!second.TryGetValue(parameter.Key, out object? value) || !Equals(parameter.Value, value))
                return false;
        }

        return true;
    }

    private sealed record Reference(
        Type ContentType,
        Dictionary<string, object?> ProcessorParameters,
        string OutputPath,
        ContentItem ContentItem);
}
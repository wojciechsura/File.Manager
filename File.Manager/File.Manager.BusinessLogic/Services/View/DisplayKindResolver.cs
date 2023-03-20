using Dev.Editor.BusinessLogic.Services.Highlighting;
using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.View
{
    public class DisplayKindResolver : IDisplayKindResolver
    {
        private readonly Dictionary<string, DisplayKind> knownResolutions = new();

        public DisplayKindResolver(IHighlightingProvider highlightingProvider)
        {
            // Register known text types

            foreach (string extension in highlightingProvider.HighlightingDefinitions
                .SelectMany(h => h.Extensions))
                knownResolutions.Add(extension.ToLowerInvariant(), DisplayKind.Text);
        }

        public DisplayKind? Resolve(string filename)
        {
            var extension = System.IO.Path.GetExtension(filename).ToLowerInvariant();

            if (knownResolutions.TryGetValue(extension, out DisplayKind result))
                return result;

            return null;
        }
    }
}

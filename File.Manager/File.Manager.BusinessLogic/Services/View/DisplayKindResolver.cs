using Dev.Editor.BusinessLogic.Services.Highlighting;
using File.Manager.BusinessLogic.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.View
{
    public class DisplayKindResolver : IDisplayKindResolver
    {
        private const int BYTES_WITHOUT_NULL_COUNT = 8000;
        private const byte NULL = 0;

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

        public (DisplayKind displayKind, Stream newStream) ResolveManually(Stream inputStream)
        {
            var ms = new MemoryStream();

            byte[] buffer = new byte[BYTES_WITHOUT_NULL_COUNT];
            int bytesRead;
            do
            {
                bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                ms.Write(buffer, 0, bytesRead);
            }
            while (bytesRead > 0);

            try
            {
                ms.Seek(0, SeekOrigin.Begin);
                bytesRead = ms.Read(buffer, 0, BYTES_WITHOUT_NULL_COUNT);

                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == NULL)
                        return (DisplayKind.Hex, ms);
                }

                return (DisplayKind.Text, ms);
            }
            finally
            {
                ms.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}

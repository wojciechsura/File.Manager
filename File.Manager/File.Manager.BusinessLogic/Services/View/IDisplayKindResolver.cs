using File.Manager.BusinessLogic.Types;
using System.IO;

namespace File.Manager.BusinessLogic.Services.View
{
    public interface IDisplayKindResolver
    {
        DisplayKind? Resolve(string filename);
        (DisplayKind displayKind, Stream newStream) ResolveManually(Stream inputStream);
    }
}
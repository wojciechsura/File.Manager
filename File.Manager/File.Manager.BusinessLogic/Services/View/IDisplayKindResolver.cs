using File.Manager.BusinessLogic.Types;

namespace File.Manager.BusinessLogic.Services.View
{
    public interface IDisplayKindResolver
    {
        DisplayKind? Resolve(string filename);
    }
}
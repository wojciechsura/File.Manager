using File.Manager.BusinessLogic.Models.Highlighting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dev.Editor.BusinessLogic.Services.Highlighting
{
    public interface IHighlightingProvider
    {
        HighlightingInfo GetDefinitionByExtension(string extension);
        HighlightingInfo GetDefinitionByName(string value);
        IReadOnlyList<HighlightingInfo> HighlightingDefinitions { get; }
        HighlightingInfo EmptyHighlighting { get; }
    }
}

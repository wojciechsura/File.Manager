using File.Manager.BusinessLogic.Types;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace File.Manager.BusinessLogic.Models.Highlighting
{
    public class HighlightingInfo
    {
        public HighlightingInfo(string name,
            IHighlightingDefinition definition,
            FoldingKind foldingKind,
            string[] extensions,
            bool hidden)
        {
            Name = name;
            Definition = definition;
            FoldingKind = foldingKind;
            Extensions = extensions;
            Hidden = hidden;
        }

        public string Name { get; }

        public IHighlightingDefinition Definition { get; }

        public FoldingKind FoldingKind { get; }

        public bool Hidden { get; }

        public IEnumerable<string> Extensions { get; }

        public string Initial => Name.Substring(0, 1);
    }
}

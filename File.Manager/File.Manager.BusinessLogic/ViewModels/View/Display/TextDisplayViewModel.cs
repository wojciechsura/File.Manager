using Dev.Editor.BusinessLogic.Services.Highlighting;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.View.Display
{
    public class TextDisplayViewModel : BaseDisplayViewModel
    {
        private readonly TextDocument document;

        public TextDisplayViewModel(IDisplayHost host, 
            IHighlightingProvider highlightingProvider, 
            Stream stream, 
            string filename)
            : base(host, filename)
        {
            using (var reader = new StreamReader(stream))
                document = new TextDocument(reader.ReadToEnd());

            var extension = System.IO.Path.GetExtension(filename).ToLowerInvariant();

            var highlightingDefinition = highlightingProvider.GetDefinitionByExtension(extension);
            if (highlightingDefinition != null)
                this.Highlighting = highlightingDefinition.Definition;
        }

        public TextDocument Document => document;
        public IHighlightingDefinition Highlighting { get; }
    }
}

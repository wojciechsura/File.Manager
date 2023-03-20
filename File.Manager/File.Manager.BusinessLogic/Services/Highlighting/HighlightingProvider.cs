using File.Manager.BusinessLogic.Models.Highlighting;
using File.Manager.BusinessLogic.Types;
using File.Manager.Resources.Services.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Dev.Editor.BusinessLogic.Services.Highlighting
{
    class HighlightingProvider : IHighlightingProvider, IHighlightingDefinitionReferenceResolver
    {
        // Private fields -----------------------------------------------------

        private const string ResourcePrefix = "File.Manager.BusinessLogic.Resources.Highlighting.";

        private readonly List<HighlightingInfo> allHighlightingInfos = new List<HighlightingInfo>();
        private readonly List<HighlightingInfo> visibleHighlightingInfos = new List<HighlightingInfo>();
        private readonly Dictionary<string, HighlightingInfo> highlightingsByExt = new Dictionary<string, HighlightingInfo>(StringComparer.OrdinalIgnoreCase);

        private HighlightingInfo emptyHighlighting;

        // Private methods ----------------------------------------------------

        private Stream OpenResourceStream(string resource)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourcePrefix + resource);
        }

        private IHighlightingDefinition LoadDefinition(string resourceName)
        {
            XshdSyntaxDefinition xshd;
            using (Stream s = OpenResourceStream(resourceName))
            using (XmlTextReader reader = new XmlTextReader(s))
            {
                xshd = HighlightingLoader.LoadXshd(reader);
            }

            return HighlightingLoader.Load(xshd, this);
        }

        private void RegisterHighlighting(string name, 
            string[] extensions, 
            string resourceName, 
            FoldingKind foldingKind,
            bool hidden = false)
        {
            var info = new HighlightingInfo(name, LoadDefinition(resourceName), foldingKind, extensions, hidden);
            allHighlightingInfos.Add(info);

            if (extensions != null)
                foreach (var ext in extensions)
                    highlightingsByExt.Add(ext, info);
        }

        private void InitializeHighlightings()
        {
            // Special: empty

            emptyHighlighting = new HighlightingInfo(Strings.SyntaxHighlighting_None, 
                null, 
                FoldingKind.None, 
                Array.Empty<string>(), 
                false);
            allHighlightingInfos.Add(emptyHighlighting);

            // Register internal highlightings

            RegisterHighlighting("XmlDoc",
                null,
                "XmlDoc.xshd",
                FoldingKind.None,
                true);

            RegisterHighlighting("CommentMarkers",
                null,
                "CommentMarkers.xshd",
                FoldingKind.None,
                true);

            // Register regular highlightings

            RegisterHighlighting("BinaryDefinition",
                new[] { ".bindef" },
                "BinDef.xshd",
                FoldingKind.Braces);

            RegisterHighlighting("C#",
                new[] { ".cs" },
                "CSharp.xshd",
                FoldingKind.Braces);

            RegisterHighlighting("JavaScript",
                new[] { ".js" },
                "JavaScript.xshd",
                FoldingKind.Braces);

            RegisterHighlighting("Json",
                new[] { ".json" },
                "json.xshd",
                FoldingKind.Braces,
                false);

            RegisterHighlighting("CSS",
                new[] { ".css" },
                "CSS.xshd",
                FoldingKind.Braces);

            RegisterHighlighting("HTML",
                new[] { ".htm", ".html" },
                "HTML.xshd",
                FoldingKind.None);

            RegisterHighlighting("ASP",
                new[] { ".asp", ".aspx", ".asax", ".asmx", ".ascx", ".master" },
                "ASPX.xshd",
                FoldingKind.None);

            RegisterHighlighting("C++",
                new[] { ".c", ".h", ".cc", ".cpp", ".hpp", ".ino" },
                "CPP.xshd",
                FoldingKind.Braces);

            RegisterHighlighting("Ini",
                new[] { ".ini" },
                "INI.xshd",
                FoldingKind.None);

            RegisterHighlighting("Java",
                new[] { ".java" },
                "Java.xshd",
                FoldingKind.Braces);

            RegisterHighlighting("Patch",
                new[] { ".patch", ".diff" },
                "Patch.xshd",
                FoldingKind.None);

            RegisterHighlighting("PowerShell",
                new[] { ".ps1", ".psm1", ".psd1" },
                "PowerShell.xshd",
                FoldingKind.Braces);

            RegisterHighlighting("PHP",
                new[] { ".php" },
                "PHP.xshd",
                FoldingKind.Braces);

            RegisterHighlighting("Python",
                new[] { ".py", ".pyw" },
                "Python.xshd",
                FoldingKind.None);

            RegisterHighlighting("TSQL",
                new[] { ".sql" },
                "TSQL.xshd",
                FoldingKind.None);

            RegisterHighlighting("VB",
                new[] { ".vb" },
                "VB-Mode.xshd",
                FoldingKind.None);

            RegisterHighlighting("XML",
                new[] { ".xml", ".xsl", ".xslt", ".xsd", ".manifest", ".config", ".addin", ".xshd", ".wxs", ".wxi", ".wxl", ".proj", ".csproj", ".vbproj", ".ilproj", ".booproj", ".build", ".xfrm", ".targets", ".xaml", ".xpt", ".xft", ".map", ".wsdl", ".disco", ".ps1xml", ".nuspec" },
                "XML-Mode.xshd",
                FoldingKind.Xml,
                false);

            RegisterHighlighting("MarkDown",
                new[] { ".md" },
                "MarkDown.xshd",
                FoldingKind.None,
                false);

            RegisterHighlighting("Feature (Gherkin)",
                new[] { ".feature" },
                "feature.xshd",
                FoldingKind.None,
                false);

            // Sort highlightings
            allHighlightingInfos.Sort((i1, i2) => i1.Name.CompareTo(i2.Name));

            // Pick visible highlightings
            visibleHighlightingInfos.AddRange(allHighlightingInfos.Where(hi => !hi.Hidden));
        }

        // IHighlightingDefinitionReferenceResolver implementation ------------

        IHighlightingDefinition IHighlightingDefinitionReferenceResolver.GetDefinition(string name)
        {
            return allHighlightingInfos.First(hi => string.Equals(hi.Definition?.Name, name))
                .Definition;
        }

        // Public methods -----------------------------------------------------

        public HighlightingProvider()
        {
            InitializeHighlightings();            
        }

        public HighlightingInfo GetDefinitionByExtension(string extension)
        {
            if (highlightingsByExt.ContainsKey(extension))
                return highlightingsByExt[extension];

            return emptyHighlighting;
        }

        public HighlightingInfo GetDefinitionByName(string name)
        {
            return allHighlightingInfos
                .FirstOrDefault(hi => String.Equals(hi.Name, name))
                ?? emptyHighlighting;
        }

        // Public properties --------------------------------------------------

        public IReadOnlyList<HighlightingInfo> HighlightingDefinitions => visibleHighlightingInfos;

        public HighlightingInfo EmptyHighlighting => emptyHighlighting;
    }
}

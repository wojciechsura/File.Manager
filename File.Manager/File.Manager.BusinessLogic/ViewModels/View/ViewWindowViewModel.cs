using Dev.Editor.BusinessLogic.Services.Highlighting;
using File.Manager.BusinessLogic.Services.View;
using File.Manager.BusinessLogic.Types;
using File.Manager.BusinessLogic.ViewModels.Base;
using File.Manager.BusinessLogic.ViewModels.View.Display;
using Spooksoft.VisualStateManager.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace File.Manager.BusinessLogic.ViewModels.View
{
    public class ViewWindowViewModel : BaseViewModel, IDisplayHost
    {
        private readonly IViewWindowAccess access;
        private readonly IDisplayKindResolver displayKindResolver;
        private readonly IHighlightingProvider highlightingProvider;

        private BaseDisplayViewModel display;

        private void DoClose()
        {
            access.Close();
        }

        private BaseDisplayViewModel BuildDisplay(Stream stream, string filename)
        {
            DisplayKind? resolved = displayKindResolver.Resolve(filename);

            if (resolved == null)
                (resolved, stream) = displayKindResolver.ResolveManually(stream);

            return resolved switch
            {
                DisplayKind.Text => new TextDisplayViewModel(this, highlightingProvider, stream, filename),
                DisplayKind.Hex => new HexDisplayViewModel(this, stream, filename),
                _ => throw new InvalidOperationException("Unsupported display kind!")
            };
        }

        public ViewWindowViewModel(IViewWindowAccess access, 
            IDisplayKindResolver displayKindResolver,
            IHighlightingProvider highlightingProvider,
            Stream stream, 
            string filename)
        {
            this.access = access;
            this.displayKindResolver = displayKindResolver;
            this.highlightingProvider = highlightingProvider;

            display = BuildDisplay(stream, filename);

            CloseCommand = new AppCommand(obj => DoClose());
        }

        public ICommand CloseCommand { get; }

        public BaseDisplayViewModel Display => display;
    }
}

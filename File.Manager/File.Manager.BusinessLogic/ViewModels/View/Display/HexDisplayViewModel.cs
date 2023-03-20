using Spooksoft.HexEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.ViewModels.View.Display
{
    public class HexDisplayViewModel : BaseDisplayViewModel
    {
        private readonly HexByteContainer document;

        public HexDisplayViewModel(IDisplayHost host, Stream stream, string filename) 
            : base(host, filename)
        {
            document = new HexByteContainer(stream);
        }

        public HexByteContainer Document => document;
    }
}

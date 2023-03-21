using File.Manager.BusinessLogic.Models.Configuration.Session;
using Spooksoft.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Configuration
{
    public class ConfigModel : ConfigRoot
    {
        internal const string NAME = "FileManager";

        public ConfigModel()
            : base(NAME)
        {
            Session = new SessionConfig(this);
        }

        public SessionConfig Session { get; }
    }
}

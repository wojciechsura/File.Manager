using Spooksoft.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Configuration.Session
{
    public class SessionConfig : ConfigItem
    {        
        internal const string NAME = "Session";

        private readonly ConfigValue<string> leftPaneAddress;
        private readonly ConfigValue<string> rightPaneAddress;

        public SessionConfig(BaseItemContainer parent)
            : base(NAME, parent)
        {
            leftPaneAddress = new ConfigValue<string>("LeftPaneAddress", this, null);
            rightPaneAddress = new ConfigValue<string>("RightPaneAddress", this, null);
        }

        public ConfigValue<string> LeftPaneAddress => leftPaneAddress;
        public ConfigValue<string> RightPaneAddress => rightPaneAddress;
    }
}

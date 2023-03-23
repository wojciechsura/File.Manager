using Spooksoft.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Configuration.Ftp
{
    public class FtpSession : BaseCollectionItem
    {
        internal const string NAME = "Session";

        private readonly ConfigValue<string> sessionName;
        private readonly ConfigValue<string> host;
        private readonly ConfigValue<int> port;
        private readonly ConfigValue<string> username;

        public FtpSession() 
            : base(NAME)
        {
            sessionName = new ConfigValue<string>("Name", this, null);
            host = new ConfigValue<string>("Host", this, null);
            port = new ConfigValue<int>("Port", this, 21);
            username = new ConfigValue<string>("Username", this, null);
        }

        public ConfigValue<string> SessionName => sessionName;
        public ConfigValue<string> Host => host;
        public ConfigValue<int> Port => port;
        public ConfigValue<string> Username => username;
    }
}

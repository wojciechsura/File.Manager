using Spooksoft.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Configuration.Ftp
{
    public class FtpConfig : ConfigItem
    {
        internal const string NAME = "FtpConfig";

        private readonly FtpSessionCollection sessions;

        public FtpConfig(BaseItemContainer parent) 
            : base(NAME, parent)
        {
            sessions = new FtpSessionCollection(this);
        }

        public FtpSessionCollection Sessions => sessions;
    }
}

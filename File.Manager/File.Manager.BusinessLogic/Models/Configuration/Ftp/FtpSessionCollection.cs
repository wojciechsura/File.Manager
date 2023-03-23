using Spooksoft.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Configuration.Ftp
{
    public class FtpSessionCollection : BaseTypedItemCollection<FtpSession>
    {
        internal const string NAME = "Sessions";

        private readonly List<BaseChildInfo> childInfos;

        public FtpSessionCollection(BaseItemContainer parent) : base(NAME, parent)
        {
            childInfos = new List<BaseChildInfo>()
            {
                new ChildInfo<FtpSession>(FtpSession.NAME, () => new FtpSession())
            };
        }

        protected override IEnumerable<BaseChildInfo> ChildInfos => childInfos;
    }
}

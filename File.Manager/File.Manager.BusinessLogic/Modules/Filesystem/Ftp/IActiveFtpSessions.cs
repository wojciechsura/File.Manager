using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Ftp
{
    public interface IActiveFtpSessions
    {
        Dictionary<(string host, string username), ActiveFtpSession> ActiveSessions { get; }
    }
}

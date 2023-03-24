using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Dialogs.FtpCredentials
{
    public class FtpCredentialsModel
    {
        public FtpCredentialsModel(string username, SecureString password) 
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }
        public SecureString Password { get; }
    }
}

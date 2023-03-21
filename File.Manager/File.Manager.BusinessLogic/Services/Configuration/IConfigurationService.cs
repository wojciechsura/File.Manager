using File.Manager.BusinessLogic.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Services.Configuration
{
    public interface IConfigurationService
    {
        ConfigModel Configuration { get; }

        bool Save();
    }
}

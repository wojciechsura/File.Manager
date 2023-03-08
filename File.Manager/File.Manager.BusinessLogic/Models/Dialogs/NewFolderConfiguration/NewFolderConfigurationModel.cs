using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Models.Dialogs.NewFolderConfiguration
{
    public class NewFolderConfigurationModel
    {
        public NewFolderConfigurationModel(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}

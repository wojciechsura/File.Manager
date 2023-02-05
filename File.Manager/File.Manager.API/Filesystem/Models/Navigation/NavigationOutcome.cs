using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Navigation
{
    public class NavigationOutcome
    {
        private protected NavigationOutcome() 
        { 
        
        }

        public static NavigationSuccess NavigationSuccess() => new NavigationSuccess();
        public static NavigationError NavigationError(string message) => new NavigationError(message); 
    }
}

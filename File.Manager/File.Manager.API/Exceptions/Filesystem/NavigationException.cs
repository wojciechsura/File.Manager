using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Exceptions.Filesystem
{

	[Serializable]
	public class NavigationException : Exception
	{
		public NavigationException() 
		{ 
		
		}

		public NavigationException(string message) : base(message) 
		{ 

		}

		public NavigationException(string message, Exception inner) : base(message, inner) 
		{ 

		}

		protected NavigationException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) 
		{

		}
	}
}

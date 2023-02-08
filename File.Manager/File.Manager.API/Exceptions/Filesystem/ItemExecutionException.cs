using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Exceptions.Filesystem
{

	[Serializable]
	public class ItemExecutionException : Exception
	{
        protected ItemExecutionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) 
		{ 
		
		}

        public ItemExecutionException() 
		{ 
		
		}

		public ItemExecutionException(string message) : base(message) 
		{
		
		}

		public ItemExecutionException(string message, Exception inner) : base(message, inner) 
		{ 
		
		}
	}
}

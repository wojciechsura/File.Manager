using File.Manager.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.Types
{
    public class PixelPoint : BaseIntPoint<PixelPoint>
    {
        public PixelPoint(int x, int y) 
            : base(x, y)
        {

        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.API.Filesystem.Models.Plan
{
    public class PlanFolder : BasePlanItem, IReadOnlyList<BasePlanItem>
    {
        private readonly List<BasePlanItem> items;

        public PlanFolder(string name, List<BasePlanItem> items) 
            : base(name)
        {
            this.items = items;
        }

        public IEnumerator<BasePlanItem> GetEnumerator()
        {
            return ((IEnumerable<BasePlanItem>)items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }

        public int Count => items.Count;

        public BasePlanItem this[int index] => items[index];
    }
}

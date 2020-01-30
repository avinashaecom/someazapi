using System;
using System.Collections.Generic;

namespace AECOM.GNGApi.Models
{
    public partial class GNGStatus
    {
        public GNGStatus()
        {
            GoNoGo = new HashSet<GoNoGo>();
        }

        public int GngstatusId { get; set; }
        public string GngstatusValue { get; set; }

        public virtual ICollection<GoNoGo> GoNoGo { get; set; }
    }
}

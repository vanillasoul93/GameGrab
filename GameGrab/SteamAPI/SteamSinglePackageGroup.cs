using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameGrab.SteamAPI
{
    public class SteamSinglePackageGroup
    {
        public string name { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string selection_text { get; set; }
        public string save_text { get; set; }
        public int display_type { get; set; }
        public string is_recurring_subscription { get; set; }
        public List<SteamSingleSub> subs { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameGrab.SteamAPI
{
    public class SteamAppChunk
    {
        public int appid { get; set; }
        public string name { get; set; }
        public int last_modified { get; set; }
        public int price_change_number { get; set; }
    }
}

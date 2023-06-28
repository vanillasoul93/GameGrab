using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameGrab.SteamAPI
{
    public class SteamAppChunkResponse
    {
        public List<SteamAppChunk> apps { get; set; }
        public bool have_more_results { get; set; }
        public int last_appid { get; set; }
    }
}

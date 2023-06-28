using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameGrab.SteamAPI
{
    public class SteamUserGameList
    {
        public int game_count { get; set; }
        public List<SteamUserGame> games { get; set; }
    }
}

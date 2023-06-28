using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace GameGrab
{
    public class Game
    {
        
        public List<GameCategory> Categories { get; set; } = new List<GameCategory>();
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public int steamAppId { get; set; }
        public bool CustomGame { get; set; } = false;
    }
}

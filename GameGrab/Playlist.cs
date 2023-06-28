using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GameGrab
{
    public class Playlist
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string PlayListImagePath { get; set; }
        public List<Game> Games { get; set; } = new List<Game>();
        public DateTime CreatedDate { get; set; }
        public BitmapImage tempImage1 { get; set; }
        public BitmapImage tempImage2 { get; set;}
        public BitmapImage tempImage3 { get; set;}
        public BitmapImage tempImage4 { get; set;}
    }
}

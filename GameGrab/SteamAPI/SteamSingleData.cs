using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameGrab.SteamAPI
{
    public class SteamSingleData
    {
        public string type { get; set; }
        public string name { get; set; }
        public int steam_appid { get; set; }
        public int required_age { get; set; }
        public bool is_free { get; set; }
        public string detailed_description { get; set; }
        public string about_the_game { get; set; }
        public string short_description { get; set; }
        public string supported_languages { get; set; }
        public string header_image { get; set; }
        public string capsule_image { get; set; }
        public string capsule_imagev5 { get; set; }
        public string website { get; set; }
        //public string pc_requirements { get; set; }
        //public string mac_requirements { get; set; }
        //public string linux_requirements { get; set; }
        public string[] developers { get; set; }
        public string[] publishers { get; set; }
        public SteamSinglePriceOverview price_overview { get; set; }
        public int[] packages { get; set; }
        public List<SteamSinglePackageGroup> package_groups { get; set; }
        public SteamSinglePlatforms platforms { get; set; }
        //public List<SteamSingleCategory> categories { get; set; }
        //public List<SteamSingleGenre> genres { get; set; }
        public List<SteamSingleScreenshot> screenshots { get; set; }
        public SteamSingleRecommendations recommendations { get; set; }
        public SteamSingleReleaseDate release_date { get; set; }
        public SteamSingleSupportInfo support_info { get; set; }
        public string background { get; set; }
        public string background_raw { get; set; }
        public SteamSingleContentDescriptors content_descriptors { get; set; }
    }
}

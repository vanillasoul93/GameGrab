using GameGrab.Converters;
using GameGrab.SteamAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static System.Net.WebRequestMethods;

namespace GameGrab
{
    public class SteamApiClient
    {
        private readonly string steamKey = "73EB69EABF3D223A7792EEB1340AD05A";
        //https://api.steampowered.com/ISteamApps/GetAppList/v2/
        private const string PassiveAggressiveUserAgent = "This is definitely a user agent, just so you know.";
        public SteamAppList GetAllSteamGames()
        {
            SteamAppList appList = new SteamAppList();
            SteamAppRoot appRoot = JsonConvert.DeserializeObject<SteamAppRoot>(GetResponseAsync($"https://api.steampowered.com/ISteamApps/GetAppList/v2/?key={steamKey}"));
            appList = appRoot.applist;
            return appList;
        }

        public SteamAppChunkResponse GetSteamAppChunk(int lastAppId)
        {
            SteamAppChunkResponse steamAppChunk = new SteamAppChunkResponse();
            SteamAppChunkRoot chunkRoot = JsonConvert.DeserializeObject<SteamAppChunkRoot>(GetResponseAsync($"https://api.steampowered.com/IStoreService/GetAppList/v1/?key={steamKey}&include_games=true&include_dlc=false&last_appid={lastAppId}&max_results=15000"));
            steamAppChunk = chunkRoot.response;
            return steamAppChunk;
        }

        public SteamUserGameList GetUsersGames(string steamID)
        {
            SteamUserGameList usersGames = new SteamUserGameList();
            SteamUserGameListRoot appRoot = JsonConvert.DeserializeObject<SteamUserGameListRoot>
                
                (GetResponseAsync($"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={steamKey}&steamid={steamID}&include_appinfo=true&include_played_free_games=true&skip_unvetted_apps=true&include_extended_appinfo=true"));
            usersGames = appRoot.response;
            return usersGames;
            //http://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={steamKey}&steamid={steamID}&format=json
        }

        public SteamSingle GetSteamGameFromID(string appID)
        {
            Console.WriteLine("PULLING GAME INFO WITH APP ID: " + appID);
            var settings = new JsonSerializerSettings
            {
                // Pass true if you want single-item lists to be reserialized as single items
                Converters = { new SingleOrArrayListConverter(true) },
            };
            var dict = JsonConvert.DeserializeObject<Dictionary<string, SteamSingle>>(GetResponseAsync($"https://store.steampowered.com/api/appdetails?appids={appID}"), settings);
            //SteamSingleRoot appRoot = JsonConvert.DeserializeObject<SteamSingleRoot>
            //    (GetResponseAsync($"https://store.steampowered.com/api/appdetails?appids={appID}"));
            SteamSingle steamSingleData = dict[appID.ToString()];

            return steamSingleData;
        }

        public SteamUserResponse GetUserDetailsFromSteamID(string steamID)
        {
            SteamUserRoot appRoot = JsonConvert.DeserializeObject<SteamUserRoot>
                (GetResponseAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={steamKey}&steamids={steamID}&format=json"));
            SteamUserResponse response = appRoot.response;
            return response;
        }

        public string GetResponseAsync(string url)
        {
            WebRequest request = WebRequest.Create(url);
            request.Headers.Add("User_Agent", PassiveAggressiveUserAgent);
            using (var response = request.GetResponse())
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                string result = reader.ReadToEnd();
                if (result.Length > 0)
                {
                    Console.WriteLine(result);
                    return result;
                }
                else
                {
                    return null;
                }
            }
             
        }

    }
}


using GameGrab.SteamAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.WebRequestMethods;

namespace GameGrab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Game> gameVault = new ObservableCollection<Game>();
        public Game selectedVaultGame = new Game();
        public SteamApp selectedSteamGame = new SteamApp();
        public SteamAppList appList = new SteamAppList();
        public ObservableCollection<SteamAppChunk> steamGames = new ObservableCollection<SteamAppChunk>();
        public ObservableCollection<SteamApp> apps = new ObservableCollection<SteamApp>();

        public string currentSteamUserID = string.Empty;
        public bool UserImportSuccess = false;
        public bool userStoppedImport = false;
        public BackgroundWorker worker = new BackgroundWorker();
        public BackgroundWorker steamAppWorker = new BackgroundWorker();
        public ObservableCollection<Playlist> playlists = new ObservableCollection<Playlist>();
        public Regex englishCheck = new Regex("^[a-zA-Z0-9. -_?]*$");

        public MainWindow()
        {
            InitializeComponent();
            ListViewSteamGames.ItemsSource = steamGames;
            ListViewVaultGames.ItemsSource = gameVault;



            //Console.WriteLine("\n\nSTEAM APPS FOUND:\n\n");
            //foreach (SteamApp app in appList.apps)
            //{
            //    Console.WriteLine(app.name + "\n\n");
            //} 

            //ListViewSteamGames.ItemsSource = null;
            //ListViewSteamGames.Items.Clear();

            //ListViewSteamGames.ItemsSource = null;
            //ListViewSteamGames.Items.Clear();

            //IEnumerable<SteamApp> selectedGames = appList.apps.Where(x => x.name == "inputName");
            //if(selectedGames != null)
            //{
            //}
            //else
            //{
            //    //Game was not found
            //}

            Import_Steam_Games_Worker_StartWork();
        }

        private void ButtonGameVault_Click(object sender, RoutedEventArgs e)
        {
            var selectedButton = ButtonGameVault;
            var selectedGridPanel = GridGameVault;
            PanelToggle(selectedGridPanel);
            ButtonToggle(selectedButton);

        }

        private void ButtonSearchVaultGames_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Setting List View Source to Apps");
            //ListViewSG.ItemsSource = apps;
            Console.WriteLine("List View Source Successfully Changed");
            //GridImportSteamId.Visibility = Visibility.Visible;


        }

        private void ButtonSearchSteamGames_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxSearchSteamGames.Text == "" || TextBoxSearchSteamGames.Text.Length < 3)
            {
                Console.WriteLine("No Search Input\n\n");

                ListViewSteamGames.ItemsSource = null;
                ListViewSteamGames.Items.Clear();
                ListViewSteamGames.ItemsSource = steamGames;
                if (ListViewSteamGames.Items.Count > 0)
                {
                    ListViewSteamGames.SelectedIndex = 0;
                    ListViewSteamGames.ScrollIntoView(ListViewSteamGames.Items[0]);
                }
            }
            else
            {
                ObservableCollection<SteamAppChunk> appSearch = new ObservableCollection<SteamAppChunk>(steamGames.Where(x => x.name.ToLower().Contains(TextBoxSearchSteamGames.Text.ToLower())));
                Console.WriteLine("Apps Found With Search: " + TextBoxSearchSteamGames.Text + "\n\n");

                ListViewSteamGames.ItemsSource = null;
                ListViewSteamGames.Items.Clear();
                ListViewSteamGames.ItemsSource = appSearch;
                if (ListViewSteamGames.Items.Count > 0)
                {
                    ListViewSteamGames.SelectedIndex = 0;
                    ListViewSteamGames.ScrollIntoView(ListViewSteamGames.Items[0]);
                }
            }

        }

        private void ListViewSteamGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListViewSteamGames.SelectedIndex >= 0)
            {
                selectedSteamGame = ListViewSteamGames.SelectedItem as SteamApp;
                if (selectedSteamGame != null)
                {
                Console.WriteLine(selectedSteamGame.name);
                }
                
            }
            else
            {
                //Console.WriteLine("Invalid Selection");
            }

        }

        public void Import_Steam_User_Worker_StartWork()
        {
            UserImportSuccess = false;
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += Import_Steam_User_Worker_DoWork;
            worker.RunWorkerCompleted += Import_Steam_User_Worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
            Console.WriteLine("STARTUP", "Importing user from steam Start");
            gameVault.Clear();
            GridImportSteamId.Visibility = Visibility.Visible;
        }



        private void Import_Steam_User_Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
                Console.WriteLine("Finished Populating Game Vault");
                ListViewVaultGames.ItemsSource = null;
                ListViewVaultGames.Items.Clear();
                ListViewVaultGames.ItemsSource = gameVault;
                GridImportSteamId.Visibility = Visibility.Hidden;




        }

        private void Import_Steam_User_Worker_DoWork(object sender, DoWorkEventArgs e)
        {

            //Update UI here

            double playtimeInMinutes = 0;
            double doubleSteamID;

            string steamID = currentSteamUserID;
            steamID = steamID.Trim();
            if (double.TryParse(steamID, out doubleSteamID))
            {
                SteamApiClient steamClient = new SteamApiClient();
                SteamUserGameList usersGames = steamClient.GetUsersGames(steamID);
                Console.WriteLine("Games Found: " + usersGames.game_count + "\n\n");
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                {
                    ProgressBarImport.Maximum = usersGames.game_count;
                    ProgressBarImport.Minimum = 1;
                }));

                int gameCount = 1;
                Console.WriteLine();
                foreach (SteamUserGame userGame in usersGames.games)
                {           
                    if (!worker.CancellationPending)
                    {
                        playtimeInMinutes += userGame.playtime_forever;
                        Game newGameFound = new Game();
                        newGameFound.Name = userGame.name;
                        newGameFound.steamAppId = userGame.appid;

                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                          {
                              gameVault.Add(newGameFound);
                              ProgressBarImport.Value = gameCount;
                              LabelProgressGameName.Content = newGameFound.Name;
                              LabelProgressCount.Content = gameCount + 1 + " / " + usersGames.game_count;
                              TextBlockImportProfileSteamId.Text = "Steam Id: " + steamID;
                              TextBlockVaultGamesCount.Text = gameCount + " Games";
                          }));
                        gameCount++;

                    }

                }

                //This will give us the full name path of the executable file:
                //i.e. C:\Program Files\MyApplication\MyApplication.exe
                string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                //This will strip just the working path name:
                //C:\Program Files\MyApplication
                string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);

                JsonSerializer serializer = new JsonSerializer();
                using (StreamWriter sw = new StreamWriter(strWorkPath + @"/Profiles/" + steamID + ".txt"))
                using (JsonWriter jsonWriter = new JsonTextWriter(sw))
                {
                    serializer.Serialize(jsonWriter, gameVault);
                }

                double hours = playtimeInMinutes / 60;
                double days = hours / 24;
                //MessageBox.Show($"Holy shit bro. You have played for a total of {playtimeInMinutes} minutes!\nThat is {hours} hours or {days} days");

            }
            else
            {
                //MessageBox.Show("Enter a valid Steam ID");
            }


            UserImportSuccess = true;
        }

        private void ButtonImportFromSteam_Click(object sender, RoutedEventArgs e)
        {

            currentSteamUserID = TextBoxSteamID.Text;
            SteamApiClient steamClient = new SteamApiClient();
            SteamUserResponse users = steamClient.GetUserDetailsFromSteamID(currentSteamUserID);
            if (users.players.Count == 1)
            {
                SteamUser user = users.players[0];
                BitmapImage avatar = new BitmapImage(new Uri(user.avatarfull, UriKind.Absolute));
                ImageImportProfilePicture.Source = avatar;
                TextBlockImportProfileUsername.Text = user.personaname;
                TextBlockImportProfileSteamId.Text = user.steamid;
            }
            GridUserDetails.Visibility = Visibility.Visible;
            //Import_Steam_User_Worker_StartWork();

        }

        private void ListViewVaultGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListViewVaultGames.SelectedIndex >= 0)
            {
                selectedVaultGame = ListViewVaultGames.SelectedItem as Game;
                Console.WriteLine("Name: " + selectedVaultGame.Name + "\nAppId: " + selectedVaultGame.steamAppId);
            }
            else
            {

            }

        }

        public void UIButtonsEnabled(bool enabled)
        {
            if (enabled)
            {
                ButtonCoinFlip.IsEnabled = true;
                ButtonGameGrab.IsEnabled = true;
                ButtonGameVault.IsEnabled = true;
                ButtonPlaylists.IsEnabled = true;
                ButtonSettings.IsEnabled = true;
                ButtonExit.IsEnabled = true;
            }
            else
            {
                ButtonCoinFlip.IsEnabled = false;
                ButtonGameGrab.IsEnabled = false;
                ButtonGameVault.IsEnabled = false;
                ButtonPlaylists.IsEnabled = false;
                ButtonSettings.IsEnabled = false;
                ButtonExit.IsEnabled = false;
            }
        }

        private void ButtonCancelImportSteamId_Click(object sender, RoutedEventArgs e)
        {
            userStoppedImport = true;
            worker.CancelAsync();
            GridImportSteamId.Visibility = Visibility.Hidden;
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            var selectedButton = ButtonSettings;
            var selectedGridPanel = GridSettings;
            PanelToggle(selectedGridPanel);
            ButtonToggle(selectedButton);
        }

        public void PanelToggle(Grid selectedGrid)
        {

            GridGameVault.Visibility = Visibility.Hidden;
            GridGameGrab.Visibility = Visibility.Hidden;
            GridCoinFlip.Visibility = Visibility.Hidden;
            GridPlaylists.Visibility = Visibility.Hidden;
            GridSettings.Visibility = Visibility.Hidden;
            GridUserDetails.Visibility = Visibility.Hidden;

            selectedGrid.Visibility = Visibility.Visible;
        }
        public void ButtonToggle(Button selectedButton)
        {
            ButtonGameGrab.Background = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            ButtonCoinFlip.Background = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            ButtonGameVault.Background = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            ButtonPlaylists.Background = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));
            ButtonSettings.Background = new SolidColorBrush(Color.FromArgb(102, 0, 0, 0));

            selectedButton.Background = new SolidColorBrush(Color.FromArgb(165, 0, 0, 0));
        }

        private void ButtonGameGrab_Click(object sender, RoutedEventArgs e)
        {
            var selectedButton = ButtonGameGrab;
            var selectedGridPanel = GridGameGrab;
            PanelToggle(selectedGridPanel);
            ButtonToggle(selectedButton);
        }

        private void ButtonCoinFlip_Click(object sender, RoutedEventArgs e)
        {
            ButtonCoinFlip.Background = new SolidColorBrush(Color.FromArgb(255, 25, 29, 37));
            var selectedGridPanel = GridCoinFlip;
            var selectedButton = ButtonCoinFlip;
            PanelToggle(selectedGridPanel);
            ButtonToggle(selectedButton);
        }

        private void ButtonPlaylists_Click(object sender, RoutedEventArgs e)
        {
            ButtonPlaylists.Background = new SolidColorBrush(Color.FromArgb(255, 25, 29, 37));
            var selectedGridPanel = GridPlaylists;
            var selectedButton = ButtonPlaylists;
            PanelToggle(selectedGridPanel);
            ButtonToggle(selectedButton);
        }

        private void TextBoxSearchSteamGames_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSearchSteamGames.Text == "" || TextBoxSearchSteamGames.Text.Length < 3)
            {
                Console.WriteLine("No Search Input\n\n");

                ListViewSteamGames.ItemsSource = null;
                ListViewSteamGames.Items.Clear();
                ListViewSteamGames.ItemsSource = steamGames;
                if (ListViewSteamGames.Items.Count > 0)
                {
                    ListViewSteamGames.SelectedIndex = 0;
                    ListViewSteamGames.ScrollIntoView(ListViewSteamGames.Items[0]);
                }
            }
            else
            {
                ObservableCollection<SteamAppChunk> appSearch = new ObservableCollection<SteamAppChunk>(steamGames.Where(x => x.name.ToLower().Contains(TextBoxSearchSteamGames.Text.ToLower())));
                Console.WriteLine("Apps Found With Search: " + TextBoxSearchSteamGames.Text + "\n\n");

                ListViewSteamGames.ItemsSource = null;
                ListViewSteamGames.Items.Clear();
                ListViewSteamGames.ItemsSource = appSearch;
                if (ListViewSteamGames.Items.Count > 0)
                {
                    ListViewSteamGames.SelectedIndex = 0;
                    ListViewSteamGames.ScrollIntoView(ListViewSteamGames.Items[0]);
                }
            }
        }

        private void ButtonImportGamesFromProfile_Click(object sender, RoutedEventArgs e)
        {
            Import_Steam_User_Worker_StartWork();
        }

        private void ButtonAddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            //ListViewPlaylists.Items.Clear();
            Playlist pl = new Playlist();
            pl.Name = "LAN Party 2023";
            pl.Description = "A collection of games picked by Fatalcore, Epithermal, and VanillaSoul";
            Game game = new Game();
            game.Name = "Counter-strike";
            game.Description = "shooter";
            game.ImageUrl = "https://cdn.akamai.steamstatic.com/steam/apps/10/header.jpg?t=1666823513";
            Game game2 = new Game();
            game2.Name = "Counter-strike: Condition-Zero";
            game2.Description = "shooter";
            game2.ImageUrl = "https://cdn.akamai.steamstatic.com/steam/apps/80/header.jpg?t=1602535977";
            Game game3 = new Game();
            game3.Name = "Day of Defeat";
            game3.Description = "shooter";
            game3.ImageUrl = "https://cdn.akamai.steamstatic.com/steam/apps/300/header.jpg?t=1675457675";
            Game game4 = new Game();
            game4.Name = "Day of Defeat";
            game4.Description = "shooter";
            game4.ImageUrl = "https://cdn.akamai.steamstatic.com/steam/apps/20/header.jpg?t=1579634708";
            pl.Games.Add(game);
            pl.Games.Add(game2);
            pl.Games.Add(game3);
            pl.Games.Add(game4);
            pl.tempImage1 = new BitmapImage(new Uri(game.ImageUrl, UriKind.Absolute));
            pl.tempImage2 = new BitmapImage(new Uri(game2.ImageUrl, UriKind.Absolute));
            pl.tempImage3 = new BitmapImage(new Uri(game3.ImageUrl, UriKind.Absolute));
            pl.tempImage4 = new BitmapImage(new Uri(game4.ImageUrl, UriKind.Absolute));
            playlists.Add(pl);
            ListViewPlaylists.ItemsSource = null;
            ListViewPlaylists.Items.Clear();
            ListViewPlaylists.ItemsSource = playlists;

            //ListViewPlaylists.Items.Add(pl);


        }

        public void Import_Steam_Games_Worker_StartWork()
        {
            steamAppWorker.WorkerSupportsCancellation = false;
            steamAppWorker.DoWork += Import_Steam_Games_Worker_DoWork;
            steamAppWorker.RunWorkerCompleted += Import_Steam_Games_Worker_RunWorkerCompleted;
            steamAppWorker.RunWorkerAsync();
            Console.WriteLine("STARTUP", "Start import of all of steams games");
            //TextBoxSearchSteamGames.IsEnabled = false;
            //ButtonSearchSteamGames.IsEnabled = false;
        }
        private void Import_Steam_Games_Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("Finished importing all steam games from API");
            Console.WriteLine("Sorting Steam List Alphabetically");
            
            //TextBoxSearchSteamGames.IsEnabled = true;
            //ButtonSearchSteamGames.IsEnabled = true;
        }
        private void Import_Steam_Games_Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            SteamApiClient client = new SteamApiClient();
            SteamAppChunkResponse appChunk = new SteamAppChunkResponse();

            //continue pulling chunks until more results = false;
            bool moreResults = true;
            int lastAppId = 0;
            int count = 0;
            while (moreResults)
            {
                //Pulls 10,000 results from the API
                appChunk = client.GetSteamAppChunk(lastAppId);
                //For each game it finds, add it to the steamGames Observable Collection
                foreach (var app in appChunk.apps)
                {
                    //counter to see how many games have been imported
                    count++;
                    //Dispatcher to call back to the UI thread
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate
                    {
                        steamGames.Add(app);
                        TextBlockSteamImportGameCount.Text = count + " Games";
                    }));
                }
                //If response indicated there is no more results, break out of the while loop.
                if (!appChunk.have_more_results)
                {
                    moreResults = false;
                }
                //If the response indicates there is more results, then set the lastAppId to the last app returned from the previous call.
                else
                {
                    lastAppId = appChunk.last_appid;
                }
            }
        }
    }
}

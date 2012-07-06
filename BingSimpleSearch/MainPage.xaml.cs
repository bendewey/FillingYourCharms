using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bing;
using NotificationsExtensions.TileContent;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Search;
using Windows.Media.PlayTo;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BingSimpleSearch
{
    /// <summary>
    /// The code-behind for the MainPage of the app
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SearchPane _searchPane;
        private DataTransferManager _dataTransfer;
        private PlayToManager _playToManager;
        private SettingsPane _settingsPane;
        private string _lastSearch;

        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;
            Unloaded += MainPage_Unloaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _searchPane = SearchPane.GetForCurrentView();
            _searchPane.QuerySubmitted += QuerySubmitted;
            _searchPane.SuggestionsRequested += SuggestionsRequested;

            _dataTransfer = DataTransferManager.GetForCurrentView();
            _dataTransfer.DataRequested += DataRequested;

            _playToManager = PlayToManager.GetForCurrentView();
            _playToManager.SourceRequested += SourceRequested;

            _settingsPane = SettingsPane.GetForCurrentView();
            _settingsPane.CommandsRequested += CommandsRequested;
        }

        void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            _searchPane.QuerySubmitted -= QuerySubmitted;
            _dataTransfer.DataRequested -= DataRequested;
            _playToManager.SourceRequested -= SourceRequested;
            _settingsPane.CommandsRequested += CommandsRequested;
        }

        #region Search
        async void QuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            await ExecuteSearch(args.QueryText);
        }

        void Search_Click(object sender, RoutedEventArgs e)
        {
            _searchPane.Show();
        }

        public async Task ExecuteSearch(string searchQuery)
        {
            _lastSearch = searchQuery;
            var context = new BingSearchContainer(
                new Uri("https://api.datamarket.azure.com/Data.ashx/Bing/Search"));
            context.Credentials = new NetworkCredential(BingAzureDataMarket.AccountKey, BingAzureDataMarket.AccountKey);

            var serviceQuery = context.Image(searchQuery, "en-US", null, null, null, null);
            var result = await serviceQuery.ExecuteAsync();

            ImagesList.ItemsSource = result.ToList();
        }

        void SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            var matches = from w in Dictionary.Fruits
                          where w.ToUpper().StartsWith(args.QueryText.ToUpper())
                          select w;

            foreach (var w in matches)
            {
                args.Request.SearchSuggestionCollection.AppendQuerySuggestion(w);
            }
        }
        #endregion

        #region Sharing
        void DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (ImagesList.SelectedItem == null)
            {
                ShareUri(args);
            }
            else
            {
                var image = ImagesList.SelectedItem as ImageResult;
                ShareBitmap(args, image);
            }
        }

        public void ShareUri(DataRequestedEventArgs args)
        {
            args.Request.Data.Properties.Title = "Bing Image Search Link";
            args.Request.Data.SetUri(new Uri("http://www.bing.com/images/search?q=" 
                + _lastSearch));
        }

        private void ShareBitmap(DataRequestedEventArgs args, ImageResult image)
        {
            args.Request.Data.Properties.Title = "Bing Image Search Bitmap";
            args.Request.Data.SetDataProvider(StandardDataFormats.Bitmap, async dpr =>
            {
                var deferral = dpr.GetDeferral();

                var shareFile = await DownloadFileAsync(image.MediaUrl);
                var stream = await shareFile.OpenAsync(FileAccessMode.Read);
                dpr.SetData(RandomAccessStreamReference.CreateFromStream(stream));

                deferral.Complete();
            });
        }

        private async Task<StorageFile> DownloadFileAsync(string uri)
        {
            var filename = Regex.Replace(uri, "https?://|[/?&#]", "");
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await new BackgroundDownloader().CreateDownload(new Uri(uri), file).StartAsync();
            return file;
        } 
        #endregion

        #region Start (Tiles)
        private async void SetTile_Click(object sender, RoutedEventArgs e)
        {
            var image = ImagesList.SelectedItem as ImageResult;
            if (image == null)
            {
                await new MessageDialog("Nothing Selected").ShowAsync();
                return;
            }

            var url = image.MediaUrl;
            if (image.Width > 800)
            {
                // after much testing it appears that images > 800px cannot be used as tiles
                url = image.Thumbnail.MediaUrl;
            }

            var content = TileContentFactory.CreateTileWidePeekImageAndText01();
            content.TextBodyWrap.Text = image.Title;
            content.Image.Src = url;
            content.Image.Alt = image.Title;

            // Square image substitute
            var squareContent = TileContentFactory.CreateTileSquareImage();
            squareContent.Image.Src = url;
            squareContent.Image.Alt = image.Title;

            content.SquareContent = squareContent;

            var notification = content.CreateNotification();
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);

            await new MessageDialog("Tile Update sent for " + image.Title).ShowAsync();
        } 
        #endregion

        #region Devices
        void SourceRequested(PlayToManager sender, PlayToSourceRequestedEventArgs args)
        {
            var deferral = args.SourceRequest.GetDeferral();
            var handler = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                args.SourceRequest.SetSource(FullSizeImage.PlayToSource);
                deferral.Complete();
            });
        } 
        #endregion

        #region Settings
        void CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var preferencesCommand = new SettingsCommand("Preferences", "Preferences", 
                ShowPreferences);
            args.Request.ApplicationCommands.Add(preferencesCommand);
        }

        private void ShowPreferences(IUICommand command)
        {
            this.PreferencesPage.Show();
        }
        #endregion
    }
}

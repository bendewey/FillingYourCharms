using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using Windows.UI.Xaml;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace BingSimpleSearch
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private MainPage _mainPage;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Launch(args.PreviousExecutionState);
        }

        protected override async void OnSearchActivated(SearchActivatedEventArgs args)
        {
            Launch(args.PreviousExecutionState);
            await _mainPage.ExecuteSearch(args.QueryText);
        }

        private void Launch(ApplicationExecutionState previousState)
        {
            // Do not repeat app initialization when already running, just ensure that
            // the window is active
            if (previousState == ApplicationExecutionState.Running)
            {
                Window.Current.Activate();
                return;
            }

            _mainPage = new MainPage();
            Window.Current.Content = _mainPage;
            Window.Current.Activate();
        }

        void App_SuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            var matches = from w in Dictionary.Fruits
                          where w.ToUpper().StartsWith(args.QueryText.ToUpper())
                          select w;

            foreach(var w in matches)
            {
                args.Request.SearchSuggestionCollection.AppendQuerySuggestion(w);
            }
        }
    }
}

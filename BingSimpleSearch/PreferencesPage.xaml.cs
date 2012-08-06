using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BingSimpleSearch
{
    public sealed partial class PreferencesPage : UserControl
    {
        public PreferencesPage()
        {
            this.InitializeComponent();
        }

        public void Show()
        {
            VisualStateManager.GoToState(this, "PreferencesOpened", true);
        }

        public void Hide()
        {
            VisualStateManager.GoToState(this, "PreferencesClosed", true);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            SettingsPane.Show();
        }

        private void Overlay_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Hide();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await new MessageDialog("Message from settings.").ShowAsync();
        }
    }
}

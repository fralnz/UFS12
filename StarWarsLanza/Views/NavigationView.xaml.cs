using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Per la NavigationView ho seguito l'esempio riportato dalle registrazioni del prof.

namespace StarWarsLanza.Views
{
    public sealed partial class NavigationView : Page
    {
        public NavigationView()
        {
            this.InitializeComponent();
        }

        private void OnNavigate(Object sender, RoutedEventArgs e)
        {
            NavView.IsBackEnabled = NavigationViewFrame.CanGoBack;
        }

        private void NavMain_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationViewFrame.Navigate(typeof(Views.CharacterRequest), e);
        }

        private void NavOne_Tapped(object sender, TappedRoutedEventArgs e)
        {
            NavigationViewFrame.Navigate(typeof(Views.ExportPage), e);
        }

        private void NavigationViewFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavView.IsBackEnabled = NavigationViewFrame.CanGoBack;
        }

        private void NavView_BackRequested(Windows.UI.Xaml.Controls.NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (NavigationViewFrame.CanGoBack)
            {
                NavigationViewFrame.GoBack();
            }
        }
    }
}

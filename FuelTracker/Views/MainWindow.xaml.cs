using FuelTracker.Models;
using System;
using System.Windows;

namespace FuelTracker.Views
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow
    {
        #region Window Manipulation

        public MainWindow() => InitializeComponent();

        private void WindowMain_Loaded(object sender, RoutedEventArgs e) => AppState.MainWindow = this;

        #endregion Window Manipulation
    }
}
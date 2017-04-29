using System;
using System.Collections.Generic;
using Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FuelTracker
{
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow
    {
        #region Control Manipulation

        /// <summary>Handles enabling the Login button.</summary>
        private void TextChanged()
        {
            BtnLogin.IsEnabled = TxtUsername.Text.Length > 0 && PswdPassword.Password.Length > 0;
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (await AppState.Login(TxtUsername.Text, PswdPassword.Password))
            {
                TxtUsername.Text = "";
                PswdPassword.Clear();
                TxtUsername.Focus();
                ViewAccountWindow viewAccountWindow = new ViewAccountWindow { PreviousWindow = this };
                viewAccountWindow.RefreshItemsSource();
                viewAccountWindow.Show();
                Visibility = Visibility.Hidden;
            }
        }

        private void BtnNewUser_Click(object sender, RoutedEventArgs e)
        {
            NewUserWindow newUserWindow = new NewUserWindow { PreviousWindow = this };
            newUserWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void TxtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        private void TxtUsername_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Letters);
        }

        private void TxtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        private void PswdPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.PasswordBoxGotFocus(sender);
        }

        private void PswdPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TextChanged();
        }

        #endregion Control Manipulation

        #region Window Manipulation

        public MainWindow()
        {
            InitializeComponent();
            TxtUsername.Focus();
        }

        #endregion Window Manipulation

        private void WindowMain_Loaded(object sender, RoutedEventArgs e)
        {
            AppState.VerifyDatabaseIntegrity();
        }
    }
}
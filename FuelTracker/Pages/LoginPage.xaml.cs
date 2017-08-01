using Extensions;
using Extensions.Enums;
using FuelTracker.Classes;
using FuelTracker.Pages.Account;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FuelTracker.Pages
{
    /// <summary>Interaction logic for LoginPage.xaml</summary>
    public partial class LoginPage
    {
        #region Control Manipulation

        /// <summary>Handles enabling the Login button.</summary>
        private void TextChanged() => BtnLogin.IsEnabled = TxtUsername.Text.Length > 0 && PswdPassword.Password.Length > 0;

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (await AppState.Login(TxtUsername.Text, PswdPassword.Password))
            {
                TxtUsername.Text = "";
                PswdPassword.Clear();
                TxtUsername.Focus();
                AppState.Navigate(new ViewAccountPage());
            }
        }

        private void BtnNewUser_Click(object sender, RoutedEventArgs e) => AppState.Navigate(new NewUserPage());

        private void TxtUsername_GotFocus(object sender, RoutedEventArgs e) => Functions.TextBoxGotFocus(sender);

        private void TxtUsername_PreviewKeyDown(object sender, KeyEventArgs e) => Functions.PreviewKeyDown(e, KeyType.Letters);

        private void TxtUsername_TextChanged(object sender, TextChangedEventArgs e) => TextChanged();

        private void PswdPassword_GotFocus(object sender, RoutedEventArgs e) => Functions.PasswordBoxGotFocus(sender);

        private void PswdPassword_PasswordChanged(object sender, RoutedEventArgs e) => TextChanged();

        #endregion Control Manipulation

        #region Page-Manipulation Methods

        public LoginPage()
        {
            InitializeComponent();
            AppState.VerifyDatabaseIntegrity();
            TxtUsername.Focus();
        }

        private void LoginPage_OnLoaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);

        #endregion Page-Manipulation Methods
    }
}
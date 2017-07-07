using Extensions;
using Extensions.Enums;
using FuelTracker.Classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FuelTracker.Windows.Account
{
    /// <summary>Interaction logic for NewUserWindow.xaml</summary>
    public partial class NewUserWindow
    {
        internal Window PreviousWindow { get; set; }

        /// <summary>Handles enabling the Create button.</summary>
        private void TextChanged()
        {
            BtnCreate.IsEnabled = TxtUsername.Text.Length > 0 && PswdPassword.Password.Length > 0 && PswdConfirm.Password.Length > 0;
        }

        #region Button-Click Methods

        private async void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (await AppState.NewUser(TxtUsername.Text, PswdPassword.Password))
                CloseWindow();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region TextBox Manipulation

        private void TxtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        private void TxtUsername_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Letters);
        }

        private void Pswd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TextChanged();
        }

        private void Pswd_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.PasswordBoxGotFocus(sender);
        }

        private void TxtUsername_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        #endregion TextBox Manipulation

        #region Window Manipulation

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        public NewUserWindow()
        {
            InitializeComponent();
            TxtUsername.Focus();
        }

        private void WindowNewUser_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PreviousWindow.Show();
        }

        #endregion Window Manipulation
    }
}
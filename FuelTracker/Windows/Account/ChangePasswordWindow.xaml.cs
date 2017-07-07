using System.ComponentModel;
using System.Windows;
using Extensions;
using Extensions.Encryption;
using FuelTracker.Classes;
using FuelTracker.Classes.Entities;

namespace FuelTracker.Windows.Account
{
    /// <summary>Interaction logic for ChangePasswordWindow.xaml</summary>
    public partial class ChangePasswordWindow
    {
        internal ViewAccountWindow PreviousWindow { get; set; }

        #region Button-Click Methods

        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (Argon2.ValidatePassword(AppState.CurrentUser.Password, PswdCurrentPassword.Password))
                if (PswdNewPassword.Password.Length >= 4 && PswdConfirmPassword.Password.Length >= 4)
                    if (PswdNewPassword.Password == PswdConfirmPassword.Password)
                        if (PswdCurrentPassword.Password != PswdNewPassword.Password)
                        {
                            User newUser =
                                new User(AppState.CurrentUser)
                                {
                                    Password = Argon2.HashPassword(PswdNewPassword.Password)
                                };
                            if (await AppState.ChangeUserDetails(AppState.CurrentUser, newUser))
                            {
                                AppState.CurrentUser.Password = newUser.Password;
                                AppState.DisplayNotification("Successfully changed password.", "Sulimn", this);
                                CloseWindow();
                            }
                        }
                        else
                        {
                            AppState.DisplayNotification("The new password can't be the same as the current password.", "Sulimn",
                                this);
                        }
                    else
                        AppState.DisplayNotification("Please ensure the new passwords match.", "Sulimn", this);
                else
                    AppState.DisplayNotification("Your password must be at least 4 characters.", "Sulimn", this);
            else
                AppState.DisplayNotification("Invalid current password.", "Sulimn", this);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        private void PswdChanged(object sender, RoutedEventArgs e)
        {
            BtnSubmit.IsEnabled = PswdCurrentPassword.Password.Length > 0 && PswdNewPassword.Password.Length > 0 &&
                                  PswdConfirmPassword.Password.Length > 0;
        }

        private void Pswd_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.PasswordBoxGotFocus(sender);
        }

        #endregion Window-Manipulation Methods

        public ChangePasswordWindow()
        {
            InitializeComponent();
        }

        private void WindowChangePassword_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.Show();
        }
    }
}
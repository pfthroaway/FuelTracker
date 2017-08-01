using Extensions;
using Extensions.Encryption;
using FuelTracker.Classes;
using FuelTracker.Classes.Entities;
using System.Windows;

namespace FuelTracker.Pages.Account
{
    /// <summary>Interaction logic for ChangePasswordPage.xaml</summary>
    public partial class ChangePasswordPage
    {
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
                                AppState.DisplayNotification("Successfully changed password.", "Fuel Tracker");
                                ClosePage();
                            }
                        }
                        else
                        {
                            AppState.DisplayNotification("The new password can't be the same as the current password.", "Fuel Tracker");
                        }
                    else
                        AppState.DisplayNotification("Please ensure the new passwords match.", "Fuel Tracker");
                else
                    AppState.DisplayNotification("Your password must be at least 4 characters.", "Fuel Tracker");
            else
                AppState.DisplayNotification("Invalid current password.", "Fuel Tracker");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => ClosePage();

        #endregion Button-Click Methods

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        private void PswdChanged(object sender, RoutedEventArgs e) => BtnSubmit.IsEnabled =
            PswdCurrentPassword.Password.Length > 0 && PswdNewPassword.Password.Length > 0 &&
            PswdConfirmPassword.Password.Length > 0;

        private void Pswd_GotFocus(object sender, RoutedEventArgs e) => Functions.PasswordBoxGotFocus(sender);

        #endregion Page-Manipulation Methods

        public ChangePasswordPage()
        {
            InitializeComponent();
            PswdCurrentPassword.Focus();
        }

        private void ChangePasswordPage_OnLoaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);
    }
}
using Extensions;
using Extensions.DataTypeHelpers;
using Extensions.Enums;
using FuelTracker.Classes;
using FuelTracker.Classes.Entities;
using FuelTracker.Windows.Account;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FuelTracker.Windows.Vehicles
{
    /// <summary>Interaction logic for ModifyVehicleWindow.xaml</summary>
    public partial class ModifyVehicleWindow
    {
        internal Vehicle UnmodifiedVehicle, ModifiedVehicle;

        internal ManageVehicleWindow PreviousWindow { get; set; }

        /// <summary>Resets all TextBoxes to no text.</summary>
        internal void DisplayOriginalVehicle()
        {
            TxtNickname.Text = UnmodifiedVehicle.Nickname;
            TxtMake.Text = UnmodifiedVehicle.Make;
            TxtModel.Text = UnmodifiedVehicle.Model;
            TxtYear.Text = UnmodifiedVehicle.Year.ToString();
        }

        #region Button-Click Methods

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ModifiedVehicle = new Vehicle(await AppState.GetNextVehicleIndex(), AppState.CurrentUser.ID, TxtNickname.Text, TxtMake.Text, TxtModel.Text, Int32Helper.Parse(TxtYear.Text), UnmodifiedVehicle.Transactions);
            if (await AppState.ModifyVehicle(UnmodifiedVehicle, ModifiedVehicle))
            {
                AppState.CurrentUser.ModifyVehicle(UnmodifiedVehicle, ModifiedVehicle);
                PreviousWindow.CurrentVehicle = ModifiedVehicle;
                CloseWindow();
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            DisplayOriginalVehicle();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        #endregion Button-Click Methods

        #region Text Manipulation

        /// <summary>Handles all the TextBoxes TextChanged events.</summary>
        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            bool enabled = TxtNickname.Text.Length > 0 && TxtMake.Text.Length > 0 && TxtModel.Text.Length > 0 &&
                           TxtYear.Text.Length == 4 && (TxtNickname.Text != UnmodifiedVehicle.Nickname ||
                                                        TxtMake.Text != UnmodifiedVehicle.Make ||
                                                        TxtModel.Text != UnmodifiedVehicle.Model ||
                                                        TxtYear.Text != UnmodifiedVehicle.Year.ToString());
            BtnSave.IsEnabled = enabled;
            BtnReset.IsEnabled = enabled;
        }

        private void Txt_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        private void TxtYear_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Integers);
        }

        #endregion Text Manipulation

        #region Window Manipulation

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        public ModifyVehicleWindow()
        {
            InitializeComponent();
        }

        private void WindowModifyVehicle_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.RefreshItemsSource();
            PreviousWindow.Show();
        }

        #endregion Window Manipulation
    }
}
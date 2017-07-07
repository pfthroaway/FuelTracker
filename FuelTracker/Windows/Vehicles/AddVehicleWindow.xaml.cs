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
    /// <summary>Interaction logic for AddVehicleWindow.xaml</summary>
    public partial class AddVehicleWindow
    {
        internal ViewAccountWindow PreviousWindow { get; set; }

        /// <summary>Resets all TextBoxes to no text.</summary>
        private void Reset()
        {
            TxtNickname.Text = "";
            TxtMake.Text = "";
            TxtModel.Text = "";
            TxtYear.Text = "";
        }

        #region Button-Click Methods

        private async void BtnAddVehicle_Click(object sender, RoutedEventArgs e)
        {
            Vehicle newVehicle = new Vehicle(await AppState.GetNextVehicleIndex(), AppState.CurrentUser.ID, TxtNickname.Text, TxtMake.Text, TxtModel.Text, Int32Helper.Parse(TxtYear.Text), new List<Transaction>());
            if (await AppState.NewVehicle(newVehicle))
            {
                AppState.CurrentUser.AddVehicle(newVehicle);
                CloseWindow();
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
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
            BtnAddVehicle.IsEnabled = TxtNickname.Text.Length > 0 && TxtMake.Text.Length > 0 && TxtModel.Text.Length > 0 && TxtYear.Text.Length == 4;
            BtnReset.IsEnabled = TxtNickname.Text.Length > 0 || TxtMake.Text.Length > 0 || TxtModel.Text.Length > 0 || TxtYear.Text.Length > 0;
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

        public AddVehicleWindow()
        {
            InitializeComponent();
        }

        private void WindowAddVehicle_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.RefreshItemsSource();
            PreviousWindow.Show();
        }

        #endregion Window Manipulation
    }
}
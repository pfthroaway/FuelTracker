﻿using Extensions;
using Extensions.ListViewHelp;
using FuelTracker.Classes;
using FuelTracker.Classes.Entities;
using FuelTracker.Pages.Vehicles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace FuelTracker.Pages.Account
{
    /// <summary>Interaction logic for ViewAccountPage.xaml</summary>
    public partial class ViewAccountPage : INotifyPropertyChanged
    {
        private List<Vehicle> _allVehicles;
        private ListViewSort _sort = new ListViewSort();
        private Vehicle _selectedVehicle;

        /// <summary>The currently selected Vehicle.</summary>
        public Vehicle SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                _selectedVehicle = value;
                OnPropertyChanged("SelectedVehicle");
            }
        }

        /// <summary>Refreshes the ListView's ItemsSource.</summary>
        internal void RefreshItemsSource()
        {
            _allVehicles = new List<Vehicle>(AppState.CurrentUser.Vehicles);
            LVVehicles.ItemsSource = _allVehicles;
            LVVehicles.Items.Refresh();
            DataContext = SelectedVehicle;
        }

        private void ToggleButtons(bool enabled)
        {
            BtnManageFuelups.IsEnabled = enabled;
            BtnModifyVehicle.IsEnabled = enabled;
            BtnDeleteVehicle.IsEnabled = enabled;
        }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this,
            new PropertyChangedEventArgs(property));

        #endregion Data-Binding

        #region Button-Click Methods

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e) =>
            AppState.Navigate(new ChangePasswordPage());

        private async void BtnDeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            string message = "Are you sure you want to delete this vehicle?";
            if (_selectedVehicle.Transactions.Count > 0)
                message += $" You will also be deleting its {_selectedVehicle.Transactions.Count} fuel-ups.";
            message += " This action cannot be undone.";
            if (AppState.YesNoNotification(message, "Fuel Tracker"))
                if (await AppState.DeleteVehicle(_selectedVehicle))
                {
                    AppState.CurrentUser.RemoveVehicle(_selectedVehicle);
                    RefreshItemsSource();
                }
        }

        private void BtnLogOut_Click(object sender, RoutedEventArgs e)
        {
            AppState.CurrentUser = new User();
            ClosePage();
        }

        private void BtnModifyVehicle_Click(object sender, RoutedEventArgs e) => AppState.Navigate(
            new ModifyVehiclePage { UnmodifiedVehicle = _selectedVehicle, ModifiedVehicle = new Vehicle(_selectedVehicle) });

        private void BtnNewVehicle_Click(object sender, RoutedEventArgs e) => AppState.Navigate(new AddVehiclePage());

        private void BtnManageVehicle_Click(object sender, RoutedEventArgs e)
        {
            AppState.Navigate(
                new ManageFuelupsPage { CurrentVehicle = SelectedVehicle });
            //LVVehicles.UnselectAll();
        }

        #endregion Button-Click Methods

        #region Page-Manipulation Methods

        /// <summary>Closes the Page.</summary>
        private void ClosePage() => AppState.GoBack();

        private void LVVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedVehicle = (Vehicle)LVVehicles.SelectedItem;
            ToggleButtons(LVVehicles.SelectedIndex >= 0);
            RefreshItemsSource();
        }

        private void LVVehiclesColumnHeader_Click(object sender, RoutedEventArgs e) => _sort = Functions.ListViewColumnHeaderClick(sender, _sort, LVVehicles, "#CCCCCC");

        public ViewAccountPage() => InitializeComponent();

        private void ViewAccountPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            AppState.CalculateScale(Grid);
            RefreshItemsSource();
        }

        #endregion Page-Manipulation Methods
    }
}
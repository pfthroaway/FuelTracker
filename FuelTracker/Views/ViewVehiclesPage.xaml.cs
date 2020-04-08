﻿using Extensions;
using Extensions.ListViewHelp;
using FuelTracker.Models;
using FuelTracker.Models.Entities;
using FuelTracker.Views.Vehicles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace FuelTracker.Views
{
    /// <summary>Interaction logic for ViewVehiclesPage.xaml</summary>
    public partial class ViewVehiclesPage : INotifyPropertyChanged
    {
        private bool _loaded;
        private List<Vehicle> _allVehicles;
        private ListViewSort _sort = new ListViewSort();
        private Vehicle _selectedVehicle;

        /// <summary>The currently selected <see cref="Vehicle"/>.</summary>
        public Vehicle SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                _selectedVehicle = value;
                NotifyPropertyChanged(nameof(SelectedVehicle));
            }
        }

        /// <summary>Refreshes the ListView's ItemsSource.</summary>
        internal void RefreshItemsSource()
        {
            _allVehicles = new List<Vehicle>(AppState.AllVehicles);
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

        /// <summary>Event that fires if a Property value has changed so that the UI can properly be updated.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Invokes <see cref="PropertyChangedEventHandler"/> to update the UI when a Property value changes.</summary>
        /// <param name="property">Name of Property whose value has changed</param>
        private void NotifyPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        #endregion Data-Binding

        #region Button-Click Methods

        private async void BtnDeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            string message = "Are you sure you want to delete this vehicle?";
            if (_selectedVehicle.Transactions.Count > 0)
                message += $" You will also be deleting its {_selectedVehicle.Transactions.Count} fuel-ups.";
            message += " This action cannot be undone.";
            if (AppState.YesNoNotification(message, "Fuel Tracker"))
                if (await AppState.DatabaseInteraction.DeleteVehicle(_selectedVehicle))
                {
                    AppState.AllVehicles.Remove(_selectedVehicle);
                    RefreshItemsSource();
                }
        }

        private void BtnModifyVehicle_Click(object sender, RoutedEventArgs e) => AppState.Navigate(
            new ModifyVehiclePage { UnmodifiedVehicle = _selectedVehicle, ModifiedVehicle = new Vehicle(_selectedVehicle) });

        private void BtnNewVehicle_Click(object sender, RoutedEventArgs e) => AppState.Navigate(new AddVehiclePage());

        private void BtnManageVehicle_Click(object sender, RoutedEventArgs e) => AppState.Navigate(
                new ManageFuelupsPage { CurrentVehicle = SelectedVehicle });//LVVehicles.UnselectAll();

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

        public ViewVehiclesPage() => InitializeComponent();

        private async void ViewVehiclesPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_loaded)
            {
                await AppState.FileManagement();
                _loaded = true;
            }

            RefreshItemsSource();
        }

        #endregion Page-Manipulation Methods
    }
}
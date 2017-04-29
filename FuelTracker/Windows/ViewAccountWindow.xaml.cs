using Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FuelTracker
{
    /// <summary>Interaction logic for ViewAccountWindow.xaml</summary>
    public partial class ViewAccountWindow : INotifyPropertyChanged
    {
        private List<Vehicle> _allVehicles;
        private GridViewColumnHeader _listViewSortCol;
        private SortAdorner _listViewSortAdorner;
        private Vehicle _selectedVehicle;
        internal MainWindow PreviousWindow { get; set; }

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

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Button-Click Methods

        private void BtnLogOut_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnNewVehicle_Click(object sender, RoutedEventArgs e)
        {
            AddVehicleWindow addVehicleWindow = new AddVehicleWindow { PreviousWindow = this };
            addVehicleWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnDeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.YesNoNotification(
                $"Are you sure you want to delete this vehicle? It will be deleted, along with the {SelectedVehicle.Transactions.Count} fuel-ups associated with it. This action cannot be undone.",
                "Fuel Tracker", this))
            { }
        }

        private void BtnModifyVehicle_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnAddFuelup_Click(object sender, RoutedEventArgs e)
        {
            AddTransactionWindow addTransactionWindow = new AddTransactionWindow { PreviousWindow = this, CurrentVehicle = SelectedVehicle };
            addTransactionWindow.Show();
            Visibility = Visibility.Hidden;
        }

        #endregion Button-Click Methods

        #region Window-Manipulation Methods

        private void CloseWindow()
        {
            Close();
        }

        private void LVVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool selected = LVVehicles.SelectedIndex >= 0;
            SelectedVehicle = (Vehicle)LVVehicles.SelectedItem;
            BtnAddFuelup.IsEnabled = selected;
            BtnModifyVehicle.IsEnabled = selected;
            BtnDeleteVehicle.IsEnabled = selected;
            RefreshItemsSource();
        }

        private void LVVehiclesColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            if (column != null)
            {
                string sortBy = column.Tag.ToString();
                if (_listViewSortCol != null)
                {
                    AdornerLayer.GetAdornerLayer(_listViewSortCol).Remove(_listViewSortAdorner);
                    LVVehicles.Items.SortDescriptions.Clear();
                }

                ListSortDirection newDir = ListSortDirection.Ascending;
                if (Equals(_listViewSortCol, column) && _listViewSortAdorner.Direction == newDir)
                    newDir = ListSortDirection.Descending;

                _listViewSortCol = column;
                _listViewSortAdorner = new SortAdorner(_listViewSortCol, newDir);
                AdornerLayer.GetAdornerLayer(_listViewSortCol).Add(_listViewSortAdorner);
                LVVehicles.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
            }
        }

        public ViewAccountWindow()
        {
            InitializeComponent();
        }

        private void WindowViewAccount_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}
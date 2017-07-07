using Extensions;
using Extensions.ListViewHelp;
using FuelTracker.Classes;
using FuelTracker.Classes.Entities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using FuelTracker.Windows.Vehicles;

namespace FuelTracker.Windows.Account
{
    /// <summary>Interaction logic for ViewAccountWindow.xaml</summary>
    public partial class ViewAccountWindow : INotifyPropertyChanged
    {
        private List<Vehicle> _allVehicles;
        private ListViewSort _sort = new ListViewSort();
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
            AppState.CurrentUser = new User();
            CloseWindow();
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            ChangePasswordWindow window = new ChangePasswordWindow { PreviousWindow = this };
            window.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnNewVehicle_Click(object sender, RoutedEventArgs e)
        {
            AddVehicleWindow addVehicleWindow = new AddVehicleWindow { PreviousWindow = this };
            addVehicleWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnManageVehicle_Click(object sender, RoutedEventArgs e)
        {
            ManageVehicleWindow window =
                new ManageVehicleWindow { PreviousWindow = this, CurrentVehicle = SelectedVehicle };
            window.RefreshItemsSource();
            window.Show();
            Visibility = Visibility.Hidden;
        }

        #endregion Button-Click Methods

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        private void LVVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedVehicle = (Vehicle)LVVehicles.SelectedItem;
            BtnManageVehicle.IsEnabled = LVVehicles.SelectedIndex >= 0; ;
            RefreshItemsSource();
        }

        private void LVVehiclesColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _sort = Functions.ListViewColumnHeaderClick(sender, _sort, LVVehicles, "#BDC7C1");
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
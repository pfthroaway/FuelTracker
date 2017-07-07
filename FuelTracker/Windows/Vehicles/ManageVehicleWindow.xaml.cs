using Extensions;
using Extensions.ListViewHelp;
using FuelTracker.Classes;
using FuelTracker.Classes.Entities;
using FuelTracker.Windows.Transactions;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FuelTracker.Windows.Vehicles
{
    /// <summary>Interaction logic for ViewVehicleTransactionsWindow.xaml</summary>
    public partial class ManageVehicleWindow
    {
        private Vehicle _currentVehicle;
        private ListViewSort _sort = new ListViewSort();
        private Transaction _selectedTransaction = new Transaction();

        internal Account.ViewAccountWindow PreviousWindow { get; set; }

        /// <summary>The currently selected Vehicle.</summary>
        internal Vehicle CurrentVehicle
        {
            get => _currentVehicle;
            set
            {
                _currentVehicle = value;
                OnPropertyChanged("CurrentVehicle");
            }
        }

        /// <summary>Refreshes the ListView's ItemsSource.</summary>
        internal void RefreshItemsSource()
        {
            CurrentVehicle = AppState.CurrentUser.Vehicles.ToList()
                .Find(vehicle => vehicle.Nickname == CurrentVehicle.Nickname);
            LVTransactions.ItemsSource = CurrentVehicle.Transactions;
            LVTransactions.Items.Refresh();
            DataContext = CurrentVehicle;
        }

        #region Data-Binding

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #endregion Data-Binding

        #region Click

        private void BtnAddFuelup_Click(object sender, RoutedEventArgs e)
        {
            AddTransactionWindow addTransactionWindow =
                new AddTransactionWindow { PreviousWindow = this, CurrentVehicle = CurrentVehicle };
            addTransactionWindow.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private async void BtnDeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (AppState.YesNoNotification("Are you sure you want to delete this Transaction? This action cannot be undone.",
                "Fuel Tracker", this))
                if (await AppState.DeleteTransaction(_selectedTransaction))
                {
                    CurrentVehicle.RemoveTransaction(_selectedTransaction);
                    RefreshItemsSource();
                }
        }

        private async void BtnDeleteVehicle_Click(object sender, RoutedEventArgs e)
        {
            string message = "Are you sure you want to delete this vehicle?";
            if (CurrentVehicle.Transactions.Count > 0)
                message += $" You will also be deleting its {CurrentVehicle.Transactions.Count} fuel-ups.";
            message += " This action cannot be undone.";
            if (AppState.YesNoNotification(message, "Fuel Tracker", this))
                if (await AppState.DeleteVehicle(_currentVehicle))
                {
                    AppState.CurrentUser.RemoveVehicle(CurrentVehicle);
                    CloseWindow();
                }
        }

        private void BtnModifyTransaction_Click(object sender, RoutedEventArgs e)
        {
            ModifyTransactionWindow window = new ModifyTransactionWindow
            {
                PreviousWindow = this,
                UnmodifiedTransaction = _selectedTransaction,
                ModifiedTransaction = _selectedTransaction,
                CurrentVehicle = CurrentVehicle
            };
            window.DisplayOriginalTransaction();
            window.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnModifyVehicle_Click(object sender, RoutedEventArgs e)
        {
            ModifyVehicleWindow window = new ModifyVehicleWindow { UnmodifiedVehicle = CurrentVehicle, ModifiedVehicle = CurrentVehicle, PreviousWindow = this };
            window.DisplayOriginalVehicle();
            window.Show();
            Visibility = Visibility.Hidden;
        }

        private void BtnSearchTransactions_Click(object sender, RoutedEventArgs e)
        {
        }

        private void LVTransactions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnModifyTransaction.IsEnabled = LVTransactions.SelectedIndex >= 0;
            BtnDeleteTransaction.IsEnabled = LVTransactions.SelectedIndex >= 0;
            _selectedTransaction = LVTransactions.SelectedIndex >= 0
                ? (Transaction)LVTransactions.SelectedItem
                : new Transaction();
        }

        private void LVTransactionsColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            _sort = Functions.ListViewColumnHeaderClick(sender, _sort, LVTransactions, "#BDC7C1");
        }

        #endregion Click

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        public ManageVehicleWindow()
        {
            InitializeComponent();
        }

        private void WindowViewVehicleTransactions_Closing(object sender, CancelEventArgs e)
        {
            PreviousWindow.RefreshItemsSource();
            PreviousWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}
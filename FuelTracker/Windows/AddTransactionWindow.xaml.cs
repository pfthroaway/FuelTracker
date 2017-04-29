using Extensions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FuelTracker
{
    /// <summary>Interaction logic for AddTransaction.xaml</summary>
    public partial class AddTransactionWindow
    {
        internal ViewAccountWindow PreviousWindow { get; set; }
        internal Vehicle CurrentVehicle { get; set; }

        /// <summary>Attempts to add a Transaction to the database.</summary>
        /// <returns>Returns true if successfully added</returns>
        private async Task<bool> AddTransaction()
        {
            Transaction newTransaction = new Transaction(await AppState.GetNextTransactionIndex(),
                CurrentVehicle.VehicleID, DateTimeHelper.Parse(TransactionDate.SelectedDate), TxtStore.Text,
                Int32Helper.Parse(TxtOctane.Text), DecimalHelper.Parse(TxtDistance.Text),
                DecimalHelper.Parse(TxtGallons.Text), DecimalHelper.Parse(TxtPrice.Text),
                DecimalHelper.Parse(TxtOdometer.Text), Int32Helper.Parse(TxtRange.Text));

            return await AppState.NewTransaction(newTransaction);
        }

        /// <summary>Resets all values to default status.</summary>
        private void Reset()
        {
            TxtStore.Text = "";
            TxtOctane.Text = "";
            TxtDistance.Text = "";
            TxtGallons.Text = "";
            TxtPrice.Text = "";
            TxtOdometer.Text = "";
            TxtRange.Text = "";
        }

        /// <summary>Toggles Buttons on the Window.</summary>
        /// <param name="enabled">Should Button be enabled?</param>
        private void ToggleButtons(bool enabled)
        {
            BtnSaveAndDone.IsEnabled = enabled;
            BtnSaveAndDuplicate.IsEnabled = enabled;
            BtnSaveAndNew.IsEnabled = enabled;
        }

        #region Button-Click Methods

        private async void BtnSaveAndDone_Click(object sender, RoutedEventArgs e)
        {
            if (await AddTransaction())
                CloseWindow();
            else
                new Notification("Unable to process transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
        }

        private async void BtnSaveAndDuplicate_Click(object sender, RoutedEventArgs e)
        {
            if (!await AddTransaction())
                new Notification("Unable to process transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
        }

        private async void BtnSaveAndNew_Click(object sender, RoutedEventArgs e)
        {
            if (await AddTransaction())
            {
                Reset();
                TxtStore.Focus();
            }
            else
                new Notification("Unable to process transaction.", "Finances", NotificationButtons.OK, this).ShowDialog();
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

        #region Input Manipulation

        /// <summary>Checks whether or not the Save buttons should be enabled.</summary>
        private void TextChanged()
        {
            ToggleButtons(TransactionDate.SelectedDate != null && TxtStore.Text.Length > 0 && TxtOctane.Text.Length > 0 && (TxtDistance.Text.Length > 0 | TxtOdometer.Text.Length > 0) && TxtGallons.Text.Length > 0 && TxtPrice.Text.Length > 0);
        }

        private void Txt_GotFocus(object sender, RoutedEventArgs e)
        {
            Functions.TextBoxGotFocus(sender);
        }

        private void Decimal_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Decimals);
        }

        private void Txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged();
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            TextChanged();
        }

        private void String_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Letters);
        }

        private void Integer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Functions.PreviewKeyDown(e, KeyType.Integers);
        }

        #endregion Input Manipulation

        #region Window-Manipulation Methods

        /// <summary>Closes the Window.</summary>
        private void CloseWindow()
        {
            Close();
        }

        public AddTransactionWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            PreviousWindow.RefreshItemsSource();
            PreviousWindow.Show();
        }

        #endregion Window-Manipulation Methods
    }
}
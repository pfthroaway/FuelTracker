using FuelTracker.Classes;
using System.Windows;

namespace FuelTracker.Pages.Transactions
{
    /// <summary>Interaction logic for SearchTransactionsResultsPage.xaml</summary>
    public partial class SearchTransactionsResultsPage
    {
        public SearchTransactionsResultsPage()
        {
            InitializeComponent();
        }

        private void SearchTransactionsResultsPage_OnLoaded(object sender, RoutedEventArgs e) => AppState.CalculateScale(Grid);
    }
}
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FuelTracker.Windows.Transactions
{
    /// <summary>
    /// Interaction logic for SearchTransactionsWindow.xaml
    /// </summary>
    public partial class SearchTransactionsWindow : Window
    {
        public SearchTransactionsWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
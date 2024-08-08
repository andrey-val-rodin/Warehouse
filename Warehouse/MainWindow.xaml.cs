﻿using System.Data;
using System.Windows;
using System.Windows.Media;
using Warehouse.Model;
using Warehouse.ViewModel;

namespace Warehouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        private void DataGrid_LoadingRow(object sender, System.Windows.Controls.DataGridRowEventArgs e)
        {
            var row = e.Row.DataContext as DataRowView;
            if (row != null && sender == ComponentsDataGrid)
            {
                var component = Component.FromDataRow(row.Row);
                if (component.Amount < 1)
                    e.Row.Background = new SolidColorBrush(Colors.LightYellow);
            }
        }
    }
}
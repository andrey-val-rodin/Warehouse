using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Warehouse.Database;
using Warehouse.Model;
using Warehouse.View;
using Warehouse.ViewModel;

namespace Warehouse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush _yellowBrush = new(Colors.LightYellow);
        private static readonly SolidColorBrush _whiteBrush = new(Colors.White);

        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }

        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();
        private MainViewModel Model => DataContext as MainViewModel;

        private void ComponentsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row.DataContext as DataRowView;
            if (row != null && sender == ComponentsDataGrid)
            {
                var component = Component.FromDataRow(row.Row);
                e.Row.Background = component.Remainder < 1 ? _yellowBrush : _whiteBrush;
            }
        }

        private void ComponentsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowDialog(ComponentsDataGrid, Model?.ComponentViewModel);
        }

        private void ProductComponentsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var row = e.Row.DataContext as DataRowView;
            if (row != null && sender == ProductComponentsDataGrid)
            {
                var productComponent = ProductComponent.FromDataRow(row.Row);
                e.Row.Background = productComponent.Remainder < productComponent.Required
                    ? _yellowBrush
                    : _whiteBrush;
            }
        }

        private void ProductComponentsDataGridCell_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowDialog(ProductComponentsDataGrid, Model?.ProductComponentViewModel);
        }

        private void ShowDialog(DataGrid source, TabViewModel model)
        {
            if (source.SelectedItem is not DataRowView row)
                return;

            // Instantiate the dialog box
            var dlg = new UpdateComponentDialog();

            // Configure the dialog box
            var c = Component.FromDataRow(row.Row);
            var originalComponent = (Component)c.Clone();

            dlg.Owner = this;
            dlg.Title = c.Name;
            dlg.Component = c;

            // Open the dialog box modally
            if (dlg.ShowDialog() is true)
            {
                bool hasChanges =
                    dlg.Component.Amount != originalComponent.Amount ||
                    dlg.Component.Price != originalComponent.Price ||
                    dlg.Component.Ordered != originalComponent.Ordered ||
                    dlg.Component.Details != originalComponent.Details;

                if (hasChanges)
                {
                    SqlProvider.UpdateComponent(dlg.Component);
                    model?.Refresh();
                    source.SelectedItem = null;
                }
            }
        }
    }
}
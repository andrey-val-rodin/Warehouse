using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private static readonly SolidColorBrush _yellowBrush = new(Colors.Yellow);
        private static readonly SolidColorBrush _whiteBrush = new(Colors.White);
        private static readonly SolidColorBrush _redBrush = new(Colors.Red);
        private static readonly SolidColorBrush _blackBrush = new(Colors.Black);

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
            var component = e.Row.DataContext as Component;
            if (component != null)
            {
                bool lackOfComponents = component.Remainder < 1;
                bool overdueDate = DateTime.Now > component.ExpectedDate;

                // Check remainder
                e.Row.Background = lackOfComponents || overdueDate ? _yellowBrush : _whiteBrush;

                // Check expected date
                e.Row.Foreground = overdueDate ? _redBrush : _blackBrush;
            }
        }

        private void ComponentsDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowDialog(ComponentsDataGrid, Model?.ComponentViewModel);
        }

        private void ProductComponentsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var productComponent = e.Row.DataContext as ProductComponent;
            if (productComponent != null)
            {
                bool lackOfComponents = productComponent.Remainder < 1;
                bool overdueDate = DateTime.Now > productComponent.ExpectedDate;

                // Check remainder
                e.Row.Background = lackOfComponents || overdueDate ? _yellowBrush : _whiteBrush;

                // Check expected date
                e.Row.Foreground = overdueDate ? _redBrush : _blackBrush;
            }
        }

        private void ProductComponentsDataGridCell_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowDialog(ProductComponentsDataGrid, Model?.ProductComponentViewModel);
        }

        private void ShowDialog(DataGrid source, TabViewModel model)
        {
            if (source.SelectedItem is not Component c)
                return;

            var originalComponent = (Component)c.Clone();

            // Instantiate the dialog box
            var dlg = new UpdateComponentDialog
            {
                Owner = this,
                Title = c.Name,
                // We have to clone original component to prevent changing component in DataGrid
                Component = (Component)c.Clone()
            };

            // Open the dialog box modally
            if (dlg.ShowDialog() is true)
            {
                bool hasChanges =
                    dlg.Component.Amount != originalComponent.Amount ||
                    dlg.Component.Price != originalComponent.Price ||
                    dlg.Component.Ordered != originalComponent.Ordered ||
                    dlg.Component.ExpectedDate != originalComponent.ExpectedDate ||
                    dlg.Component.Details != originalComponent.Details;

                if (hasChanges)
                {
                    SqlProvider.UpdateComponent(dlg.Component);
                    model?.Refresh(dlg.Component);
                }
            }
        }

        private void ComponentsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowDialog(ComponentsDataGrid, Model?.ComponentViewModel);
                e.Handled = true;
            }
        }

        private void ProductComponentsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowDialog(ProductComponentsDataGrid, Model?.ProductComponentViewModel);
                e.Handled = true;
            }
        }
    }
}
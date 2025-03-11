using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Warehouse.Database;
using Warehouse.Model;
using Warehouse.Tools;
using Warehouse.View;
using Warehouse.ViewModel;
using ListSortDirection = System.ComponentModel.ListSortDirection;

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

        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();
        private static Sender Sender { get; } = new Sender();
        private MainViewModel Model => DataContext as MainViewModel;
        private TabViewModel CurrentChildModel { get; set; }

        private void ComponentsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ShowComponentDialog(ComponentsDataGrid, Model.ComponentViewModel);
        }

        private void ProductComponentsDataGridCell_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowComponentDialog(ProductComponentsDataGrid, Model.ProductComponentViewModel);
        }

        private void ShowComponentDialog(DataGrid source, TabViewModel model)
        {
            if (source.SelectedItem is not Component c)
                return;

            var originalComponent = c;

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
                    if (dlg.Component.Price != originalComponent.Price)
                        SqlProvider.UpdateAllUnitPrices();

                    model.Refresh(dlg.Component);
                    source.ScrollIntoView(dlg.Component);
                }
            }
        }

        private Fabrication ShowFabricationDialog(Fabrication fabrication)
        {
            bool isEditing = fabrication != null;
            Fabrication originalFabrication = fabrication;

            // Instantiate the dialog box
            var dlg = new FabricationDialog(isEditing ? (Fabrication)fabrication.Clone() : null)
            {
                Title = isEditing ? "Производство" : "Новое производство",
                Owner = this
            };

            // Open the dialog box modally
            if (dlg.ShowDialog() is true)
            {
                bool hasChanges = originalFabrication == null ||
                    dlg.Fabrication.ProductId != originalFabrication.ProductId ||
                    dlg.Fabrication.ProductName != originalFabrication.ProductName ||
                    dlg.Fabrication.Status != originalFabrication.Status ||
                    dlg.Fabrication.Number != originalFabrication.Number ||
                    dlg.Fabrication.TableId != originalFabrication.TableId ||
                    dlg.Fabrication.Client != originalFabrication.Client ||
                    dlg.Fabrication.Details != originalFabrication.Details ||
                    dlg.Fabrication.ExpectedDate != originalFabrication.ExpectedDate;

                // just to be sure...
                if (dlg.IsReadonly)
                    hasChanges = false;

                if (hasChanges)
                    return dlg.Fabrication;
            }

            return null;
        }

        private void ComponentsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowComponentDialog(ComponentsDataGrid, Model.ComponentViewModel);
                e.Handled = true;
            }
        }

        private void ProductComponentsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ShowComponentDialog(ProductComponentsDataGrid, Model.ProductComponentViewModel);
                e.Handled = true;
            }
        }

        private void FabricationsDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            SortColumn(FabricationsDataGrid, e);
        }

        private void ComponentsDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            SortColumn(ComponentsDataGrid, e);
        }

        private void ProductComponentsDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            SortColumn(ProductComponentsDataGrid, e);
        }

        private void SortColumn(DataGrid source, DataGridSortingEventArgs e)
        {
            DataGridColumn column = e.Column;

            // custom sort for column
            if (column.SortMemberPath == "ExpectedDate" ||
                column.SortMemberPath == "Ordered" ||
                column.SortMemberPath == "Price" ||
                column.SortMemberPath == "StartedDate" ||
                column.SortMemberPath == "ClosedDate")
            {
                // Prevent auto sorting
                e.Handled = true;

                // sort direction
                column.SortDirection = (column.SortDirection != ListSortDirection.Ascending)
                    ? ListSortDirection.Ascending
                    : ListSortDirection.Descending;

                // comparer
                var lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(source.ItemsSource);
                var comparer = CreateComparer(column.SortMemberPath, column.SortDirection.Value);
                lcv.CustomSort = comparer;
            }
        }

        private IComparer CreateComparer(string sortMemberPath, ListSortDirection direction)
        {
            switch (sortMemberPath)
            {
                case "ExpectedDate":
                    return TabControl.SelectedItem == FabricationTab
                        ? new FabricationDateComparer(direction, sortMemberPath)
                        : new ComponentDateComparer(direction);
                case "StartedDate":
                case "ClosedDate":
                    return new FabricationDateComparer(direction, sortMemberPath);
                case "Ordered":
                    return new ComponentOrderedComparer(direction);
                case "Price":
                    return new ComponentPriceComparer(direction);
                default:
                    throw new InvalidOperationException("Unsupported sortMemberPath");
            }
        }

        private void ComponentTab_Selected(object sender, RoutedEventArgs e)
        {
            CurrentChildModel = Model.ComponentViewModel;
            Model.ComponentViewModel.Update();
        }

        private void ProductTab_Selected(object sender, RoutedEventArgs e)
        {
            CurrentChildModel = Model.ProductComponentViewModel;
            Model.ProductComponentViewModel.Update();
        }

        private void FabricationTab_Selected(object sender, RoutedEventArgs e)
        {
            CurrentChildModel = Model.FabricationViewModel;
            Model.FabricationViewModel.Update();
        }

        private void NewFabricationButton_Click(object sender, RoutedEventArgs e)
        {
            var fabrication = ShowFabricationDialog(null);
            if (fabrication != null)
            {
                fabrication.StartedDate = DateTime.Now.Date;
                SqlProvider.InsertFabrication(fabrication);
                SqlProvider.AddProductAmountsInUse(fabrication.ProductId);
                Model.FabricationViewModel.OnInsertNewFabrication(fabrication);
                Task.Run(async () => await Sender.PostInfoAsync(fabrication));
            }
        }

        private void FabricationsDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var fabrication = Model.FabricationViewModel.CurrentFabrication;
                if (fabrication != null)
                {
                    EditFabrication(fabrication);
                    e.Handled = true;
                }
            }
        }

        private void FabricationsDataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fabrication = Model.FabricationViewModel.CurrentFabrication;
            if (fabrication != null)
            {
                EditFabrication(fabrication);
                e.Handled = true;
            }
        }

        private void EditFabrication(Fabrication fabrication)
        {
            var changedFabrication = ShowFabricationDialog(fabrication);
            if (changedFabrication != null)
            {
                if (changedFabrication.Status != fabrication.Status)
                {
                    switch (changedFabrication.Status)
                    {
                        case FabricationStatus.Closed:
                            changedFabrication.ClosedDate = DateTime.Now.Date;
                            SqlProvider.UpdateFabrication(changedFabrication);
                            SqlProvider.SubtractProductAmounts(changedFabrication.ProductId);
                            SqlProvider.SubtractProductAmountsInUse(changedFabrication.ProductId);
                            Model.FabricationViewModel.Update();
                            Task.Run(async () => await Sender.PostInfoAsync(changedFabrication));
                            break;
                        case FabricationStatus.Cancelled:
                            changedFabrication.ClosedDate = DateTime.Now.Date;
                            SqlProvider.UpdateFabrication(changedFabrication);
                            SqlProvider.SubtractProductAmountsInUse(changedFabrication.ProductId);
                            Model.FabricationViewModel.Update();
                            Task.Run(async () => await Sender.PostInfoAsync(changedFabrication));
                            break;
                        default:
                            throw new InvalidOperationException("Invalid FabricationStatus");
                    }
                }
                else
                {
                    SqlProvider.UpdateFabrication(changedFabrication);
                    Model.FabricationViewModel.Refresh(changedFabrication);
                    FabricationsDataGrid.ScrollIntoView(changedFabrication);
                }
            }
        }

        private void Copy(object sender, ExecutedRoutedEventArgs e)
        {
            CurrentChildModel?.Copy();
        }
    }
}
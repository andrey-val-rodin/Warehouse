﻿using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Warehouse.Database;
using Warehouse.Model;

namespace Warehouse.View
{
    /// <summary>
    /// Логика взаимодействия для FabricationDialog.xaml
    /// </summary>
    public partial class FabricationDialog : Window
    {
        private readonly bool _isReadonly;
        private readonly bool _isCreating;
        private readonly FabricationStatus _originalStatus;
        private readonly Product[] _products;

        public FabricationDialog(Fabrication fabrication = null)
        {
            _products = SqlProvider.GetProducts().ToArray();
            TableIdValidationRule.Products = _products;

            InitializeComponent();

            datePicker.DisplayDateStart = DateTime.Now;
            datePicker.DisplayDateEnd = datePicker.DisplayDateStart + TimeSpan.FromDays(180);

            if (fabrication == null)
            {
                _isCreating = true;
                fabrication = new Fabrication();
                // First selection in ProductComboBox
                fabrication.ProductId = 1;
            }

            _originalStatus = fabrication.Status;
            Fabrication = fabrication;
            _isReadonly = Fabrication.Status != FabricationStatus.Opened;

            PrepareProducts();
            PrepareStatuses();
            PrepareTableId();

            SetControlStates();
        }

        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();

        public Fabrication Fabrication
        {
            get { return (Fabrication)DataContext; }
            set
            {
                DataContext = value;
                TableIdValidationRule.Fabrication = value;
            }
        }

        public bool IsReadonly => _isReadonly;
        public bool IsCreating => _isCreating;
        public bool IsEditing => !_isCreating;
        private bool IsBluetoothTable() => TableIdValidationRule.IsBluetoothTable(Fabrication.ProductId);

        private void PrepareProducts()
        {
            var ProductNames = SqlProvider.GetProductNames();
            foreach (var productName in ProductNames)
            {
                productComboBox.Items.Add(productName);
            }

            productComboBox.SelectedIndex = Fabrication.ProductId - 1;
            productComboBox.IsEnabled = IsCreating;
        }

        private void PrepareStatuses()
        {
            statusComboBox.Items.Add("Открыто");
            statusComboBox.Items.Add("Закрыто");
            statusComboBox.Items.Add("Отменено");
            if (IsCreating)
            {
                statusComboBox.SelectedIndex = 0; // Открыто
                statusComboBox.IsEnabled = false;
            }
            else
            {
                switch (Fabrication.Status)
                {
                    case FabricationStatus.Opened:
                        statusComboBox.SelectedIndex = 0; // Открыто
                        statusComboBox.IsEnabled = true;
                        break;
                    case FabricationStatus.Closed:
                        statusComboBox.SelectedIndex = 1; // Закрыто
                        statusComboBox.IsEnabled = false;
                        break;
                    case FabricationStatus.Cancelled:
                        statusComboBox.SelectedIndex = 2; // Отменено
                        statusComboBox.IsEnabled = false;
                        break;
                    default:
                        throw new InvalidOperationException("Invalid FabricationStatus");
                }
            }
        }

        private void PrepareTableId()
        {
            var isBluetoothTable = IsBluetoothTable();
            tableIdTextBox.IsEnabled = isBluetoothTable;
        }

        private void SetControlStates()
        {
            if (_isReadonly)
            {
                // Disable all
                productLabel.IsEnabled = false;
                productComboBox.IsEditable = false;
                statusLabel.IsEnabled = false;
                statusComboBox.IsEnabled = false;
                datePicker.IsEnabled = false;
                numberLabel.IsEnabled = false;
                numberTextBox.IsReadOnly = true;
                tableIdLabel.IsEnabled = false;
                tableIdTextBox.IsReadOnly = true;
                clientLabel.IsEnabled = false;
                clientTextBox.IsReadOnly = true;
                detailsLabel.IsEnabled = false;
                detailsTextBox.IsReadOnly = true;
            }
            else
            {
                if (Fabrication.IsUnit)
                {
                    numberLabel.IsEnabled = false;
                    numberTextBox.IsEnabled = false;
                    tableIdLabel.IsEnabled = false;
                    tableIdTextBox.IsEnabled = false;
                    numberLabel.IsEnabled = false;
                    numberTextBox.IsEnabled = false;
                }
                else
                {
                    numberLabel.IsEnabled = true;
                    numberTextBox.IsEnabled = true;
                    if (IsBluetoothTable())
                    {
                        tableIdLabel.IsEnabled = true;
                        tableIdTextBox.IsEnabled = true;
                    }
                    else
                    {
                        tableIdLabel.IsEnabled = false;
                        tableIdTextBox.IsEnabled = false;
                    }
                    numberLabel.IsEnabled = true;
                    numberTextBox.IsEnabled = true;
                }
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Dialog box canceled
            DialogResult = false;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            // Fire validation rule
            tableIdTextBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            // Don't accept the dialog box if there is invalid data
            if (!IsValid(this))
                return;

            // Validate TableId
            if (!IsBluetoothTable())
                Fabrication.TableId = null;

            if (IsEditing)
            {
                if (Fabrication.Status != _originalStatus)
                {
                    switch (Fabrication.Status)
                    {
                        case FabricationStatus.Closed:
                            var missingComponents = SqlProvider.GetMissingComponents(Fabrication.ProductId, true);
                            if (missingComponents.Count() > 0)
                            {
                                MessageBox.Show(GetMissingInfo(missingComponents),
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                return;
                            }

                            if (MessageBox.Show("Все использованные компоненты будут вычтены из поля 'Наличие'.\n\nПродолжить?",
                                "Закрытие производства", MessageBoxButton.YesNo) == MessageBoxResult.No)
                                return;
                            break;
                        case FabricationStatus.Cancelled:
                            if (MessageBox.Show("Все используемые компоненты будут возвращены.\n\nПродолжить?",
                                "Отмена производства", MessageBoxButton.YesNo) == MessageBoxResult.No)
                                return;
                            break;
                    }
                }
            }

            // Dialog box accepted
            if (!IsReadonly)
            {
                // Validate Client and Details
                if (string.IsNullOrWhiteSpace(Fabrication.Client))
                    Fabrication.Client = null;
                if (string.IsNullOrWhiteSpace(Fabrication.Details))
                    Fabrication.Details = null;
            }

            DialogResult = true;
        }

        private static string GetMissingInfo(IEnumerable<Component> components)
        {
            var result = new StringBuilder();
            result.AppendLine("Для закрытия производства в базе не хватает следующих комплектующих:");
            foreach (var c in components)
            {
                result.AppendLine($"  • {c.Name}");
            }

            return result.ToString();
        }

        // Validate all dependency objects in a window
        private bool IsValid(DependencyObject node)
        {
            // Check if dependency object was passed
            if (node != null)
            {
                // Check if dependency object is valid.
                // NOTE: Validation.GetHasError works for controls that have validation rules attached 
                var isValid = !Validation.GetHasError(node);
                if (!isValid)
                {
                    // If the dependency object is invalid, and it can receive the focus,
                    // set the focus
                    if (node is IInputElement)
                        Keyboard.Focus((IInputElement)node);

                    return false;
                }
            }

            // If this dependency object is valid, check all child dependency objects
            if (!LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>().All(IsValid))
                return false;

            // All dependency objects are valid
            return true;
        }

        private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Fabrication.ProductId = productComboBox.SelectedIndex + 1;
            Fabrication.ProductName = productComboBox.SelectedItem as string;
            Fabrication.IsUnit = _products[productComboBox.SelectedIndex].IsUnit;
            Fabrication.Number = Fabrication.IsUnit ? 0 : SqlProvider.GetMaxFabricationNumber() + 1;
            numberTextBox.Text = Fabrication.Number.ToString();
            PrepareTableId();
            SetControlStates();
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Fabrication.Status = (FabricationStatus)statusComboBox.SelectedIndex;
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Warehouse.Model;

namespace Warehouse.View
{
    /// <summary>
    /// Логика взаимодействия для UpdateComponentDialog.xaml
    /// </summary>
    public partial class UpdateComponentDialog : Window
    {
        public UpdateComponentDialog()
        {
            InitializeComponent();
            datePicker.DisplayDateStart = DateTime.Now;
            datePicker.DisplayDateEnd = datePicker.DisplayDateStart + TimeSpan.FromDays(180);
        }

        public Component Component
        {
            get { return (Component)DataContext; }
            set
            {
                // Validate component
                if (value.Ordered == null)
                    value.ExpectedDate = null;

                DataContext = value;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Dialog box canceled
            DialogResult = false;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            // Don't accept the dialog box if there is invalid data
            if (!IsValid(this))
                return;

            // Dialog box accepted
            DialogResult = true;
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
                    if (node is IInputElement) Keyboard.Focus((IInputElement)node);
                    return false;
                }
            }

            // If this dependency object is valid, check all child dependency objects
            return LogicalTreeHelper.GetChildren(node).OfType<DependencyObject>().All(IsValid);

            // All dependency objects are valid
        }

        private void ConfirmReceiving_Click(object sender, RoutedEventArgs e)
        {
            if (Component.Ordered == null)
                return;

            var component = (Component)Component.Clone();
            component.Amount += Component.Ordered.Value;
            component.Ordered = null;
            Component = component;
        }

        private void CancelOrder_Click(object sender, RoutedEventArgs e)
        {
            var component = (Component)Component.Clone();
            component.Ordered = null;
            Component = component;
        }
    }
}

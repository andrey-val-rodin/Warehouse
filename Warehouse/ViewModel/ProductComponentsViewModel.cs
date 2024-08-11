using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Windows;
using Warehouse.Database;

namespace Warehouse.ViewModel
{
    public class ProductComponentsViewModel : TabViewModel
    {
        private int _currentProductIndex;
        private DataRowView _currentProductComponent;

        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();

        public int CurrentProductIndex
        {
            get { return _currentProductIndex; }
            set
            {
                if (SetProperty(ref _currentProductIndex, value))
                    RaisePropertyChanged(nameof(ProductComponents));
            }
        }

        public DataRowView CurrentProductComponent
        {
            get { return _currentProductComponent; }
            set
            {
                SetProperty(ref _currentProductComponent, value);
            }
        }

        public DataView ProductComponents => SqlProvider.GetProductComponents(CurrentProductIndex + 1);

        public string[] ProductNames => SqlProvider.GetProductNames();

        public override void Refresh()
        {
            RaisePropertyChanged(nameof(ProductComponents));
        }
    }
}

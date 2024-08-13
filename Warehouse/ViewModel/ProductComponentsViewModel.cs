using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using Warehouse.Database;
using Warehouse.Model;

namespace Warehouse.ViewModel
{
    public class ProductComponentsViewModel : TabViewModel
    {
        private int _currentProductIndex;
        private Component _currentProductComponent;
        private ObservableCollection<ProductComponent> _productComponents;

        public ProductComponentsViewModel()
        {
            _productComponents = new ObservableCollection<ProductComponent>(SqlProvider.GetProductComponents(1));
        }

        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();

        public int CurrentProductIndex
        {
            get { return _currentProductIndex; }
            set
            {
                if (SetProperty(ref _currentProductIndex, value))
                {
                    _productComponents = new ObservableCollection<ProductComponent>(SqlProvider.GetProductComponents(value + 1));
                    RaisePropertyChanged(nameof(ProductComponents));
                    RaisePropertyChanged(nameof(Price));
                }
            }
        }

        public Component CurrentProductComponent
        {
            get => _currentProductComponent;
            set
            {
                SetProperty(ref _currentProductComponent, value);
            }
        }

        public decimal Price => SqlProvider.GetProductPrice(CurrentProductIndex + 1);

        public ObservableCollection<ProductComponent> ProductComponents => _productComponents;

        public string[] ProductNames => SqlProvider.GetProductNames();

        public override void Refresh<T>(T component)
        {
            var comp = component as ProductComponent;
            var index = ProductComponents.IndexOf(ProductComponents.First(c => c.Id == comp.Id));
            ProductComponents[index] = comp;
            CurrentProductComponent = comp;
            RaisePropertyChanged(nameof(Price));
        }
    }
}

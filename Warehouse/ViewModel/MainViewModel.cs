using System.Collections.ObjectModel;

namespace Warehouse.ViewModel
{
    public class MainViewModel : NotifyPropertyChangedImpl
    {
        private readonly ObservableCollection<NotifyPropertyChangedImpl> _children;

        public MainViewModel()
        {
            _children = [new OrdersViewModel(), new ProductsViewModel(), new ComponentsViewModel()];
        }

        public ObservableCollection<NotifyPropertyChangedImpl> Children => _children;

        public OrdersViewModel OrderViewModel { get => (OrdersViewModel)_children[0]; }
        public ProductsViewModel ProductViewModel { get => (ProductsViewModel)_children[1]; }
        public ComponentsViewModel ComponentViewModel { get => (ComponentsViewModel)_children[2]; }
    }
}

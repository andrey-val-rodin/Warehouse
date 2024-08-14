using System.Collections.ObjectModel;

namespace Warehouse.ViewModel
{
    public class MainViewModel : NotifyPropertyChangedImpl
    {
        private readonly ObservableCollection<TabViewModel> _children;

        public MainViewModel()
        {
            _children = [
                new FabricationsViewModel(),
                new ComponentsViewModel(),
                new ProductComponentsViewModel()];
        }

        public ObservableCollection<TabViewModel> Children => _children;

        public FabricationsViewModel FabricationViewModel { get => (FabricationsViewModel)_children[0]; }
        public ComponentsViewModel ComponentViewModel { get => (ComponentsViewModel)_children[1]; }
        public ProductComponentsViewModel ProductComponentViewModel { get => (ProductComponentsViewModel)_children[2]; }
    }
}

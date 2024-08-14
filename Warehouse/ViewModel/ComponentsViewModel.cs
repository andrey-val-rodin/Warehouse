using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using Warehouse.Database;
using Warehouse.Model;

namespace Warehouse.ViewModel
{
    public class ComponentsViewModel : TabViewModel
    {
        private int _currentType;
        private Component _currentComponent;
        private ObservableCollection<Component> _components;

        public ComponentsViewModel()
        {
            Update();
        }

        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();

        public int CurrentType
        {
            get { return _currentType; }
            set
            {
                if (SetProperty(ref _currentType, value))
                {
                    Update();
                }
            }
        }

        public Component CurrentComponent
        {
            get => _currentComponent;
            set
            {
                SetProperty(ref _currentComponent, value);
            }
        }

        public ObservableCollection<Component> Components => _components;

        public string[] ComponentTypes
        {
            get
            {
                var result = SqlProvider.GetComponentTypes();
                return result.Prepend("Все").ToArray();
            }
        }

        public override void Refresh<T>(T component)
        {
            var comp = component as Component;
            var index = Components.IndexOf(Components.First(c => c.Id == comp.Id));
            Components[index] = comp;
            CurrentComponent = comp;
        }

        public override void Update()
        {
            _components = new ObservableCollection<Component>(SqlProvider.GetComponents(CurrentType));
            RaisePropertyChanged(nameof(Components));
        }
    }
}

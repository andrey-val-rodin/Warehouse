using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Windows;
using Warehouse.Database;

namespace Warehouse.ViewModel
{
    public class ComponentsViewModel : NotifyPropertyChangedImpl
    {
        private int _currentType;

        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();

        public int CurrentType
        {
            get {  return _currentType; }
            set
            {
                if (SetProperty(ref _currentType, value))
                    RaisePropertyChanged(nameof(Components));
            }
        }

        public DataView Components => SqlProvider.GetComponents(CurrentType);

        public string[] ComponentTypes
        {
            get
            {
                var result = SqlProvider.GetComponentTypes();
                return result.Prepend("Все").ToArray();
            }
        }
    }
}

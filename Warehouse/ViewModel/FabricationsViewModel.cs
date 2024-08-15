using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using Warehouse.Database;
using Warehouse.Model;

namespace Warehouse.ViewModel
{
    public class FabricationsViewModel : TabViewModel
    {
        private int _currentModeIndex;
        private Fabrication _currentFabrication;
        private ObservableCollection<Fabrication> _fabrications;
        private static readonly string[] _modeNames = ["Активные производства", "История"];

        public FabricationsViewModel()
        {
            Update();
        }

        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();

        public int CurrentModeIndex
        {
            get { return _currentModeIndex; }
            set
            {
                if (SetProperty(ref _currentModeIndex, value))
                {
                    Update();
                }
            }
        }

        public Fabrication CurrentFabrication
        {
            get => _currentFabrication;
            set
            {
                SetProperty(ref _currentFabrication, value);
            }
        }

        public ObservableCollection<Fabrication> Fabrications => _fabrications;

        public string[] ModeNames => _modeNames;

        public override void Refresh<T>(T position)
        {
        }

        public override void Update()
        {
            _fabrications = new ObservableCollection<Fabrication>(
                CurrentModeIndex == 0
                    ? SqlProvider.GetOpenedFabrications()
                    : SqlProvider.GetHistoricalFabrications());
            RaisePropertyChanged(nameof(Fabrications));
        }
    }
}

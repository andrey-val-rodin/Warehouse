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
                    RaisePropertyChanged(nameof(ActiveFabricationsVisibility));
                    RaisePropertyChanged(nameof(HistoricalFabricationsVisibility));
                }
            }
        }

        public bool IsActiveFabrications => CurrentModeIndex == 0;
        public bool IsHistoricalFabrications => CurrentModeIndex == 1;

        public Visibility ActiveFabricationsVisibility => IsActiveFabrications ? Visibility.Visible : Visibility.Collapsed;
        public Visibility HistoricalFabricationsVisibility => IsHistoricalFabrications ? Visibility.Visible : Visibility.Collapsed;

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

        public override void Refresh<T>(T fabrication)
        {
            var f = fabrication as Fabrication;
            var index = Fabrications.IndexOf(Fabrications.First(c => c.Id == f.Id));
            Fabrications[index] = f;
            CurrentFabrication = f;
        }

        public override void Update()
        {
            _fabrications = new ObservableCollection<Fabrication>(
                IsActiveFabrications
                    ? SqlProvider.GetOpenedFabrications()
                    : SqlProvider.GetHistoricalFabrications());
            RaisePropertyChanged(nameof(Fabrications));
        }

        public void OnInsertNewFabrication(Fabrication fabrication)
        {
            if (IsHistoricalFabrications)
            {
                // will call Update
                CurrentModeIndex = 0;
            }
            else
            {
                Update();
            }
            var index = Fabrications.IndexOf(Fabrications.First(c => c.Id == fabrication.Id));
            Fabrications[index] = fabrication;
            CurrentFabrication = fabrication;
        }
    }
}

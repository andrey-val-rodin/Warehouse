namespace Warehouse.ViewModel
{
    public class ProductsViewModel : TabViewModel
    {
        private int _sliderValue;//TODO remove

        public ProductsViewModel()
        {
            SliderValue = 33;
        }

        public int SliderValue
        {
            get { return _sliderValue; }
            set
            {
                SetProperty(ref _sliderValue, value);
            }
        }

        public override void Refresh<T>(T position)
        {
            throw new NotImplementedException();
        }
    }
}

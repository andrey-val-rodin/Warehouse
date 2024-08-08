namespace Warehouse.ViewModel
{
    public class ProductsViewModel : NotifyPropertyChangedImpl
    {
        private int _sliderValue;

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
    }
}

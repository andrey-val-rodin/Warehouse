﻿namespace Warehouse.ViewModel
{
    public abstract class TabViewModel : NotifyPropertyChangedImpl
    {
        public abstract void Refresh<T>(T updatedItem) where T: class;
    }
}

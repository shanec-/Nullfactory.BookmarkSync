using System;
using System.ComponentModel;
using GalaSoft.MvvmLight;

namespace Nullfactory.BookmarkSync.ViewModel
{
    public class ItemViewModel : ViewModelBase
    {
        private string _Title;
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                if (value != _Title)
                {
                    _Title = value;
                    RaisePropertyChanged("Title");
                }
            }
        }

        private string _Url;
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                if (value != _Url)
                {
                    _Url = value;
                    RaisePropertyChanged("Url");
                }
            }
        }
    }
}
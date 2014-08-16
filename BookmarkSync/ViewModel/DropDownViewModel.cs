using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight;

namespace Nullfactory.BookmarkSync.ViewModel
{
    public class DropDownViewModel : ViewModelBase
    {
        public const string DisplayValuePropertyName = "DisplayValue";
        private string _DisplayValue = string.Empty;
        
        public string DisplayValue
        {
            get
            {
                return _DisplayValue;
            }

            set
            {
                if (_DisplayValue == value)
                {
                    return;
                }

                var oldValue = _DisplayValue;
                _DisplayValue = value;

                RaisePropertyChanged(DisplayValuePropertyName);
            }
        }

        public const string ValuePropertyName = "Value";
        private string _Value = string.Empty;

        public string Value
        {
            get
            {
                return _Value;
            }

            set
            {
                if (_Value == value)
                {
                    return;
                }

                var oldValue = _Value;
                _Value = value;

                RaisePropertyChanged(ValuePropertyName);
            }
        }
    }
}

using GalaSoft.MvvmLight.Command;
using System;

namespace Nullfactory.PhoneClient.Helpers
{
    public class AsyncRelayCommand : RelayCommand
    {
        public AsyncRelayCommand(Action action) : base(() => { BackgroundHelper.Execute(action); })
        {
        }

        public AsyncRelayCommand(Action action, Func<bool> canExecute) 
            : base (()=> {BackgroundHelper.Execute(action); }, canExecute)
        {
        }
    }
}

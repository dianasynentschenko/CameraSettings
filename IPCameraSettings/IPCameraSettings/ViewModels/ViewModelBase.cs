using IPCameraSettings.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IPCameraSettings.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, value))
            { 
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set => SetProperty(ref errorMessage, value);
        }

        private bool isBusy;
        public bool IsBusy
        { 
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        protected async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
        {
            if (IsBusy) throw new InvalidOperationException("Operation already in progress.");
            IsBusy = true;
            ErrorMessage = null;

            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return default;
            }
            finally
            { 
                IsBusy = false;
            }

        }
    }
}

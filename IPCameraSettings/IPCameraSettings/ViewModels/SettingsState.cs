using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCameraSettings.ViewModels
{
    public enum SettingsState
    {
        Idle,
        LoadingSettings,
        SavingSettings,
        Loaded,
        Error
    }
}

using Presentation.Models;

namespace Presentation.Helpers;

public interface IToastHelper
{
    void ShowAlarmToast(string title, string content);
    void ShowAlarmToastWithSnooze<T>(string title, string content) where T : IAlarmModel;
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Oms.Client.Modules.OrderEditor;

namespace Oms.Client.Services
{
    public interface IWindowService
    {
        Task ShowDialogAsync(object viewModel);
    }

    /// <summary>Implemented by view models that can ask their window to close.</summary>
    public interface IRequestClose
    {
        event EventHandler CloseRequested;
    }

    public class WindowService : IWindowService
    {
        private static readonly Dictionary<Type, Func<Window>> Views = new Dictionary<Type, Func<Window>>
        {
            { typeof(OrderEditorViewModel), () => new OrderEditorView() },
        };

        public Task ShowDialogAsync(object viewModel)
        {
            var window = Views[viewModel.GetType()]();
            window.DataContext = viewModel;
            if (viewModel is IRequestClose requestClose)
                requestClose.CloseRequested += (_, _) => window.Close();
            var owner = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            return window.ShowDialog(owner);
        }
    }
}

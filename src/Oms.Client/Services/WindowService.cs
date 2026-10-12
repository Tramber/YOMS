using System;
using System.Collections.Generic;
using System.Windows;
using Oms.Client.Modules.OrderEditor;

namespace Oms.Client.Services
{
    public interface IWindowService
    {
        bool? ShowDialog(object viewModel);
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

        public bool? ShowDialog(object viewModel)
        {
            var window = Views[viewModel.GetType()]();
            window.DataContext = viewModel;
            window.Owner = Application.Current.MainWindow;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (viewModel is IRequestClose requestClose)
                requestClose.CloseRequested += (_, _) => window.Close();
            return window.ShowDialog();
        }
    }
}

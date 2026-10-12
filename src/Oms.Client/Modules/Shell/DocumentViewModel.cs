using CommunityToolkit.Mvvm.ComponentModel;

namespace Oms.Client.Modules.Shell
{
    /// <summary>A view model shown as a tab in the shell.</summary>
    public abstract class DocumentViewModel : ObservableObject
    {
        public abstract string DisplayName { get; }
    }
}

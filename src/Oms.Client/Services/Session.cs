using CommunityToolkit.Mvvm.ComponentModel;

namespace Oms.Client.Services
{
    /// <summary>Who is using the client. The server has no authentication yet: the login is sent as the token of every request.</summary>
    public partial class Session : ObservableObject
    {
        [ObservableProperty]
        private string _login = "trader";
    }
}

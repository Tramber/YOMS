using System;
using System.Collections.Generic;

namespace Oms.Client.Framework.Themes
{
    public class DarkTheme : ITheme
    {
        public string Name
        {
            get { return "Dark"; }
        }

        public IEnumerable<Uri> ApplicationResources
        {
            get
            {
                yield return new Uri("pack://application:,,,/Oms.Client;component/Styles/Themes/AvalonDock/DarkTheme.xaml");
                yield return new Uri("pack://application:,,,/Oms.Client;component/Styles/Themes/VS2013/DarkTheme.xaml");
            }
        }

        public IEnumerable<Uri> MainWindowResources
        {
            get { yield break; }
        }
    }
}
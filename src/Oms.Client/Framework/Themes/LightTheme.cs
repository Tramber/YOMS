using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Oms.Client.Framework.Themes
{
    [Export(typeof(ITheme))]
    public class LightTheme : ITheme
    {
        public string Name
        {
            get { return "Light"; }
        }

        public IEnumerable<Uri> ApplicationResources
        {
            get
            {
                yield return new Uri("pack://application:,,,/Oms.Client;component/Styles/Themes/AvalonDock/LightTheme.xaml");
                yield return new Uri("pack://application:,,,/Oms.Client;component/Styles/Themes/VS2013/LightTheme.xaml");
            }
        }

        public IEnumerable<Uri> MainWindowResources
        {
            get { yield break; }
        }
    }
}
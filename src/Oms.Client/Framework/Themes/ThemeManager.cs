using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

namespace Oms.Client.Framework.Themes
{
    [Export(typeof(IThemeManager))]
    public class ThemeManager : IThemeManager
    {
        public event EventHandler CurrentThemeChanged;

        private ResourceDictionary _applicationResourceDictionary;

        private readonly ITheme[] _themes;

        public IEnumerable<ITheme> Themes
        {
            get { return _themes; }
        }

        public ITheme CurrentTheme { get; private set; }

        [ImportingConstructor]
        public ThemeManager([ImportMany] ITheme[] themes)
        {
            _themes = themes;
        }

        public bool SetCurrentTheme(string name)
        {
            var theme = _themes.FirstOrDefault(x => x.Name == name);
            if (theme == null)
                return false;

            CurrentTheme = theme;

            if (_applicationResourceDictionary == null)
            {
                _applicationResourceDictionary = new ResourceDictionary();
                Application.Current.Resources.MergedDictionaries.Add(_applicationResourceDictionary);
            }
            _applicationResourceDictionary.BeginInit();
            _applicationResourceDictionary.MergedDictionaries.Clear();

            var windowResourceDictionary = Application.Current.MainWindow.Resources.MergedDictionaries[0];
            windowResourceDictionary.BeginInit();
            windowResourceDictionary.MergedDictionaries.Clear();

            foreach (var uri in theme.ApplicationResources)
                _applicationResourceDictionary.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = uri
                });

            foreach (var uri in theme.MainWindowResources)
                windowResourceDictionary.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = uri
                });

            _applicationResourceDictionary.EndInit();
            windowResourceDictionary.EndInit();

            RaiseCurrentThemeChanged(EventArgs.Empty);

            return true;
        }

        private void RaiseCurrentThemeChanged(EventArgs args)
        {
            var handler = CurrentThemeChanged;
            if (handler != null)
                handler(this, args);
        }
    }
}
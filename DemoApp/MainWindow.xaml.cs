using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace DemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DemoViewModel _viewModel;

        static MainWindow()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
        }

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new DemoViewModel();
            DataContext = _viewModel;
        }
    }
}

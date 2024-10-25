using System.Windows;

namespace IRDC
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        MainWindow _w = new MainWindow();
        DataCollector _dc = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DataViewModel vm = new DataViewModel();
            _dc = new DataCollector(vm);

            _w.DataContext = vm;
            _w.Show();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            _w.Close();
            _dc.Terminate();
        }
    }
}

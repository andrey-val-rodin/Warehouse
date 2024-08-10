using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using Warehouse.Database;

namespace Warehouse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ServiceProvider ServiceProvider { get; }

        public App()
        {
            //TODO
            //FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISqlProvider>(new SqlProvider());
            //TODO
            //services.AddSingleton<MainWindow>();
            //after this call we can add constructor MainWindow(ISqlProvider p)
            //https://stackoverflow.com/questions/54877352/dependency-injection-in-net-core-3-0-for-wpf
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //TODO: temporary path
            const string path = "C:\\Users\\andre\\OneDrive\\Desktop\\Components.db";
            if (!File.Exists(path))
            {
                MessageBox.Show($"Файл не найден\n{path}", "Ошибка открытия БД");
                Current.Shutdown();
                return;
            }

            var sqlProvider = ServiceProvider.GetService<ISqlProvider>();
            if (!sqlProvider.Connect(path))
                Current.Shutdown();
        }
    }
}

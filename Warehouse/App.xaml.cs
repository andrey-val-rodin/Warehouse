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
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISqlProvider>(new SqlProvider());
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            const string path = "Components.db";
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

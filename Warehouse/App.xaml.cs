﻿using Microsoft.Extensions.DependencyInjection;
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
            //TODO
            //services.AddSingleton<MainWindow>();
            //after this call we can add constructor MainWindow(ISqlProvider p)
            //https://stackoverflow.com/questions/54877352/dependency-injection-in-net-core-3-0-for-wpf
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var sqlProvider = ServiceProvider.GetService<ISqlProvider>();
            if (!sqlProvider.Connect())
                Current.Shutdown();
        }
    }
}

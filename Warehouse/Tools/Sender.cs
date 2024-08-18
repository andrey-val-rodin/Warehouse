using System.Net.Http;
using System.Net;
using Warehouse.Model;
using Warehouse.Database;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace Warehouse.Tools
{
    internal class Sender
    {
        private const string Uri = "http://rtable.ru.xsph.ru?fabrications=true";

        private readonly HttpClient _httpClient = new();

        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();

        public async Task<bool> PostInfoAsync(Fabrication fabrication)
        {
            try
            {
                var payload = GetRow(fabrication);
                System.Diagnostics.Debug.WriteLine(payload);

                var content = new StringContent(payload);
                var response = await _httpClient.PostAsync(Uri, content);
                return response?.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        private static string GetRow(Fabrication fabrication)
        {
            var pn = fabrication.ProductName;
            var st = fabrication.StatusText;
            var n = fabrication.Number;
            var ti = fabrication.TableId;
            var pp = SqlProvider.GetProductPrice(fabrication.ProductId);
            var c = fabrication.Client;
            var d = fabrication.Details;
            var sd = fabrication.StartedDate.ToString("dd.MM.yyyy");
            var ed = fabrication.ExpectedDate?.ToString("dd.MM.yyyy");
            var cd = fabrication.ClosedDate?.ToString("dd.MM.yyyy");

            return $"{pn}\t{st}\t{n}\t{ti}\t{pp}\t{c}\t{d}\t{sd}\t{ed}\t{cd}";
        }
    }
}

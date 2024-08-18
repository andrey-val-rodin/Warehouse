using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Windows;
using Warehouse.Database;
using Warehouse.Model;

namespace Warehouse.Tools
{
    internal class GoogleSheetsProvider
    {
        private readonly string _sheetId = "1Dbk0Evh2bchfNaszjOrA_2No_qY_KlIvdL8r5LASm_4";

        private SheetsService Service { get; set; }
        private bool IsAuthorized => Service != null;
        private static ServiceProvider ServiceProvider => ((App)Application.Current).ServiceProvider;
        private static ISqlProvider SqlProvider { get; } = ServiceProvider.GetService<ISqlProvider>();

        public async Task<bool> AuthorizeAsync()
        {
            if (IsAuthorized)
                return true;

            try
            {
                UserCredential credential;
                using (var stream = new FileStream("Warehouse.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        [SheetsService.Scope.Spreadsheets],
                        "andrey.val.rodin@gmail.com",
                        CancellationToken.None);
                }

                Service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Warehouse"
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.GetType().ToString());
                return false;
            }

            return true;
        }

        public async Task<bool> AppendFabricationAsync(Fabrication fabrication)
        {
            if (!IsAuthorized)
                throw new InvalidOperationException("Not authorized");

            try
            {
                var range = "Производство!A:J";
                ValueRange valueRange = new ValueRange
                {
                    MajorDimension = "ROWS",
                    Values = GetValues(fabrication)
                };
                var values = Service.Spreadsheets.Values;
                var request = values.Append(valueRange, _sheetId, range);
                request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
                var response = await request.ExecuteAsync();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.GetType().ToString());
                return false;
            }

            return true;
        }

        private static IList<IList<object>> GetValues(Fabrication fabrication)
        {
            if (fabrication == null)
                throw new ArgumentNullException(nameof(fabrication));

            // Изделие	Номер	BluetoothId	Цена	Клиент	Заметки	Открыто	Ожидается	Закрыто																		
            return [[
                    fabrication.ProductName,
                    fabrication.StatusText,
                    fabrication.Number,
                    fabrication.TableId,
                    SqlProvider.GetProductPrice(fabrication.ProductId),
                    fabrication.Client,
                    fabrication.Details,
                    fabrication.StartedDate.ToString("dd.MM.yyyy"),
                    fabrication.ExpectedDate?.ToString("dd.MM.yyyy"),
                    fabrication.ClosedDate?.ToString("dd.MM.yyyy")]];
        }
    }
}

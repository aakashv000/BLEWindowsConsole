using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using BLEWindowsConsole.src.Services.GattUuidsService;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLEWindowsConsole.src.Models
{
    public class ObservableGattDeviceService
    {
        private GattDeviceService service;
        private string name;
        private string uuid;
        private ObservableCollection<ObservableGattCharacteristics> characteristics = 

        public ObservableGattDeviceService( GattDeviceService service)
        {
            this.service = service;
            name = GattUuidsService.ConvertUuidToName(service.Uuid);
            uuid = this.service.Uuid.ToString();
            GetAllCharacteristics();
        }

        //Get all the characteristics of this service
        private async void GetAllCharacteristics()
        {
            Console.WriteLine("ObservableGattDeviceService::getAllCharacteristics: ");

            try
            {
                //Request the necessary access permission for the service and abort if permissions are denied
                GattOpenStatus status = await service.OpenAsync(GattSharingMode.SharedReadAndWrite);
                if (status != GattOpenStatus.Success && status != GattOpenStatus.AlreadyOpened)
                {
                    Console.WriteLine("Error: " + status.ToString());
                    return;
                }

                CancellationTokenSource tokenSource = new CancellationTokenSource(5000);
                var t = Task.Run(() => service.GetCharacteristicsAsync(Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached), tokenSource.Token);

                GattCharacteristicsResult result = null;
                result = await t.Result;
            }
            catch
            {

            }
        }
    }
}
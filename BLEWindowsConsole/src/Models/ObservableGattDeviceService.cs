using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private ObservableCollection<ObservableGattCharacteristics> characteristics = new ObservableCollection<ObservableGattCharacteristics>();

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
                var task = Task.Run(() => service.GetCharacteristicsAsync(Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached), tokenSource.Token);

                GattCharacteristicsResult result = null;
                result = await task.Result;

                if(result.Status == GattCommunicationStatus.Success)
                {
                    Console.WriteLine("getAllCharacteristics found " + result.Characteristics.Count() + " characteristics");

                    foreach( GattCharacteristic gattChar in result.Characteristics)
                    {
                        characteristics.Add(new ObservableGattCharacteristics(gattChar, this));
                    }
                }
                else if( result.Status == GattCommunicationStatus.Unreachable)
                {
                    Console.WriteLine("getAllCharacteristics failed with Unreachable");
                }
                else if(result.Status == GattCommunicationStatus.ProtocolError)
                {
                    Console.WriteLine("getAllCharacteristics failed with Unreachable");
                }
            }
            catch( AggregateException ae)
            {
                foreach(var ex in ae.InnerExceptions)
                {
                    if(ex is TaskCanceledException){
                        Console.WriteLine("Getting characteristics took too long. Timed out.");
                        return;
                    }
                }
            }
            catch( Exception ex)
            {
                Console.WriteLine("getAllCharacteristics: Exception: " + ex.Message);
                throw;
            }
        }
    }
}
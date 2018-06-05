using System;
using BLEWindowsConsole.src.GattHelper.Converters;
using BLEWindowsConsole.src.Services.GattUuidsService;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLEWindowsConsole.src.Models
{
    public class ObservableGattCharacteristics
    {
        private GattCharacteristic characteristic;
        private ObservableGattDeviceService parent;
        private string name;
        private string uuid;

        public ObservableGattCharacteristics(GattCharacteristic characteristic, ObservableGattDeviceService parent)
        {
            this.characteristic = characteristic;
            this.parent = parent;
            name = GattUuidsService.ConvertUuidToName(this.characteristic.Uuid);
            uuid = this.characteristic.Uuid.ToString();

            ReadValueAsync();

            return;
        }

        //Reads the value of the characteristic
        public async void ReadValueAsync()
        {
            try
            {
                GattReadResult result = await this.characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    //Assuming the display type to be always UTF8
                    string value = GattConvert.ToUTF8String(result.Value);
                    Console.WriteLine("Value of characteristic: " + value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }
    }
}
﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;


namespace BLEWindowsConsole.src.Models
{
    public class ObservableBLEDevice
    {
        private BluetoothLEDevice _BLEDevice;
        public DeviceInformation deviceInfo;
        public string name;
        public string bluetoothAddressAsString;
        public bool isPaired;
        public bool isConnected;
        private GattDeviceServicesResult result;
        public ObservableCollection<ObservableGattDeviceService> services = new ObservableCollection<ObservableGattDeviceService>();
        public int serviceCount;
        public ulong bluetoothAddressAsUlong;

        public BluetoothLEDevice BLEDevice
        {
            get
            {
                return _BLEDevice;
            }
            private set
            {
                _BLEDevice = value;
            }
        }

        public ObservableBLEDevice(DeviceInformation deviceInformation)
        {
            deviceInfo = deviceInformation;
            name = deviceInformation.Name;

            if( deviceInfo.Properties.ContainsKey( "System.Devices.Aep.DeviceAddress"))
            {
                bluetoothAddressAsString = deviceInfo.Properties["System.Devices.Aep.DeviceAddress"].ToString();
                bluetoothAddressAsUlong = Convert.ToUInt64(bluetoothAddressAsString.Replace(":", string.Empty), 16);
            }

            isPaired = deviceInfo.Pairing.IsPaired;
        }

        public async Task<bool> Connect()
        {
            bool ret = false;

            Console.WriteLine("Connect: ");
            Console.WriteLine("Entering");

            try
            {
                if (BLEDevice == null)
                {
                    Console.WriteLine("Calling BLEDevice.FromIdAsync");
                    BLEDevice = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
                }
                else
                {
                    Console.WriteLine("Previously connected, not calling BluetoothLEDevice.FromIdAsync");
                }

                if( BLEDevice == null)
                {
                    Console.WriteLine("No permission to access device", "Connection error");
                }
                else
                {
                    Console.WriteLine("BLEDevice is: " + BLEDevice.Name);
                    name = BLEDevice.Name;
                    isPaired = deviceInfo.Pairing.IsPaired;
                    isConnected = BLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;

                    // Get all the services for this device
                    CancellationTokenSource GetGattServicesAsyncTokenSource = new CancellationTokenSource(5000);
                    var GetGattServicesAsyncTask = Task.Run(() => BLEDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached), GetGattServicesAsyncTokenSource.Token);

                    result = await GetGattServicesAsyncTask.Result;

                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        // In case we connected before, clear the service list and recreate it
                        services.Clear();

                        Console.WriteLine("GetGattServiceAsync SUCCESS");
                        foreach (var serv in result.Services)
                        {
                            services.Add(new ObservableGattDeviceService(serv));
                        }

                        serviceCount = services.Count();
                        ret = true;
                    }
                    else if (result.Status == GattCommunicationStatus.ProtocolError)
                    {
                        Console.WriteLine("GetGattServiceAsync Error: Protocol Error - " + result.ProtocolError.Value);
                        Console.WriteLine("Connection protocol error: " + result.ProtocolError.Value.ToString());

                    }
                    else if (result.Status == GattCommunicationStatus.Unreachable)
                    {
                        Console.WriteLine("GetGattServiceAsync Error: Unreachable");
                        Console.WriteLine("Device unreachable");
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Debugger.Break();
            }

            if (ret)
            {
                Console.WriteLine("Exiting(0)");
            }
            else
            {
                Console.WriteLine("Exiting(-1)");
            }

            return ret;
        }
    }
}
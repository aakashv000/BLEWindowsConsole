﻿using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BluetoothLEExplorer.Services.DispatcherService;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation.Metadata;
using System.Collections;
using System.Collections.Generic;

namespace BLEWindowsConsole.src.Models
{
    public class ObservableBLEDevice
    {
        private BluetoothLEDevice _BLEDevice;
        public DeviceInformation deviceInfo;
        public string name;
        public string btAddress;
        public bool isPaired;
        public bool isConnected;

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
                btAddress = deviceInfo.Properties["System.Devices.Aep.DeviceAddress"].ToString();
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
                        Services.Clear();

                        System.Diagnostics.Debug.WriteLine(debugMsg + "GetGattServiceAsync SUCCESS");
                        foreach (var serv in result.Services)
                        {
                            Services.Add(new ObservableGattDeviceService(serv));
                        }

                        ServiceCount = Services.Count();
                        ret = true;
                    }
                    else if (result.Status == GattCommunicationStatus.ProtocolError)
                    {
                        ErrorText = debugMsg + "GetGattServiceAsync Error: Protocol Error - " + result.ProtocolError.Value;
                        System.Diagnostics.Debug.WriteLine(ErrorText);
                        string msg = "Connection protocol error: " + result.ProtocolError.Value.ToString();
                        var messageDialog = new MessageDialog(msg, "Connection failures");
                        await messageDialog.ShowAsync();

                    }
                    else if (result.Status == GattCommunicationStatus.Unreachable)
                    {
                        ErrorText = debugMsg + "GetGattServiceAsync Error: Unreachable";
                        System.Diagnostics.Debug.WriteLine(ErrorText);
                        string msg = "Device unreachable";
                        var messageDialog = new MessageDialog(msg, "Connection failures");
                        await messageDialog.ShowAsync();
                    }
                }
            }
        }
    }
}
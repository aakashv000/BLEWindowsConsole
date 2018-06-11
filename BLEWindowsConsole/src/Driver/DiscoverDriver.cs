using System;
using System.Threading;
using BLEWindowsConsole.src.Models;


namespace BLEWindowsConsole.src.Driver
{
    //Driver for discovering device
    public class DiscoverDriver
    {
        public GattSampleContext context;
        
        private ObservableBLEDevice SelectedDevice
        {
            get
            {
                return context.SelectedBLEDevice;
            }
            set
            {
                context.SelectedBLEDevice = value;
            }
        }
        private string name;
        private ObservableBLEDevice foundDevice = null;

        public DiscoverDriver()
        {
            //context = GattSampleContext.context;          //***NOTE: Only use this to get <context> after it is initialised once
            context = new GattSampleContext();

            StartEnumeration();
            ConnectToDeviceByName("ESP32 UART Test");
        }

        public async void StartEnumeration()              //Was async
        {
            Console.WriteLine("DiscoverDriver.StartEnumeration()");
            context.StartEnumeration();
        }

        public async void StopEnumeration()                //Was async
        {
            Console.WriteLine("DiscoverDriver.StopEnumeration()");
            context.StopEnumeration();
        }

        public async void ConnectToDeviceByName( string name )
        {
            this.name = name;
            var searchThread = new Thread(SearchDeviceByName_Thread);
            searchThread.Start();
        }

        public void SearchDeviceByName_Thread()
        {
            while (true)
            {
                foundDevice = context.GetAvailableBLEDeviceByName(name);
                if (foundDevice != null)
                {
                    SelectedDevice = foundDevice;
                    break;
                }
                Thread.Sleep(5000);        //Wait for 5 secs before checking if the required device is available or not
            }

            ConnectToSelectedDevice(foundDevice);
        }

        public async void ConnectToSelectedDevice( ObservableBLEDevice bleDevice)
        {
            StopEnumeration();

            Console.WriteLine("Connecting to: " + SelectedDevice.name);
            if( await SelectedDevice.Connect() == false)
            {
                Console.WriteLine("ConnectToSelectedDevice: Something went wrong getting the BLEDevice");
                SelectedDevice = null;
                return;
            }

            Console.WriteLine("ConnectToSelectedDevice: Connected");
        }
    }
}
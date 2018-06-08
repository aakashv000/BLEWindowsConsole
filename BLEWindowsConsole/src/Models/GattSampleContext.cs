using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;

namespace BLEWindowsConsole.src.Models
{
    public class GattSampleContext
    {
        private const string BatteryLevelGUID = "{995EF0B0-7EB3-4A8B-B9CE-068BB3F4AF69} 10";
        private const string BluetoothDeviceAddress = "System.DeviceInterface.Bluetooth.DeviceAddress";
        private const string DevNodeBTLEDeviceWatcherAQSString = "(System.Devices.ClassGuid:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")";
        /// <summary>
        /// Device watcher used to find bluetooth device dev nodes
        /// </summary>
        private DeviceWatcher devNodeWatcher;
        //All the bluetooth dev nodes on the system
        private List<DeviceInformation> devNodes = new List<DeviceInformation>();

        ///<summary>
        ///Gets the app context
        ///</summary>
        public static GattSampleContext context;                //This + <context = this> -> fixed BadImageFormatException: could not find or load assembly 'System.Runtime.WindowsRuntime'

        /// <summary>
        /// Device watcher used to find bluetooth devices
        /// </summary>
        private DeviceWatcher deviceWatcher;
        /// <summary>
        /// AQS search string used to find bluetooth devices
        /// </summary>
        private const string BTLEDeviceWatcherAQSString = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";
        /// <summary>
        /// Advertisement watcher used to find bluetooth devices
        /// </summary>
        private BluetoothLEAdvertisementWatcher advertisementWatcher;

        // Gets or sets the list of available Bluetooth devices
        public ObservableCollection<ObservableBLEDevice> BLEDevices { get; set; } = new ObservableCollection<ObservableBLEDevice>();


        //Context for the entire app. This is where all app wide variables are stored
        public GattSampleContext()
        {
            context = this;
            Init();
        }

        //Initializes the app context
        private async void Init()
        {
            BluetoothAdapter adapter = await BluetoothAdapter.GetDefaultAsync();
            if (adapter == null)
            {
                Console.WriteLine("Error getting access to Bluetooth adapter. Do you have Bluetooth enabled?");
            }

            string[] requestedProperties =
                {
                    BatteryLevelGUID,
                    BluetoothDeviceAddress
                };

            devNodeWatcher =
                DeviceInformation.CreateWatcher(
                    DevNodeBTLEDeviceWatcherAQSString,
                    requestedProperties,
                    DeviceInformationKind.Device
                    );

            devNodeWatcher.Added += DevNodeWatcher_Added;

            devNodeWatcher.Start();

            return;
        }

        private async void DevNodeWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            devNodes.Add(args);
            Console.WriteLine("DevNodeWatcher_Added: " + args.ToString());
        }

        public void StartEnumeration()
        {
            string[] requestedProperties =
                {
                    "System.Devices.Aep.Category",
                    "System.Devices.Aep.ContainerId",
                    "System.Devices.Aep.DeviceAddress",
                    "System.Devices.Aep.IsConnected",
                    "System.Devices.Aep.IsPaired",
                    "System.Devices.Aep.IsPresent",
                    "System.Devices.Aep.ProtocolId",
                    "System.Devices.Aep.Bluetooth.Le.IsConnectable",
                    "System.Devices.Aep.SignalStrength"
                };

            deviceWatcher =
                DeviceInformation.CreateWatcher(
                    BTLEDeviceWatcherAQSString,
                    requestedProperties,
                    DeviceInformationKind.AssociationEndpoint);

            // Register event handlers
            deviceWatcher.Added += DeviceWatcher_Added;

            advertisementWatcher = new BluetoothLEAdvertisementWatcher();
            advertisementWatcher.Received += AdvertisementWatcher_Received;

            BLEDevices.Clear();

            deviceWatcher.Start();
            advertisementWatcher.Start();
        }

        private async void DeviceWatcher_Added( DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            try
            {
                if(sender == deviceWatcher)
                {
                    await AddDeviceToList(deviceInfo);
                }
            }
            catch( Exception ex)
            {
                Console.WriteLine("DeviceWatcher_Added: " + ex.Message);
            }
        }

        // Adds the new device to the displayed list
        private async Task AddDeviceToList( DeviceInformation deviceInfo)
        {
            ObservableBLEDevice device = new ObservableBLEDevice(deviceInfo);

            //Need to lock as another DeviceWatcher might be modifying BluetoothLEDevices - ///////Not implemented
            if( !BLEDevices.Contains( device ))
            {
                BLEDevices.Add(device);
                Console.WriteLine("AddDeviceToList: " + device.bluetoothAddressAsString);
            }
        }

        // Updates device metadata based on advertisement received
        private async void AdvertisementWatcher_Received( BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            try
            {
                foreach( ObservableBLEDevice d in BLEDevices)
                {
                    if( d.bluetoothAddressAsUlong == args.BluetoothAddress)
                    {
                        d.serviceCount = args.Advertisement.ServiceUuids.Count();
                    }
                }
            }
            catch( Exception ex)
            {
                Console.WriteLine("AdvertisementWatcher_Received: " + ex.Message);
            }
        }
    }
}
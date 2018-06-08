using System;
using BLEWindowsConsole.src.Models;


namespace BLEWindowsConsole.src.Driver
{
    //Driver for discovering device
    public class DiscoverDriver
    {
        public GattSampleContext context;

        public DiscoverDriver()
        {
            //context = GattSampleContext.context;          //***NOTE: Only use this to get <context> after it is initialised once
            context = new GattSampleContext();
            StartEnumeration();
        }

        public async void StartEnumeration()
        {
            Console.WriteLine("DiscoverDriver.StartEnumeration()");
            context.StartEnumeration();
        }
    }
}
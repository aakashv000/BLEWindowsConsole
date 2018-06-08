using System;
using BLEWindowsConsole.src.Driver;

namespace BLEWindowsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Hello World!");

                DiscoverDriver discoverDriver = new DiscoverDriver();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Main: " + ex.Message);
            }
            finally
            {
                Console.ReadKey();
            }
        }
    }
}

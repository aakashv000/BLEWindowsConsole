using System;
using Windows.Storage.Streams;

namespace BLEWindowsConsole.src.GattHelper.Converters
{
    public static class GattConvert
    {
        public static string ToUTF8String( IBuffer buffer)
        {
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;
            return reader.ReadString(buffer.Length);
        }
    }
}
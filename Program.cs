namespace MouseTrails
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;

    public sealed class MouseTrails {
        private const string MouseRegistryKey = @"HKEY_CURRENT_USER\Control Panel\Mouse";
        private const string MouseTrailsRegistryValue = "MouseTrails";
        private const uint SPI_GETMOUSETRAILS = 0x5E;
        private const uint SPI_SETMOUSETRAILS = 0x5D;
        private const uint SPIF_SENDCHANGE = 0x2;

        public static int GetMouseTrails()
        {
            var current = Registry.GetValue(MouseRegistryKey, MouseTrailsRegistryValue, null) as string;
            if (current == null) current = "0";

            uint regValue;
            if (!uint.TryParse(current, out regValue)) regValue = 0;

            uint sysParamValue = 0;
            if (!SystemParametersInfo(SPI_GETMOUSETRAILS, 0, ref sysParamValue, 0)) sysParamValue = 0;
            if (sysParamValue == 1) sysParamValue = 0;

            var highestVal = Math.Max(regValue, sysParamValue);
            return highestVal > int.MaxValue ? int.MaxValue : (int)highestVal;
        }

        public static void SetMouseTrails(int value)
        {
            if (value < 2) value = 0; // 0/1 mean "no trails" ... keep internally consistent w/ 0
            // stored as string intentionally, not sure why it isn't DWORD. Will throw if access denied which is fine.
            Registry.SetValue(MouseRegistryKey, MouseTrailsRegistryValue, value.ToString());

            uint unused = (uint)value;
            SystemParametersInfo(SPI_SETMOUSETRAILS, (uint)value, ref unused, SPIF_SENDCHANGE);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref uint pvParam, uint fWinIni);

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            switch (args[0].ToLowerInvariant())
            {
            case "on":
                SetMouseTrails(7);
                break;
            case "off":
                SetMouseTrails(0);
                break;
            default:
                Console.WriteLine("Usage: mousetrails [on|off]");
                break;
            }

            var t = GetMouseTrails();
            if (t == 0)
                Console.WriteLine("Mouse trails are \x1b[1moff\x1b[m.");
            else
                Console.WriteLine($"Mouse trails are \x1b[1mon\x1b[m ({t}).");
        }
    }
}
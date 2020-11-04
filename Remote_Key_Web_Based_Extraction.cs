using System;
using Windows.Devices.WiFi;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Management.Automation;

namespace Remote_Key_Web_Based_Extraction
{
    class Remote_Key_Web_Based_Extraction
    {
        static List<string> access_point_names = new List<string>();

        static void Main(string[] args)
        {
            Remote_Key_Web_Based_Extraction main_program = new Remote_Key_Web_Based_Extraction();
            var ap_extraction_task = main_program.extract_access_point_names();
            ap_extraction_task.Wait();
            string key_content = main_program.key_content_extraction();
        }

        public async Task extract_access_point_names()
        {
            // Credits: https://stackoverflow.com/questions/496568/how-do-i-get-the-available-wifi-aps-and-their-signal-strength-in-net?rq=1
            var adapters = await WiFiAdapter.FindAllAdaptersAsync();
            foreach (var adapter in adapters)
            {
                foreach (var network in adapter.NetworkReport.AvailableNetworks)
                {
                    Console.WriteLine($"ssid: {network.Ssid}" + " | " + $"signal strength: {network.SignalBars}");
                    access_point_names.Add(network.Ssid);
                }
            }
        }

        public string key_content_extraction()
        {
            Console.WriteLine("netsh wlan show profile " + access_point_names[0] + " key=clear");
            PowerShell ps = PowerShell.Create();
            ps.AddCommand("netsh")
                .AddParameter("wlan show profile " + access_point_names[0], "key=clear");
            var results = ps.Invoke();
            foreach (var item in results)
            {
                Console.WriteLine(item);
            }
            foreach (var item in results)
            {
                if (item.ToString().Contains("Key Content"))
                {
                    string final_key = item.ToString().Substring(item.ToString().IndexOf(":") + 2, item.ToString().Length - (item.ToString().IndexOf(":") + 2));
                    return final_key;
                }
            }
            return "nil";
        }
    }
}

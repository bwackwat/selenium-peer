using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TestingFramework.PageObjects
{
    public class LoginPage
    {
        public IWebElement usernameField;
        public IWebElement passwordField;

        public LoginPage(ChromeDriver driver)
        {
            usernameField = driver.FindElementById("username");
            passwordField = driver.FindElementById("password");
        }
    }
}

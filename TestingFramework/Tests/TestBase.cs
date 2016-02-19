using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace TestingFramework.Tests
{
    abstract class TestBase
    {
        protected ChromeDriver driver;

        public TestBase(ChromeDriver driver)
        {
            this.driver = driver;
            this.driver.Navigate().GoToUrl("https://www.bwackwat.com:3000/content-server/");
            RunTest();
        }

        abstract protected void RunTest();
    }
}

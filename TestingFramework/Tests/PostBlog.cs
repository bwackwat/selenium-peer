using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TestingFramework.PageObjects;

namespace TestingFramework.Tests
{
    class PostBlog : TestBase
    {
        public PostBlog(ChromeDriver driver) : base(driver)
        {

        }

        override protected void RunTest()
        {
            var login = new LoginPage(driver);

            login.usernameField.SendKeys("bwackwat");
            login.passwordField.SendKeys("thejohnisblue");
        }
    }
}

using OpenQA.Selenium;
using TestingFramework.PageObjects;

namespace TestingFramework.Tests
{
    class POIUseCase : TestBase
    {
        public POIUseCase(IWebDriver driver) : base(driver)
        {
        }

        override protected void RunTest()
        {
            var poi = new POIPage(driver);

            poi.usernameField.SendKeys("bwackwat");
            poi.passwordField.SendKeys("thejohnisblue");
        }
    }
}

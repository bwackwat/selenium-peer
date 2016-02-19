using OpenQA.Selenium;

namespace TestingFramework.PageObjects
{
    public class POIPage
    {
        public IWebElement loginButton;
        public IWebElement usernameField;
        public IWebElement passwordField;

        public POIPage(IWebDriver driver)
        {
            driver.Navigate().GoToUrl("https://www.bwackwat.com:2000/");

            usernameField = driver.FindElement(By.Id("username"));
            usernameField = driver.FindElement(By.Id("password"));
        }
    }
}
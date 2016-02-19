using OpenQA.Selenium;

namespace TestingFramework.Tests
{
    abstract class TestBase
    {
        protected IWebDriver driver;

        public TestBase(IWebDriver driver)
        {
            this.driver = driver;
            RunTest();
        }

        abstract protected void RunTest();
    }
}

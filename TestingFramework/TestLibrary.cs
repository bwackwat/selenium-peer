using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TestingFramework.Tests;

namespace TestingFramework
{
    class TestLibrary
    {
        static void Main(string[] args)
        {
            new TestLibrary(new ChromeDriver("E:\\Libraries\\Desktop"));
        }

        public TestLibrary(IWebDriver driver)
        {
            var test1 = new POIUseCase(driver);
        }
    }
}

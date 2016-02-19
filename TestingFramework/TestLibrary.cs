using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TestingFramework.Tests;

namespace TestingFramework
{
    class TestLibrary
    {
        static void Main(string[] args)
        {
            var tests = new TestLibrary();
        }

        public TestLibrary()
        {
            var driver = new ChromeDriver();

            var test1 = new PostBlog(driver);
            
        }
    }
}

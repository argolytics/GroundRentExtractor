using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace DataLibrary.Models;

public class WebDriverModel
{
    public WebDriver Driver { get; set; }
    public IWebElement Input { get; set; }
    public List<AddressModel> AddressList { get; set; }
}

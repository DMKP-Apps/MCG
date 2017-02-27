using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NetworkServer;
using NetworkServer.Controllers;

namespace NetworkServer.Tests
{
    [TestClass]
    public class RepositoryTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Home Page", result.ViewBag.Title);
        }
    }
}

using CustomerRankWebApi.Controllers;
using CustomerRankWebApi.Model;
using CustomerRankWebApi.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CustomerRank.xUnitTest.Controller
{
    public class CustomerControllerTest
    {
        private readonly CustomerController _controller;
        private readonly Mock<ICustomerService> _mockService;
        public CustomerControllerTest()
        {
            _mockService = new Mock<ICustomerService>();

            _controller = new CustomerController(_mockService.Object);
        }

        [Fact]
        public async void UpdteScoreTest()
        {
            var score = 200;
            _mockService.Setup(repo => repo.UpdateScore(It.IsAny<long>(), It.IsAny<decimal>())).ReturnsAsync(score);
            var result = await _controller.UpdteScore(2000, 100);
            Assert.Equal(score, result);
        }

        [Fact]
        public async void GetCustomersByRankTest()
        {
            
            _mockService.Setup(repo => repo.GetCustomersByRank(It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(GetCustomers());
            var actioinresult = await _controller.GetCustomersByRank(1, 100);
            var objectResult = Assert.IsType<OkObjectResult>(actioinresult);
            var result = Assert.IsAssignableFrom<IEnumerable<Customer>>(objectResult.Value);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async void GetNeighborhoodsTest()
        {
            _mockService.Setup(repo => repo.GetNeighborhoods(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(GetCustomers());
            var actioinresult = await _controller.Get(200,1, 2);
            var objectResult = Assert.IsType<OkObjectResult>(actioinresult);
            var result = Assert.IsAssignableFrom<IEnumerable<Customer>>(objectResult.Value);
            Assert.Equal(3, result.Count());
        }

        private IEnumerable<Customer> GetCustomers()
        {
            return new List<Customer>()
             {
                 new Customer() { CustomerID = 100, Score = 98, Rank = 1 },
                 new Customer() { CustomerID = 200, Score = 96, Rank = 2 },
                 new Customer() { CustomerID = 300, Score = 50, Rank = 3 },
             };
        }
    }
}



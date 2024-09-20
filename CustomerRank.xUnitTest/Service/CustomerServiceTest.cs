using CustomerRank.xUnitTest.UnitCore;
using CustomerRankWebApi.Controllers;
using CustomerRankWebApi.Exception;
using CustomerRankWebApi.Model;
using CustomerRankWebApi.Service;
using CustomerRankWebApi.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerRank.xUnitTest.Service
{
    [TestCaseOrderer("CustomerRank.xUnitTest.UnitCore.PriorityOrderer", "CustomerRank.xUnitTest.UnitCore")]
    public class CustomerServiceTest
    {
        private readonly CustomerService _customerSrv;
        private readonly Mock<ILogger<CustomerService>> _mockLogger;
        private readonly Mock<IHubContext<CustomerHub, ICustomerHubClient>> _mockHub;
        public CustomerServiceTest()
        {
            _mockLogger = new Mock<ILogger<CustomerService>>();
            _mockHub = new Mock<IHubContext<CustomerHub, ICustomerHubClient>>();
            _mockHub.Setup(hub=>hub.Clients.All.SendLeaderboardToUser(It.IsAny<IEnumerable<Customer>>())).Returns(Task.CompletedTask);
            _customerSrv = new CustomerService(_mockHub.Object,_mockLogger.Object);
        }

        [Theory, TestPriority(1)]
        [InlineData(0, 100)]
        [InlineData(100, 20)]
        [InlineData(-200, -1)]

        public async void GetCustomersByRank_Exception_Test(long start,long end)
        {
           
            await Assert.ThrowsAsync<BusinessException>(() => _customerSrv.GetCustomersByRank(start, end));
        }

        [Theory, TestPriority(2)]
        [InlineData(-100, 0, 100)]
        [InlineData(1000, -20,20)]
        [InlineData(00, 10,-5)]

        public async void GetNeighborhoods_Exception_Test(long customerId,long start, long end)
        {
            
            await Assert.ThrowsAsync<BusinessException>(() => _customerSrv.GetNeighborhoods(customerId, start, end));

        }

        [Theory, TestPriority(3)]
        [InlineData(0, 100)]
        [InlineData(100, -1001)]
        [InlineData(-200, 999)]
        [InlineData(200, 1001)]
        public async void GetUpdateScore_Exception_Test(long customerId, decimal score)
        {
            await Assert.ThrowsAsync<BusinessException>(() => _customerSrv.UpdateScore(customerId, score));
        }

        [Fact, TestPriority(4)]
        public async void GetNeighborhoods_Empty_Test()
        {
            var customers = await this._customerSrv.GetNeighborhoods(22222, 1, 100); ;
            Assert.Empty(customers);
        }

        [Fact]
        [TestPriority(5)]
        public async void GetCustomersByRank_Empty_Test()
        {
            var customers = await this._customerSrv.GetCustomersByRank(56, 100);
            Assert.Empty(customers);
        }
        [Fact]
        [TestPriority(6)]
        public async void UpdateScore_AddOrUpdate_Test()
        {
            var score =  await this._customerSrv.UpdateScore(1000, 98);
            Assert.Equal(98, score);
            score = await this._customerSrv.UpdateScore(2000, 96);
            Assert.Equal(96, score);
            score = await this._customerSrv.UpdateScore(2222, 36);
            Assert.Equal(36, score);

            score = await this._customerSrv.UpdateScore(18552, 65);
            Assert.Equal(65, score);

            score = await this._customerSrv.UpdateScore(1000, -40);
            Assert.Equal(58, score);

            score = await this._customerSrv.UpdateScore(2222, 200);
            Assert.Equal(236, score);

            score = await this._customerSrv.UpdateScore(18552, 0);
            Assert.Equal(65, score);

            GetCustomersByRank_Normal_Test();

            GetNeighborhoods_Normal_Test();
        }

        
        private async void GetCustomersByRank_Normal_Test()
        {
            var customers = await this._customerSrv.GetCustomersByRank(1, 100);
            Assert.NotEmpty(customers);
            Assert.Equal(4,customers.Count());
            var first=customers.FirstOrDefault();
            Assert.NotNull(first);
            Assert.Equal(236,first.Score);
        }

        private async void GetNeighborhoods_Normal_Test()
        {
            var customers = await this._customerSrv.GetNeighborhoods(18552,2, 2);
            Assert.NotEmpty(customers);
            Assert.Equal(4, customers.Count());
            var first = customers.FirstOrDefault(c=>c.CustomerID== 18552);
            Assert.NotNull(first);
            Assert.Equal(65, first.Score);
        }
    }
}

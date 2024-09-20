using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CustomerRank.xUnitTest
{
    public class IntegrationTest: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        //private readonly HttpClient _client;
        public IntegrationTest(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        //[Fact]
        public async Task UpdateScore_Return_Normal()
        {
            
            var httpClient = _factory.CreateClient();
           
            var response = await httpClient.PostAsync("/customer/22212/score/78",null);
            
      
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
          
            var score = await response.Content.ReadAsStringAsync();
            Assert.NotNull(score);
        }
    }
}

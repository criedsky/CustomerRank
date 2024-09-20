using CustomerRankWebApi.Model;
using CustomerRankWebApi.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CustomerRankWebApi.Controllers
{
   
    [ApiController]
    public class CustomerController : ControllerBase
    {
        //private readonly ILogger<CustomerController> _logger;
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            //_logger = logger;
            this._customerService=customerService;
        }

        [HttpPost]
        [Route("/customer/{customerId}/score/{score}")]
        public async Task<decimal> UpdteScore([FromRoute] long customerId, [FromRoute] decimal score)
        {
            return await this._customerService.UpdateScore(customerId,score);
        }

        [HttpGet]
        [Route("/leaderboard")]
        public async Task<IActionResult> GetCustomersByRank([FromQuery] long start, [FromQuery] long end)
        {
            //if (start<=0 || end<=0 || start > end)
            //{
            //    return BadRequest("start and end must be greater than zero,end can't be less than start.");
            //}
            var result= await this._customerService.GetCustomersByRank(start, end);
            return Ok(result);
        }

        [HttpGet]
        [Route("/leaderboard/{customerId}")]
        public async Task<IActionResult> Get(long customerId, [FromQuery] long low = 0, [FromQuery] long high = 0)
        {
            var result= await this._customerService.GetNeighborhoods(customerId, low, high);
            
            return this.Ok(result);
        }
    }
}

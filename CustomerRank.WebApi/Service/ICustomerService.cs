using CustomerRankWebApi.Model;
using System.Linq;
namespace CustomerRankWebApi.Service
{
    public interface ICustomerService
    {
        Task<decimal> UpdateScore(long customerId, decimal score);
        Task<IEnumerable<Customer>> GetCustomersByRank(long start, long end);
        Task<IEnumerable<Customer>> GetNeighborhoods(long customerId , long low =0, long high = 0);
    }
}

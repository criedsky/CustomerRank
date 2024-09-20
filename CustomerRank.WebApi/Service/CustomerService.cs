
using CustomerRankWebApi.Exception;
using CustomerRankWebApi.Model;
using CustomerRankWebApi.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

namespace CustomerRankWebApi.Service
{
    public class CustomerService : ICustomerService
    {
        private static SemaphoreSlim slimlock = new SemaphoreSlim(1, 1);
        private static LinkedCustomer head = new LinkedCustomer { Customer = new Customer { },IsEmpty = true,Next = null };
        private static IEnumerable<Customer> leaderboardCustomerList = new List<Customer>();
        //private static SortedList<double, Customer> leaderboard = new SortedList<double, Customer>();
        //private static SortedSet<Customer> leaderboard = new SortedSet<Customer>();
       
        private IHubContext<CustomerHub, ICustomerHubClient> _customerHub;
        private readonly ILogger<CustomerService> _logger;
        public CustomerService(IHubContext<CustomerHub, ICustomerHubClient> customerHub, ILogger<CustomerService> logger)
        {
            this._customerHub = customerHub;
            this._logger = logger;
        }
        public Task<IEnumerable<Customer>> GetCustomersByRank(long start, long end)
        {

            if (start <= 0 || end <= 0 || end < start)
            {
                throw new BusinessException("start and end must be greater than zero,end can't be less than start.", HttpStatusCode.BadRequest);
            }
            //var leaderboard = this.GetLeaderboard();
            return Task.FromResult(leaderboardCustomerList.Where(c => c.Rank >= start && c.Rank <= end));

        }

        public Task<IEnumerable<Customer>> GetNeighborhoods(long customerId, long low = 0, long high = 0)
        {
            if (customerId <= 0)
            {
                throw new BusinessException("customer Id must be an arbitrary positive int64 number", HttpStatusCode.BadRequest);
            }
            if (low < 0 || high < 0)
            {
                throw new BusinessException("low and heigh can't be a negative number.", HttpStatusCode.BadRequest);
            }
            //var leaderboard = this.GetLeaderboard();
            var customer = leaderboardCustomerList.FirstOrDefault(c => c.CustomerID == customerId);
            if (customer == null)
            {
                return Task.FromResult(Enumerable.Empty<Customer>());
                //throw new System.Exception("no customer found");
            }

            var customerRank = customer.Rank;

            var list = leaderboardCustomerList.Where(c =>
            {
                if (low > 0)
                {
                    return c.Rank >= customerRank - low;
                }
                else if (high > 0)
                {
                    return c.Rank <= customerRank + high;
                }
                return c.Rank >= (customerRank - low) && c.Rank <= (customerRank + high);
            });
            return Task.FromResult(list);

        }


        public async Task<decimal> UpdateScore(long customerId, decimal score)
        {
            await slimlock.WaitAsync();
            
            try
            {
                var result = this.UpdateCustomer(customerId, score);

                leaderboardCustomerList = this.GetLeaderboard();

                _logger.LogInformation($"----customer:{customerId} updte score finished , score is {result.UpdatedNode.Customer.Score}----");

                if (result.NoticeLeaderboard)
                {
                    await this._customerHub.Clients.All.SendLeaderboardToUser(leaderboardCustomerList);
                }

                return result.UpdatedNode.Customer.Score;
                /*decimal finalScore = 0;

                var customer = leaderboard.FirstOrDefault(c => c.CustomerID == customerId);
                if (customer == null)
                {
                    customer = new Customer { CustomerID = customerId, Score = score };
                    leaderboard.Add(customer);
                }
                else
                {
                    leaderboard.Remove(customer);
                    customer.Score += score;
                    leaderboard.Add(customer);
                }
                finalScore = customer.Score;
                this.CalcCustomerRank();

                await this._customerHub.Clients.All.SendLeaderboardToUser(leaderboard.Where(c => c.Score > 0));
                return finalScore;*/
            }
            finally
            {
                slimlock.Release();
            }
        }

        private (LinkedCustomer UpdatedNode, bool NoticeLeaderboard) UpdateCustomer(long customerId, decimal score)
        {
            if (customerId <= 0)
            {
                throw new BusinessException("customer Id must be an arbitrary positive int64 number", HttpStatusCode.BadRequest);
            }
            if (score < -1000 || score > 1000)
            {
                throw new BusinessException("score must be in range of [-1000,1000]", HttpStatusCode.BadRequest);
            }

            if (head.IsEmpty)
            {
                head.Customer.CustomerID = customerId;
                head.Customer.Score = score;
                head.IsEmpty = false;
                return (head, true);
            }
            LinkedCustomer? existedCustomer = null;
            LinkedCustomer? preCustomer = null;
            if (head.Customer.CustomerID == customerId)
            {
                existedCustomer = head;
            }
            else
            {
                var curr = head;
                var next = head.Next;

                while (next != null)
                {
                    if (next?.Customer.CustomerID == customerId)
                    {
                        preCustomer = curr;
                        existedCustomer = next;
                        break;
                    }
                    curr = curr?.Next;
                    next = next?.Next;
                }
            }
            if (existedCustomer == null)
            {
                existedCustomer = new LinkedCustomer { Customer = new Customer { CustomerID = customerId, Score = score } };
                var insertedCustomer=Insert(existedCustomer);
                return (insertedCustomer, true);
            }

            if (score == 0)
            {
                return (existedCustomer, false);
            }
            // update the exist customer score and re-order 
            if (preCustomer != null)
            {
                preCustomer.Next = existedCustomer.Next;
                existedCustomer.Next = null;
            }
            existedCustomer.Customer.Score += score;
            var updatedCustomer=this.Insert(existedCustomer, preCustomer==null);

            return (updatedCustomer, true);
        }

        private IEnumerable<Customer> GetLeaderboard()
        {
            if (head.IsEmpty)
            {
                yield break;
            }
            var curr = head;
            int index = 1;
            while (curr != null)
            {
                if (curr.Customer.Score > 0)
                {
                    curr.Customer.Rank = index;
                    index++;
                    yield return curr.Customer;
                    curr = curr.Next;
                }
                else
                {
                    yield break;
                }
            }
        }

        private LinkedCustomer Insert(LinkedCustomer updatingdNode, bool updateHead=false)
        {
            if (head.IsEmpty) 
            {
                return head;
            }
            if (!updateHead && updatingdNode.CompareTo(head) < 0)
            {
                var headClone = head.DeepCopy();

                head.Customer.CustomerID = updatingdNode.Customer.CustomerID;
                head.Customer.Score = updatingdNode.Customer.Score;
                head.Next = updatingdNode;

                updatingdNode.Customer.CustomerID = headClone.Customer.CustomerID;
                updatingdNode.Customer.Score = headClone.Customer.Score;
                updatingdNode.Next = headClone.Next;

                return head;
            }
            if (updateHead && updatingdNode.Next!=null && updatingdNode.Next.CompareTo(head)<0)
            {
                var headClone = head.DeepCopy(false);
                head.Customer.CustomerID = updatingdNode.Next.Customer.CustomerID;
                head.Customer.Score = updatingdNode.Next.Customer.Score;
                head.Next = updatingdNode.Next.Next;

                updatingdNode = headClone;

               
            }
            
            var curr = head;
            var next = head?.Next;
            while (curr != null)
            {
                if (updatingdNode.CompareTo(curr) > 0 && (next == null || updatingdNode.CompareTo(next) < 0))
                {
                    updatingdNode.Next = next;
                    curr.Next = updatingdNode;
                    break;
                }
                if (next == null)
                {
                    break;
                }
                curr = curr.Next;
                next = next?.Next;
            }
            return updatingdNode;
        }

        /*private void CalcCustomerRank()
        {
            var rank = 1;
            foreach (var customer in leaderboard)
            {
                customer.Rank = rank;
                rank++;
            }
        }*/
    }

    
}

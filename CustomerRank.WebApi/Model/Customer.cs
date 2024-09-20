namespace CustomerRankWebApi.Model
{
    public class Customer : IComparable<Customer>
    {
        public long CustomerID { get; set; }
        public decimal Score { get; set; }
        public long Rank { get; set; }

        public int CompareTo(Customer? other)
        {
            if (other == null)
            {
                return -1;
            }
            //first by score
            int result = other.Score.CompareTo(this.Score);

            //then customer id
            if (result == 0)
                result = this.CustomerID.CompareTo(other.CustomerID);

            return result;
        }
    }

    public class LinkedCustomer : IComparable<LinkedCustomer>
    {
        public Customer Customer { get; set; }

        public LinkedCustomer? Next { get; set; }

        public bool IsEmpty { get; set; }

        public int CompareTo(LinkedCustomer? other)
        {
            if (other == null)
            {
                return -1;
            }
            return Customer.CompareTo(other.Customer);
        }

        public LinkedCustomer DeepCopy(bool copyNext = true)
        {
            return new LinkedCustomer
            {
                Customer = new Customer { CustomerID = this.Customer.CustomerID, Score = this.Customer.Score },
                Next = copyNext ? this.Next : null
            };
        }
    }

}

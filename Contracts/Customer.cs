using System.Transactions;

namespace CustomSerializer
{    
    public class Customer
    {
        [SerializableProperty]
        public Guid CustomerID { get; set; } = Guid.NewGuid();

        [SerializableProperty]
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public string PhoneNumber { get; set; }

        [SerializableProperty]
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }    
}
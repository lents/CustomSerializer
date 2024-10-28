namespace CustomSerializer
{
    using System;

    public class Transaction
    {
        [SerializableProperty]
        public Guid TransactionID { get; set; } = Guid.NewGuid();

        [SerializableProperty]
        [Required]
        public decimal Amount { get; set; }

        [SerializableProperty]
        public DateTime Timestamp { get; set; }

        
        public string AccountNumber { get; set; }
        public string SSN { get; set; }
    }

}
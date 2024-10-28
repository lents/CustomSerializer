using System.Transactions;

namespace CustomSerializer
{
    public class Program
    {
        public static void Main()
        {
            var customer = new Customer
            {
                Name = "John Doe",
                Transactions = new List<Transaction>
                {
                new Transaction { Amount = 150.00M, Timestamp = DateTime.Now.AddHours(-2) },
                new Transaction { Amount = 200.00M, Timestamp = DateTime.Now.AddHours(-1) }
                }
            };

            var serializer = new BinarySerializer<Customer>();

            // Serialize customer object
            byte[] serializedData = serializer.Serialize(customer);
            Console.WriteLine("Serialized Customer Data (in binary format):");
            Console.WriteLine(BitConverter.ToString(serializedData));

            // Deserialize customer object
            var deserializedCustomer = serializer.Deserialize(serializedData);
            Console.WriteLine("\nDeserialized Customer Data:");
            Console.WriteLine($"Name: {deserializedCustomer.Name}");
            foreach (var transaction in deserializedCustomer.Transactions)
            {
                Console.WriteLine($"Transaction Amount: {transaction.Amount}, Timestamp: {transaction.Timestamp}");
            }
        }
    }

}

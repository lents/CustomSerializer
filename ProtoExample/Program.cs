using CustomSerializer;
using System.Transactions;
using Transaction = CustomSerializer.Transaction;

namespace ProtoExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Customer customer = new Customer
            {
                Name = "John Doe",
                Transactions = { new Transaction { Amount = 150.0, Timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() } }
            };

            // Serialize
            using (var stream = new MemoryStream())
            {
                customer.WriteTo(stream); // Proto serialization
                byte[] serializedData = stream.ToArray();
            }

            // Deserialize
            using (var stream = new MemoryStream(serializedData))
            {
                Customer deserializedCustomer = Customer.Parser.ParseFrom(stream);
            }

        }
    }
}

using System.Collections;
using System.Reflection;
using System.Text;

namespace CustomSerializer
{
    public class BinarySerializer<T> where T : new ()
    {
        private static readonly HashSet<Type> AllowedTypes = new HashSet<Type>
        {
            typeof(int),
            typeof(decimal),
            typeof(DateTime),
            typeof(Guid),
            typeof(string),
            typeof(List<Transaction>),  
            typeof(Customer),
            typeof(Transaction) 
        };
        public byte[] Serialize(T obj)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                SerializeObject(obj, writer);
                return ms.ToArray();
            }
        }

        public T Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                return DeserializeObject<T>(reader);
            }
        }

        private void SerializeObject(object obj, BinaryWriter writer)
        {
            Type type = obj.GetType();
            if (!AllowedTypes.Contains(type))
            {
                throw new NotSupportedException($"Type {type} is not allowed for serialization.");
            }
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(property, typeof(SerializablePropertyAttribute)))
                {
                    var value = property.GetValue(obj);
                    string propertyName = property.Name;

                    byte[] propertyNameBytes = Encoding.UTF8.GetBytes(propertyName);
                    writer.Write(propertyNameBytes.Length);
                    writer.Write(propertyNameBytes);

                    WriteValue(writer, value);
                }
            }
        }
        public TObj DeserializeObject<TObj>(BinaryReader reader) where TObj : new()
        {
            var obj = new TObj();
            return (TObj)DeserializeObjectWithAttributes(obj, reader);
        }
        private object DeserializeObjectWithAttributes(object obj, BinaryReader reader)
        {
            var type = obj.GetType();
            if (!AllowedTypes.Contains(type))
            {
                throw new NotSupportedException($"Type {type} is not allowed for serialization.");
            }
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(property, typeof(SerializablePropertyAttribute)))
                {
                    // Read property name length and name
                    int nameLength = reader.ReadInt32();
                    byte[] nameBytes = reader.ReadBytes(nameLength);
                    string propertyName = Encoding.UTF8.GetString(nameBytes);

                    // Check if the property name matches
                    if (property.Name == propertyName)
                    {
                        // Deserialize and set the property value
                        var value = ReadValue(reader, property.PropertyType);
                        ValidateProperty(property, value);
                        property.SetValue(obj, value);
                    }
                }
            }
            return obj;
        }

        private void WriteValue(BinaryWriter writer, object value)
        {
            if (value == null)
            {
                writer.Write((byte)0); // Null indicator
                return;
            }

            Type type = value.GetType();

            switch (type)
            {
                case Type _ when type == typeof(int):
                    writer.Write((byte)1);
                    writer.Write((int)value);
                    break;

                case Type _ when type == typeof(decimal):
                    writer.Write((byte)2);
                    writer.Write((decimal)value);
                    break;

                case Type _ when type == typeof(DateTime):
                    writer.Write((byte)3);
                    writer.Write(((DateTime)value).ToBinary());
                    break;

                case Type t when t == typeof(Guid):
                    writer.Write((byte)4);
                    writer.Write(((Guid)value).ToByteArray());
                    break;

                case Type _ when type == typeof(string):
                    writer.Write((byte)5);
                    var str = (string)value;
                    var stringBytes = Encoding.UTF8.GetBytes(str);
                    writer.Write(stringBytes.Length);
                    writer.Write(stringBytes);
                    break;                

                case Type _ when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>):
                    writer.Write((byte)6);
                    var list = (IEnumerable<object>)value;
                    writer.Write(list is not null ? list.Count() : 0);
                    foreach (var item in list)
                    {
                        WriteValue(writer, item);
                    }
                    break;

                case Type _ when type.IsClass:
                    writer.Write((byte)7);
                    SerializeObject(value, writer);
                    break;

                default:
                    throw new NotSupportedException($"Type {type} is not supported.");
            }
        }

        private object ReadValue(BinaryReader reader, Type type)
        {
            byte typeIndicator = reader.ReadByte();

            switch (typeIndicator)
            {
                case 0: // Null indicator
                    return null;

                case 1: // int
                    return reader.ReadInt32();

                case 2: // decimal
                    return reader.ReadDecimal();

                case 3: // DateTime
                    return DateTime.FromBinary(reader.ReadInt64());

                case 4: // Guid
                    return new Guid(reader.ReadBytes(16));

                case 5: // string
                    {
                        int length = reader.ReadInt32();
                        var stringBytes = reader.ReadBytes(length);
                        return Encoding.UTF8.GetString(stringBytes);
                    }

                case 6: // List of objects
                    {
                        // Get the generic type arguments
                        Type listType = type.GetGenericArguments()[0];
                        // Create a non-generic IList to hold the items
                        IList list = (IList)Activator.CreateInstance(type);

                        int count = reader.ReadInt32(); 
                        for (int i = 0; i < count; i++)
                        {
                            // Read each item and add it to the list
                            object item = ReadValue(reader, listType);
                            list.Add(item);
                        }
                        return list;
                    }

                case 7: // Nested object
                    var obj = Activator.CreateInstance(type);
                    return DeserializeObjectWithAttributes(obj, reader);

                default:
                    throw new NotSupportedException($"Type indicator {typeIndicator} is not supported.");
            }
        }

        private void ValidateProperty(PropertyInfo property, object value)
        {
            // Required attribute
            if (Attribute.IsDefined(property, typeof(RequiredAttribute)) && value == null)
            {
                throw new InvalidOperationException($"Property {property.Name} is required.");
            }

            // MaxLength attribute
            if (Attribute.IsDefined(property, typeof(MaxLengthAttribute)) && value is string strValue)
            {
                var maxLength = ((MaxLengthAttribute)Attribute.GetCustomAttribute(property, typeof(MaxLengthAttribute))).Length;
                if (strValue.Length > maxLength)
                {
                    throw new InvalidOperationException($"Property {property.Name} exceeds maximum length of {maxLength}.");
                }
            }

        }

    }
}
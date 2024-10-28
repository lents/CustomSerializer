namespace CustomSerializer
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SerializablePropertyAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Property)]
    public class MaxLengthAttribute : Attribute
    {
        public int Length { get; }
        public MaxLengthAttribute(int length) => Length = length;
    }
}
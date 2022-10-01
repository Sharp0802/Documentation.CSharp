using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Documentation.CSharp.Test.A;

/// <summary>
/// text class.
/// </summary>
public class TestClass : ISerializable
{
    /// <summary>
    /// cctor info;
    /// </summary>
    static TestClass()
    {
        
    }
    
    /// <summary>
    /// ctor info;
    /// </summary>
    /// <param name="a">param a</param>
    [JsonConstructor]
    public TestClass(int a)
    {
        
    }
    
    /// <summary>
    /// <inheritdoc cref="ISerializable.GetObjectData(SerializationInfo,StreamingContext)"/>
    /// </summary>
    /// <param name="info">param info</param>
    /// <param name="context">param context</param>
    /// <exception cref="NotImplementedException">exception</exception>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// field info.
    /// </summary>
    public volatile int Field;
    
    /// <summary>
    /// indexer info;
    /// </summary>
    /// <param name="i">param i;</param>
    [Obsolete("comment")]
    public ref readonly int this[int i] => ref Field;
}
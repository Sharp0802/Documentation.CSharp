using System.Runtime.CompilerServices;

namespace Documentation.CSharp.Test.A;

[NativeCppClass]
public struct TestStruct : ICloneable
{
    /// <summary>
    /// fixed array field;
    /// </summary>
    public unsafe fixed int FArr[16];

    /// <summary>
    /// volatile field;
    /// </summary>
    public static volatile int VolatileField;

    /// <summary>
    /// pointer field;
    /// </summary>
    public unsafe int* PointerField;
    
    /// <summary>
    /// clone this;
    /// </summary>
    /// <returns>return deep copy of this</returns>
    public object Clone() => this;
}
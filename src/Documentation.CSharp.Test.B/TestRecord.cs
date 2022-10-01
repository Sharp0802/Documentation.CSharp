using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Documentation.CSharp.Test.B;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// If not created with belonging new <see cref="string"/> the properties for <see cref="Encoder"/>
/// and <see cref="RuntimeEnvironment"/> are <c>null</c>.
/// <para>Refactoring necessary 
/// <list type="bullet">
/// <item><description>a MonitorGroup can connect to several decoders</description></item>
/// <item><description>a MonitorGroup can be connected to several monitors.</description></item>
/// </list>
/// a MonitorGroup can connect to several decoders.</para>
/// </remarks>
/// <typeparam name="T1">type param 1 for <see cref="TestRecord{T1,T2}"/></typeparam>
/// <typeparam name="T2">type param 2 after <typeparamref name="T1"/> for <see cref="TestRecord{T1,T2}"/></typeparam>
public record TestRecord<T1, T2>
    where T1 : unmanaged, INumber<int> 
    where T2 : class
{
    /// <summary>
    /// test delegate.
    /// </summary>
    /// <typeparam name="T3">type param 3</typeparam>
    /// <typeparam name="T4">type param 4</typeparam>
    public delegate void TestDelegate<in T3, in T4>(T1 a, T2 b, T3 c, T4 d);
    
    /// <summary>
    /// <inheritdoc cref="StringBuilder.ToString()" />
    /// Adding Text
    /// </summary>
    /// <returns>returns AAA BBB CCC</returns>
    public override string ToString() => null!;

    /// <summary>
    /// const field description.
    /// </summary>
    public const bool Constant = false;

    /// <summary>
    /// test event description.
    /// </summary>
    public static event Action<T1, T2> TestEvent
    {
        add {} 
        remove {}
    }

    /// <summary>
    /// Gets a value indicating whether the IList has a fixed size.
    /// </summary>
    /// <value>
    /// true if the IList has a fixed size; otherwise, false. In the default implementation of List<![CDATA[<T>]]>, this property always returns false.
    /// </value>
    public string? Property { get; init; }

    /// <summary>
    /// summary for <see cref="Foo{T3,T4}"/>
    /// </summary>
    /// <param name="tester1">param 1 before <paramref name="tester2"/> for <see cref="Foo{T3,T4}"/></param>
    /// <param name="tester2">param 2 after <paramref name="tester1"/> for <see cref="Foo{T3,T4}"/></param>
    /// <param name="tester3">param 3 after <paramref name="tester2"/> for <see cref="Foo{T3,T4}"/></param>
    /// <typeparam name="T3">type param 1 for <see cref="Foo{T3,T4}"/></typeparam>
    /// <typeparam name="T4">type param 2 for <see cref="Foo{T3,T4}"/></typeparam>
    /// <returns>Does not return.</returns>
    /// <exception cref="EvaluateException">exception description.</exception>
    /// <seealso cref="ToString"/>
    /// <example>
    /// example description <c>inline code++</c>
    /// <code>
    /// namespace AAA;
    ///
    /// void Fo()
    /// {
    /// }
    /// </code>
    /// </example>
    [Obsolete(
        "abc," + "가나다," + "test", 
        true && Constant, 
        DiagnosticId = "diag" + "nostics"), 
     CLSCompliant(false), 
     MethodImpl(MethodImplOptions.Synchronized | MethodImplOptions.AggressiveInlining)]
    [return: MarshalAs(UnmanagedType.LPUTF8Str)]
    public static string Foo<T3, T4>(
        [DisallowNull] ref TestRecord<T1, T3> tester1,
        [NotNull] out TestRecord<T1, T2> tester2,
        [MaybeNull] in TestRecord<T4, T2> tester3)
        where T3 : class
        where T4 : unmanaged, INumber<int>
    {
        tester2 = new TestRecord<T1, T2>();
        throw new EvaluateException();
    }
}
namespace Documentation.CSharp.Test.B;

[Flags]
public enum TestEnum : uint
{
    EnumA = 0,
    EnumB = 2,
    EnumC,
    EnumD = 8,
    EnumE = EnumA
}
using System.Text.Json.Serialization;
using Documentation.CSharp.Compiler.Primitives;

namespace Documentation.CSharp.Compiler;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SerializeTarget))]
internal partial class PrimitiveSerializingContext : JsonSerializerContext
{
}
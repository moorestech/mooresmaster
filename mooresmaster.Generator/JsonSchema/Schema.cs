using System.Collections.Generic;
using mooresmaster.Generator.Json;
using mooresmaster.Generator.NameResolve;
using UnitGenerator;

namespace mooresmaster.Generator.JsonSchema;

public class SchemaTable
{
    public readonly Dictionary<SchemaId, ISchema> Table = new();

    public SchemaId Add(ISchema schema)
    {
        var id = SchemaId.New();
        Table.Add(id, schema);
        return id;
    }

    public void Add(SchemaId id, ISchema schema)
    {
        Table.Add(id, schema);
    }
}

public interface ISchema
{
    string? PropertyName { get; }
    bool IsNullable { get; }
    SchemaId? Parent { get; }
}

public interface IDefineInterfacePropertySchema : ISchema
{
}

public record Schema(string SchemaId, SchemaId InnerSchema, DefineInterface[] Interfaces)
{
    public SchemaId InnerSchema = InnerSchema;
    public DefineInterface[] Interfaces = Interfaces;
    public string SchemaId = SchemaId;
}

public record ObjectSchema(string? PropertyName, SchemaId? Parent, Dictionary<string, SchemaId> Properties, string[] Required, bool IsNullable, string[] InterfaceImplementations) : ISchema
{
    public string[] InterfaceImplementations = InterfaceImplementations;
    public Dictionary<string, SchemaId> Properties = Properties;
    public string[] Required = Required;
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record ArraySchema(string? PropertyName, SchemaId? Parent, SchemaId Items, JsonString? OverrideCodeGeneratePropertyName, bool IsNullable) : ISchema
{
    public SchemaId Items = Items;
    public JsonString? OverrideCodeGeneratePropertyName = OverrideCodeGeneratePropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
    public string? PropertyName { get; } = PropertyName;
}

public static class ArraySchemaExtension
{
    public static string GetPropertyName(this ArraySchema arraySchema)
    {
        if (arraySchema.OverrideCodeGeneratePropertyName != null) return arraySchema.OverrideCodeGeneratePropertyName.Literal;

        return $"{arraySchema.PropertyName!}Element";
    }
}

public record SwitchSchema(string? PropertyName, SchemaId? Parent, SwitchCaseSchema[] IfThenArray, bool IsNullable) : ISchema
{
    public SwitchCaseSchema[] IfThenArray = IfThenArray;
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record SwitchCaseSchema(SwitchPath SwitchReferencePath, string When, SchemaId Schema)
{
    public SwitchPath SwitchReferencePath = SwitchReferencePath;
    public SchemaId Schema = Schema;
    public string When = When;
}

public record StringSchema(string? PropertyName, SchemaId? Parent, bool IsNullable, string[]? Enums) : ISchema, IDefineInterfacePropertySchema
{
    public string[]? Enums = Enums;
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record NumberSchema(string? PropertyName, SchemaId? Parent, bool IsNullable) : ISchema, IDefineInterfacePropertySchema
{
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record IntegerSchema(string? PropertyName, SchemaId? Parent, bool IsNullable) : ISchema, IDefineInterfacePropertySchema
{
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record BooleanSchema(string? PropertyName, SchemaId? Parent, bool IsNullable) : ISchema, IDefineInterfacePropertySchema
{
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record RefSchema(string? PropertyName, SchemaId? Parent, string Ref, bool IsNullable) : ISchema, IDefineInterfacePropertySchema
{
    public string Ref = Ref;
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;

    public TypeName GetRefName()
    {
        return new TypeName(Ref, $"{Ref}Module");
    }
}

public record Vector2Schema(string? PropertyName, SchemaId? Parent, bool IsNullable) : IDefineInterfacePropertySchema
{
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record Vector3Schema(string? PropertyName, SchemaId? Parent, bool IsNullable) : IDefineInterfacePropertySchema
{
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record Vector4Schema(string? PropertyName, SchemaId? Parent, bool IsNullable) : IDefineInterfacePropertySchema
{
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record Vector2IntSchema(string? PropertyName, SchemaId? Parent, bool IsNullable) : IDefineInterfacePropertySchema
{
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record Vector3IntSchema(string? PropertyName, SchemaId? Parent, bool IsNullable) : IDefineInterfacePropertySchema
{
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

public record UUIDSchema(string? PropertyName, SchemaId? Parent, bool IsNullable) : IDefineInterfacePropertySchema
{
    public string? PropertyName { get; } = PropertyName;
    public bool IsNullable { get; } = IsNullable;
    public SchemaId? Parent { get; } = Parent;
}

[UnitOf(typeof(MasterId<SchemaId>))]
public readonly partial struct SchemaId
{
    public static SchemaId New()
    {
        return new SchemaId(new MasterId<SchemaId>());
    }
}

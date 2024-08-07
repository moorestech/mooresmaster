using System;
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
    SchemaId? Parent { get; }
}

public record Schema(string SchemaId, SchemaId InnerSchema)
{
    public SchemaId InnerSchema = InnerSchema;
    public string SchemaId = SchemaId;
}

public record ObjectSchema(string? PropertyName, SchemaId? Parent, Dictionary<string, SchemaId> Properties, string[] Required) : ISchema
{
    public Dictionary<string, SchemaId> Properties = Properties;
    public string[] Required = Required;
    public string? PropertyName { get; } = PropertyName;
    public SchemaId? Parent { get; } = Parent;
}

public record ArraySchema(string? PropertyName, SchemaId? Parent, SchemaId Items, JsonString? Pattern) : ISchema
{
    public SchemaId Items = Items;
    public JsonString? Pattern = Pattern;
    public string? PropertyName { get; } = PropertyName;
    public SchemaId? Parent { get; } = Parent;
}

public record OneOfSchema(string? PropertyName, SchemaId? Parent, IfThenSchema[] IfThenArray) : ISchema
{
    public IfThenSchema[] IfThenArray = IfThenArray;
    public string? PropertyName { get; } = PropertyName;
    public SchemaId? Parent { get; } = Parent;
}

public record IfThenSchema(JsonObject If, SchemaId Then)
{
    public JsonObject If = If;
    public SchemaId Then = Then;
}

public record StringSchema(string? PropertyName, SchemaId? Parent, JsonString? Format) : ISchema
{
    public JsonString? Format = Format;
    public string? PropertyName { get; } = PropertyName;
    public SchemaId? Parent { get; } = Parent;
}

public record NumberSchema(string? PropertyName, SchemaId? Parent) : ISchema
{
    public string? PropertyName { get; } = PropertyName;
    public SchemaId? Parent { get; } = Parent;
}

public record IntegerSchema(string? PropertyName, SchemaId? Parent) : ISchema
{
    public string? PropertyName { get; } = PropertyName;
    public SchemaId? Parent { get; } = Parent;
}

public record BooleanSchema(string? PropertyName, SchemaId? Parent) : ISchema
{
    public string? PropertyName { get; } = PropertyName;
    public SchemaId? Parent { get; } = Parent;
}

public record RefSchema(string? PropertyName, SchemaId? Parent, string Ref) : ISchema
{
    public string Ref = Ref;
    public string? PropertyName { get; } = PropertyName;
    public SchemaId? Parent { get; } = Parent;

    public TypeName GetRefName()
    {
        return new TypeName(Ref, $"{Ref}Module");
    }
}

[UnitOf(typeof(Guid))]
public readonly partial struct SchemaId;

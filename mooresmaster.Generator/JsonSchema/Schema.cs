using System.Collections.Generic;
using mooresmaster.Generator.Json;

namespace mooresmaster.Generator.JsonSchema;

public interface ISchema;

public record Schema(string SchemaId, ISchema InnerSchema)
{
    public string SchemaId = SchemaId;
    public ISchema InnerSchema = InnerSchema;
}

public record ObjectSchema(Dictionary<string, ISchema> Properties, string[] Required) : ISchema
{
    public Dictionary<string, ISchema> Properties = Properties;
    public string[] Required = Required;
}

public record ArraySchema(ISchema Items, JsonString? Pattern) : ISchema
{
    public ISchema Items = Items;
    public JsonString? Pattern = Pattern;
}

public record OneOfSchema(IfThenSchema[] IfThenArray) : ISchema
{
    public IfThenSchema[] IfThenArray = IfThenArray;
}

public record IfThenSchema(JsonObject If, ISchema Then)
{
    public JsonObject If = If;
    public ISchema Then = Then;
}

public record StringSchema(JsonString? Format) : ISchema
{
    public JsonString? Format = Format;
}

public record NumberSchema : ISchema;

public record IntegerSchema : ISchema;

public record BooleanSchema : ISchema;

public record RefSchema(string Ref) : ISchema
{
    public string Ref = Ref;
}
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using mooresmaster.Generator.JsonSchema;

namespace mooresmaster.Generator.Semantic;

public static class SemanticsGenerator
{
    public static Semantics Generate(ImmutableArray<Schema> schemaArray, SchemaTable table)
    {
        var semantics = new Semantics();

        foreach (var schema in schemaArray)
            // ファイルに分けられているルートの要素はclassになる
            // ただし、objectSchemaだった場合のちのGenerateで生成されるため、ここでは生成しない
            if (table.Table[schema.InnerSchema] is ObjectSchema objectSchema)
            {
                var (innerSemantics, id) = Generate(objectSchema, table);
                semantics.AddRootSemantics(new RootSemantics(schema, id));
                innerSemantics.AddTo(semantics);
            }
            else
            {
                var typeSemantics = new TypeSemantics([], table.Table[schema.InnerSchema]);
                var typeId = semantics.AddTypeSemantics(typeSemantics);
                semantics.AddRootSemantics(new RootSemantics(schema, typeId));

                Generate(table.Table[schema.InnerSchema], table).AddTo(semantics);
            }

        return semantics;
    }

    private static Semantics Generate(ISchema schema, SchemaTable table)
    {
        var semantics = new Semantics();

        switch (schema)
        {
            case ArraySchema arraySchema:
                Generate(table.Table[arraySchema.Items], table).AddTo(semantics);
                break;
            case ObjectSchema objectSchema:
                var (innerSemantics, _) = Generate(objectSchema, table);
                innerSemantics.AddTo(semantics);
                break;
            case OneOfSchema oneOfSchema:
                var (oneOfInnerSemantics, _) = Generate(oneOfSchema, table);
                oneOfInnerSemantics.AddTo(semantics);
                break;
            case RefSchema refSchema:
            case BooleanSchema booleanSchema:
            case IntegerSchema integerSchema:
            case NumberSchema numberSchema:
            case StringSchema stringSchema:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(schema));
        }

        return semantics;
    }

    private static (Semantics, InterfaceId) Generate(OneOfSchema oneOfSchema, SchemaTable table)
    {
        var semantics = new Semantics();
        var interfaceId = semantics.AddInterfaceSemantics(new InterfaceSemantics(oneOfSchema));
        foreach (var ifThen in oneOfSchema.IfThenArray)
        {
            Generate(table.Table[ifThen.Then], table).AddTo(semantics);

            var then = semantics.SchemaTypeSemanticsTable[table.Table[ifThen.Then]];
            semantics.InheritList.Add((interfaceId, then));
        }

        return (semantics, interfaceId);
    }

    private static (Semantics, ClassId) Generate(ObjectSchema objectSchema, SchemaTable table)
    {
        var semantics = new Semantics();
        var typeId = ClassId.New();
        var properties = new List<PropertyId>();
        foreach (var property in objectSchema.Properties)
        {
            var schema = table.Table[property.Value];
            switch (schema)
            {
                case ObjectSchema innerObjectSchema:
                    var (objectInnerSemantics, objectInnerTypeId) = Generate(innerObjectSchema, table);
                    objectInnerSemantics.AddTo(semantics);
                    properties.Add(semantics.AddPropertySemantics(
                        new PropertySemantics(
                            typeId,
                            property.Key,
                            objectInnerTypeId,
                            schema
                        )
                    ));
                    break;
                case OneOfSchema oneOfSchema:
                    var (oneOfInnerSemantics, oneOfInnerTypeId) = Generate(oneOfSchema, table);
                    oneOfInnerSemantics.AddTo(semantics);
                    properties.Add(semantics.AddPropertySemantics(
                        new PropertySemantics(
                            typeId,
                            property.Key,
                            oneOfInnerTypeId,
                            schema
                        )
                    ));
                    break;
                default:
                    Generate(table.Table[property.Value], table).AddTo(semantics);
                    properties.Add(semantics.AddPropertySemantics(
                        new PropertySemantics(
                            typeId,
                            property.Key,
                            null,
                            schema
                        )
                    ));
                    break;
            }
        }

        var typeSemantics = new TypeSemantics(properties.ToArray(), objectSchema);
        semantics.TypeSemanticsTable[typeId] = typeSemantics;
        semantics.SchemaTypeSemanticsTable[typeSemantics.Schema] = typeId;

        return (semantics, typeId);
    }
}
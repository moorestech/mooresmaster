using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using mooresmaster.Generator.JsonSchema;

namespace mooresmaster.Generator.Semantic;

public static class SemanticsGenerator
{
    public static Semantics Generate(ImmutableArray<Schema> schemaArray, SchemaTable table)
    {
        var semantics = new Semantics();

        foreach (var schema in schemaArray)
        {
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

            foreach (var defineInterface in schema.Interfaces)
                GenerateInterfaceSemantics(defineInterface, schema, table).AddTo(semantics);
        }

        ResolveInterfaceInterfaceImplementations(semantics);
        ResolveClassInterfaceImplementations(semantics);

        return semantics;
    }

    private static void ResolveInterfaceInterfaceImplementations(Semantics semantics)
    {
        var interfaceTable = semantics.InterfaceSemanticsTable.ToDictionary(kvp => kvp.Value.Interface.InterfaceName, kvp => kvp.Key);

        foreach (var kvp in semantics.InterfaceSemanticsTable)
        {
            var target = kvp.Key;

            foreach (var interfaceId in kvp.Value.Interface.ImplementationInterfaces)
            {
                var other = interfaceTable[interfaceId];
                semantics.AddInterfaceInterfaceImplementation(target, other);
            }
        }
    }

    private static void ResolveClassInterfaceImplementations(Semantics semantics)
    {
        var interfaceTable = semantics.InterfaceSemanticsTable.ToDictionary(kvp => kvp.Value.Interface.InterfaceName, kvp => kvp.Key);

        foreach (var kvp in semantics.TypeSemanticsTable)
        {
            if (kvp.Value.Schema is not ObjectSchema objectSchema) continue;

            var target = kvp.Key;

            foreach (var interfaceName in objectSchema.InterfaceImplementations)
            {
                var interfaceId = interfaceTable[interfaceName];
                semantics.AddClassInterfaceImplementation(target, interfaceId);
            }
        }
    }

    private static Semantics GenerateInterfaceSemantics(DefineInterface defineInterface, Schema schema, SchemaTable table)
    {
        var semantics = new Semantics();

        var interfaceId = InterfaceId.New();

        List<InterfacePropertyId> propertyIds = new();
        foreach (var property in defineInterface.Properties)
        {
            var propertySchema = property.Value;

            var propertyId = semantics.AddInterfacePropertySemantics(new InterfacePropertySemantics(propertySchema, interfaceId));
            propertyIds.Add(propertyId);
        }

        semantics.InterfaceSemanticsTable[interfaceId] = new InterfaceSemantics(
            schema,
            defineInterface,
            propertyIds.ToArray()
        );

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
            case SwitchSchema oneOfSchema:
                var (oneOfInnerSemantics, _) = Generate(oneOfSchema, table);
                oneOfInnerSemantics.AddTo(semantics);
                break;
            case RefSchema:
            case BooleanSchema:
            case IntegerSchema:
            case NumberSchema:
            case StringSchema:
            case UUIDSchema:
            case Vector2Schema:
            case Vector3Schema:
            case Vector4Schema:
            case Vector2IntSchema:
            case Vector3IntSchema:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(schema));
        }

        return semantics;
    }

    private static (Semantics, SwitchId) Generate(SwitchSchema switchSchema, SchemaTable table)
    {
        var semantics = new Semantics();

        var interfaceId = SwitchId.New();
        List<(SwitchPath, string, ClassId)> thenList = new();
        foreach (var ifThen in switchSchema.IfThenArray)
        {
            Generate(table.Table[ifThen.Schema], table).AddTo(semantics);

            var then = semantics.SchemaTypeSemanticsTable[table.Table[ifThen.Schema]];
            semantics.SwitchInheritList.Add((interfaceId, then));
            thenList.Add((ifThen.SwitchReferencePath, ifThen.When, then));
        }

        semantics.SwitchSemanticsTable.Add(interfaceId, new SwitchSemantics(switchSchema, thenList.ToArray()));

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
                            schema,
                            schema.IsNullable
                        )
                    ));
                    break;
                case SwitchSchema oneOfSchema:
                    var (oneOfInnerSemantics, oneOfInnerTypeId) = Generate(oneOfSchema, table);
                    oneOfInnerSemantics.AddTo(semantics);
                    properties.Add(semantics.AddPropertySemantics(
                        new PropertySemantics(
                            typeId,
                            property.Key,
                            oneOfInnerTypeId,
                            schema,
                            schema.IsNullable
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
                            schema,
                            schema.IsNullable
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

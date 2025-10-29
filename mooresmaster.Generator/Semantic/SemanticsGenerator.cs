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
            var rootId = RootId.New();
            
            // ファイルに分けられているルートの要素はclassになる
            // ただし、objectSchemaだった場合のちのGenerateで生成されるため、ここでは生成しない
            if (table.Table[schema.InnerSchema] is ObjectSchema objectSchema)
            {
                var (innerSemantics, id) = Generate(objectSchema, table, rootId);
                semantics.RootSemanticsTable.Add(rootId, new RootSemantics(schema, id));
                innerSemantics.AddTo(semantics);
            }
            else
            {
                var typeSemantics = new TypeSemantics([], table.Table[schema.InnerSchema], rootId);
                var typeId = semantics.AddTypeSemantics(typeSemantics);
                semantics.RootSemanticsTable.Add(rootId, new RootSemantics(schema, typeId));
                
                Generate(table.Table[schema.InnerSchema], table, rootId).AddTo(semantics);
            }
            
            foreach (var defineInterface in schema.Interfaces)
                GenerateInterfaceSemantics(defineInterface, schema, table, rootId).AddTo(semantics);
        }
        
        ResolveInterfaceInterfaceImplementations(semantics);
        ResolveClassInterfaceImplementations(semantics);
        
        return semantics;
    }
    
    private static void ResolveInterfaceInterfaceImplementations(Semantics semantics)
    {
        var allInterfaceTable = semantics.InterfaceSemanticsTable
            .ToDictionary(kvp => kvp.Value.Interface.InterfaceName, kvp => kvp.Key);
        
        var globalInterfaceTable = semantics.InterfaceSemanticsTable
            .Where(i => i.Value.Interface.IsGlobal)
            .ToDictionary(kvp => kvp.Value.Interface.InterfaceName, kvp => kvp.Key);
        
        foreach (var kvp in semantics.InterfaceSemanticsTable)
        {
            var target = kvp.Key;
            var localInterfaceTable = semantics.InterfaceSemanticsTable
                .Where(i => i.Value.Schema.SchemaId == kvp.Value.Schema.SchemaId)
                .Where(i => !i.Value.Interface.IsGlobal)
                .ToDictionary(kvp => kvp.Value.Interface.InterfaceName, kvp => kvp.Key);
            
            foreach (var interfaceName in kvp.Value.Interface.ImplementationInterfaces)
                if (localInterfaceTable.TryGetValue(interfaceName, out var localOther))
                    semantics.AddInterfaceInterfaceImplementation(target, localOther);
                else if (globalInterfaceTable.TryGetValue(interfaceName, out var globalOther))
                    semantics.AddInterfaceInterfaceImplementation(target, globalOther);
                else
                    semantics.AddInterfaceInterfaceImplementation(target, allInterfaceTable[interfaceName]);
        }
    }
    
    private static void ResolveClassInterfaceImplementations(Semantics semantics)
    {
        var globalInterfaceTable = semantics.InterfaceSemanticsTable
            .Where(i => i.Value.Interface.IsGlobal)
            .ToDictionary(kvp => kvp.Value.Interface.InterfaceName, kvp => kvp.Key);
        
        foreach (var kvp in semantics.TypeSemanticsTable)
        {
            if (kvp.Value.Schema is not ObjectSchema objectSchema) continue;
            var localInterfaceTable = semantics.InterfaceSemanticsTable
                .Where(i => i.Value.Schema.SchemaId == semantics.RootSemanticsTable[kvp.Value.RootId].Root.SchemaId)
                .Where(i => !i.Value.Interface.IsGlobal)
                .ToDictionary(kvp => kvp.Value.Interface.InterfaceName, kvp => kvp.Key);
            
            var target = kvp.Key;
            
            foreach (var interfaceName in objectSchema.InterfaceImplementations)
                if (localInterfaceTable.TryGetValue(interfaceName, out var localOther))
                {
                    semantics.AddClassInterfaceImplementation(target, localOther);
                }
                else
                {
                    var globalOther = globalInterfaceTable[interfaceName];
                    semantics.AddClassInterfaceImplementation(target, globalOther);
                }
        }
    }
    
    private static Semantics GenerateInterfaceSemantics(DefineInterface defineInterface, Schema schema, SchemaTable table, RootId rootId)
    {
        var semantics = new Semantics();
        
        var interfaceId = InterfaceId.New();
        
        List<InterfacePropertyId> propertyIds = new();
        foreach (var property in defineInterface.Properties)
        {
            var propertySchema = property.Value;
            
            Generate(propertySchema, table, rootId).AddTo(semantics);
            
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
    
    private static Semantics Generate(ISchema schema, SchemaTable table, RootId rootId)
    {
        var semantics = new Semantics();
        
        switch (schema)
        {
            case ArraySchema arraySchema:
                Generate(table.Table[arraySchema.Items], table, rootId).AddTo(semantics);
                break;
            case ObjectSchema objectSchema:
                var (innerSemantics, _) = Generate(objectSchema, table, rootId);
                innerSemantics.AddTo(semantics);
                break;
            case SwitchSchema oneOfSchema:
                var (oneOfInnerSemantics, _) = Generate(oneOfSchema, table, rootId);
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
    
    private static (Semantics, SwitchId) Generate(SwitchSchema switchSchema, SchemaTable table, RootId rootId)
    {
        var semantics = new Semantics();
        
        var interfaceId = SwitchId.New();
        List<(SwitchPath, string, ClassId)> thenList = new();
        foreach (var ifThen in switchSchema.IfThenArray)
        {
            Generate(table.Table[ifThen.Schema], table, rootId).AddTo(semantics);
            
            var then = semantics.SchemaTypeSemanticsTable[table.Table[ifThen.Schema]];
            semantics.SwitchInheritList.Add((interfaceId, then));
            thenList.Add((ifThen.SwitchReferencePath, ifThen.When, then));
        }
        
        semantics.SwitchSemanticsTable.Add(interfaceId, new SwitchSemantics(switchSchema, thenList.ToArray()));
        
        return (semantics, interfaceId);
    }
    
    private static (Semantics, ClassId) Generate(ObjectSchema objectSchema, SchemaTable table, RootId rootId)
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
                    var (objectInnerSemantics, objectInnerTypeId) = Generate(innerObjectSchema, table, rootId);
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
                    var (oneOfInnerSemantics, oneOfInnerTypeId) = Generate(oneOfSchema, table, rootId);
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
                    Generate(table.Table[property.Value], table, rootId).AddTo(semantics);
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
        
        var typeSemantics = new TypeSemantics(properties.ToArray(), objectSchema, rootId);
        semantics.TypeSemanticsTable[typeId] = typeSemantics;
        semantics.SchemaTypeSemanticsTable[typeSemantics.Schema] = typeId;
        
        return (semantics, typeId);
    }
}
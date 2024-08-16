using System;
using System.Linq;
using mooresmaster.Generator.JsonSchema;
using mooresmaster.Generator.NameResolve;
using mooresmaster.Generator.Semantic;

namespace mooresmaster.Generator.Definitions;

public record Type
{
    public static Type GetType(NameTable nameTable, ITypeId? typeId, ISchema schema, Semantics semantics, SchemaTable schemaTable)
    {
        return schema switch
        {
            ArraySchema arraySchema => arraySchema.Pattern?.Literal switch
            {
                "@vector2" => new Vector2Type(),
                "@vector3" => new Vector3Type(),
                "@vector4" => new Vector4Type(),
                "@vector2Int" => new Vector2IntType(),
                "@vector3Int" => new Vector3IntType(),
                _ => new ArrayType(GetType(
                    nameTable,
                    semantics.SchemaTypeSemanticsTable.ContainsKey(schemaTable.Table[arraySchema.Items])
                        ? semantics.SchemaTypeSemanticsTable[schemaTable.Table[arraySchema.Items]]
                        : null,
                    schemaTable.Table[arraySchema.Items],
                    semantics,
                    schemaTable
                ))
            },
            BooleanSchema => new BooleanType(),
            IntegerSchema => new IntType(),
            NumberSchema => new FloatType(),
            StringSchema stringSchema => GetStringSchemaType(stringSchema,semantics),
            ObjectSchema => new CustomType(nameTable.TypeNames[typeId]),
            OneOfSchema => new CustomType(nameTable.TypeNames[typeId]),
            RefSchema refSchema => new CustomType(nameTable.TypeNames[GetRefTypeId(refSchema, semantics)]),
            _ => throw new ArgumentOutOfRangeException(nameof(schema))
        };
    }

    private static ITypeId GetRefTypeId(RefSchema schema, Semantics semantics)
    {
        var schemaClassId = semantics.RootSemanticsTable.First(root => root.Value.Root.SchemaId == schema.Ref).Value.ClassId;

        return schemaClassId;
    }

    private static Type GetStringSchemaType(StringSchema stringSchema, Semantics semantics)
    {
        if (stringSchema.UseCustomType?.Literal == true)
        {
            return new StringCustomType(stringSchema.PropertyName);
        }
        if (stringSchema.ForeignKey != null)
        {
            //外部キーのフォーマットは json名:data配下のプロパティ名:表示するプロパティ名 となっているので、:区切りの左2つが、ここで意味のあるものになる
            //参考：https://discord.com/channels/888315641709330433/1255119998720933909/1264855303850164356
            ClassId classId = new ClassId(); // TODO 何らかの方法で外部キーのターゲットとなるカスタムタイプを取得する 外部キーとセマンティクスを対応付けするテーブルを作ってもいいかも
            
            var foreignKeySchema = semantics.TypeSemanticsTable[classId].Schema as StringSchema;
            return GetStringSchemaType(foreignKeySchema, semantics);
        }
        
        return stringSchema.Format?.Literal switch
        {
            "uuid" => new UUIDType(),
            _ => new StringType()
        };
    }

    public string GetName()
    {
        return this switch
        {
            BooleanType booleanType => "bool",
            ArrayType arrayType => $"{arrayType.InnerType.GetName()}[]",
            DictionaryType dictionaryType => $"global::System.Collections.Generic.Dictionary<{dictionaryType.KeyType.GetName()}, {dictionaryType.ValueType.GetName()}>",
            FloatType floatType => "float",
            IntType intType => "int",
            StringType stringType => "string",
            UUIDType uuidType => "global::System.Guid",
            Vector2IntType vector2IntType => "global::UnityEngine.Vector2Int",
            Vector2Type vector2Type => "global::UnityEngine.Vector2",
            Vector3IntType vector3IntType => "global::UnityEngine.Vector3Int",
            Vector3Type vector3Type => "global::UnityEngine.Vector3",
            Vector4Type vector4Type => "global::UnityEngine.Vector4",
            CustomType customType => customType.Name.GetModelName(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public record BuiltinType : Type;

public record StringType : BuiltinType;

public record BooleanType : BuiltinType;

public record IntType : BuiltinType;

public record FloatType : BuiltinType;

public record ArrayType(Type InnerType) : BuiltinType
{
    public Type InnerType = InnerType;
}

public record Vector2Type : BuiltinType;

public record Vector3Type : BuiltinType;

public record Vector2IntType : BuiltinType;

public record Vector3IntType : BuiltinType;

public record Vector4Type : BuiltinType;

public record UUIDType : BuiltinType;

// 下のCustomTypeとほぼ同じなので、統合してもいいと思います
public record StringCustomType : BuiltinType;

public record DictionaryType(Type KeyType, Type ValueType) : BuiltinType
{
    public Type KeyType = KeyType;
    public Type ValueType = ValueType;
}

public record CustomType(TypeName Name) : Type
{
    public TypeName Name = Name;
}

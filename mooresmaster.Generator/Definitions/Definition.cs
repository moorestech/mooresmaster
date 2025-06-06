using System.Collections.Generic;
using mooresmaster.Generator.NameResolve;
using mooresmaster.Generator.Semantic;

namespace mooresmaster.Generator.Definitions;

public record Definition
{
    public readonly List<InterfaceDefinition> InterfaceDefinitions = new();
    public readonly List<TypeDefinition> TypeDefinitions = new();
}

public record InterfaceDefinition(string FileName, TypeName TypeName, Dictionary<string, InterfacePropertyDefinition> PropertyTable, List<TypeName> ImplementationList)
{
    public string FileName = FileName;
    public List<TypeName> ImplementationList = ImplementationList;
    public Dictionary<string, InterfacePropertyDefinition> PropertyTable = PropertyTable;
    public TypeName TypeName = TypeName;
}

public record TypeDefinition(string FileName, TypeName TypeName, TypeName[] InheritList, Dictionary<string, PropertyDefinition> PropertyTable)
{
    public string FileName = FileName;
    public TypeName[] InheritList = InheritList;
    public Dictionary<string, PropertyDefinition> PropertyTable = PropertyTable;
    public TypeName TypeName = TypeName;
}

public record struct PropertyDefinition(Type Type, string PropertyName, PropertyId? PropertyId, bool IsNullable, string[]? Enums);

public record struct InterfacePropertyDefinition(Type Type);

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using mooresmaster.Generator.Definitions;
using mooresmaster.Generator.JsonSchema;
using mooresmaster.Generator.Semantic;

namespace mooresmaster.Generator.Analyze;

public class Analyzer
{
    private readonly List<IPostDefinitionLayerAnalyzer> _postDefinitionLayerAnalyzers = new();
    private readonly List<IPostJsonSchemaLayerAnalyzer> _postJsonSchemaLayerAnalyzers = new();
    private readonly List<IPostSemanticsLayerAnalyzer> _postSemanticsLayerAnalyzers = new();
    private readonly List<IPreDefinitionLayerAnalyzer> _preDefinitionLayerAnalyzers = new();
    private readonly List<IPreJsonSchemaLayerAnalyzer> _preJsonSchemaLayerAnalyzers = new();
    private readonly List<IPreSemanticsLayerAnalyzer> _preSemanticsLayerAnalyzers = new();
    
    public Analyzer AddAnalyzer(IAnalyzer analyzer)
    {
        if (analyzer is IPostSemanticsLayerAnalyzer postSemanticsLayerAnalyzer) _postSemanticsLayerAnalyzers.Add(postSemanticsLayerAnalyzer);
        if (analyzer is IPreSemanticsLayerAnalyzer preSemanticsLayerAnalyzer) _preSemanticsLayerAnalyzers.Add(preSemanticsLayerAnalyzer);
        if (analyzer is IPostDefinitionLayerAnalyzer postDefinitionLayerAnalyzer) _postDefinitionLayerAnalyzers.Add(postDefinitionLayerAnalyzer);
        if (analyzer is IPreDefinitionLayerAnalyzer preDefinitionLayerAnalyzer) _preDefinitionLayerAnalyzers.Add(preDefinitionLayerAnalyzer);
        if (analyzer is IPostJsonSchemaLayerAnalyzer postJsonSchemaLayerAnalyzer) _postJsonSchemaLayerAnalyzers.Add(postJsonSchemaLayerAnalyzer);
        if (analyzer is IPreJsonSchemaLayerAnalyzer preJsonSchemaLayerAnalyzer) _preJsonSchemaLayerAnalyzers.Add(preJsonSchemaLayerAnalyzer);
        
        return this;
    }
    
    public void PostJsonSchemaLayerAnalyze(Analysis analysis, SchemaTable schemaTable)
    {
        foreach (var postJsonSchemaLayerAnalyzer in _postJsonSchemaLayerAnalyzers) postJsonSchemaLayerAnalyzer.PostJsonSchemaLayerAnalyze(analysis, schemaTable);
    }
    
    public void PreJsonSchemaLayerAnalyze(Analysis analysis, ImmutableArray<AdditionalText> texts)
    {
        foreach (var preJsonSchemaLayerAnalyzer in _preJsonSchemaLayerAnalyzers) preJsonSchemaLayerAnalyzer.PreJsonSchemaLayerAnalyze(analysis, texts);
    }
    
    public void PostSemanticsLayerAnalyze(Analysis analysis, Semantics semantics, SchemaTable schemaTable)
    {
        foreach (var postSemanticsLayerAnalyzer in _postSemanticsLayerAnalyzers) postSemanticsLayerAnalyzer.PostSemanticsLayerAnalyze(analysis, semantics, schemaTable);
    }
    
    public void PreSemanticsLayerAnalyze(Analysis analysis, SchemaTable schemaTable)
    {
        foreach (var preSemanticsLayerAnalyzer in _preSemanticsLayerAnalyzers) preSemanticsLayerAnalyzer.PreSemanticsLayerAnalyze(analysis, schemaTable);
    }
    
    public void PostDefinitionLayerAnalyze(Analysis analysis, Semantics semantics, SchemaTable schemaTable, Definition definition)
    {
        foreach (var postDefinitionLayerAnalyzer in _postDefinitionLayerAnalyzers) postDefinitionLayerAnalyzer.PostDefinitionLayerAnalyze(analysis, semantics, schemaTable, definition);
    }
    
    public void PreDefinitionLayerAnalyze(Analysis analysis, Semantics semantics, SchemaTable schemaTable)
    {
        foreach (var preDefinitionLayerAnalyzer in _preDefinitionLayerAnalyzers) preDefinitionLayerAnalyzer.PreDefinitionLayerAnalyze(analysis, semantics, schemaTable);
    }
}

public interface IAnalyzer;

public interface IPostJsonSchemaLayerAnalyzer : IAnalyzer
{
    void PostJsonSchemaLayerAnalyze(Analysis analysis, SchemaTable schemaTable);
}

public interface IPreJsonSchemaLayerAnalyzer : IAnalyzer
{
    void PreJsonSchemaLayerAnalyze(Analysis analysis, ImmutableArray<AdditionalText> texts);
}

public interface IPostSemanticsLayerAnalyzer : IAnalyzer
{
    void PostSemanticsLayerAnalyze(Analysis analysis, Semantics semantics, SchemaTable schemaTable);
}

public interface IPreSemanticsLayerAnalyzer : IAnalyzer
{
    void PreSemanticsLayerAnalyze(Analysis analysis, SchemaTable schemaTable);
}

public interface IPostDefinitionLayerAnalyzer : IAnalyzer
{
    void PostDefinitionLayerAnalyze(Analysis analysis, Semantics semantics, SchemaTable schemaTable, Definition definition);
}

public interface IPreDefinitionLayerAnalyzer : IAnalyzer
{
    void PreDefinitionLayerAnalyze(Analysis analysis, Semantics semantics, SchemaTable schemaTable);
}
# LucidDocumentation `assembly`
- [LucidDocumentation](#LucidDocumentation)
   - [DocumentationBuilder](#DocumentationBuilder)

## DocumentationBuilder `class`
##### Namespace: `LucidDocumentation`
Core static class for building documentation.

---

#### #ctor `method`
```c#
public  DocumentationBuilder(string packageResolutionPath)
```
| Parameter | Type        | Default | Summary         |
|: --- :|: --------- :|: -------|: -------------- |
| packageResolutionPath | `String` |  | The folder path to resolve any dependent nuget packages from. |
###### Detail:
Creates a DocumentationBuilder object for use in generating documentation for any number of assemblies.

```c#
var docBuilder = new DocumentationBuilder("c:\packages\");
```

---

#### BuildDocumentation `method`
```c#
public String BuildDocumentation(string assemblyPath)
```
| Parameter | Type        | Default | Summary         |
|: --- :|: --------- :|: -------|: -------------- |
| assemblyPath | `String` |  | The absolute path of the assembly to document. |
###### Returns: `String`
Markdown formated documentation
###### Detail:
Returns documentation in standard Markdown format for the assembly at the `assemblyPath` provided. The referenced assemblie's
generated documentation `.xml` file must be present in the same folder.
###### Usage:

```c#
var markdown = docBuilder.BuildDocumentation("c:\SampleAssembly.dll");
```

---



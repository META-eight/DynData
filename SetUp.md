### Preparation

Within a customization of an Epicor client screen, in 'Script Editor', there is a 'Script' block.

*Outside* the Script code, between the 'using's and the Script, you can paste the entire code from `DynData.cs`.

Before saving, ensure there are references to custom assemblies `Ice.Contracts.BO.DynamicQuery.dll` and `Ice.Core.Session.dll`. The DynData classes are then usable to create objects within the Script code.

### Initial Set-up

The standard implementation pattern has the following common Script-level variables (declared after `// Add Custom Module Level Variables Here **`):

```c#
    private Dictionary<string,DynData> baqs;
```

plus one DynData variable per BAQ required in the customisation, in the form

```c#
    private DynData ddXXXX;
```

where 'XXXX' is something meaningful.


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

Within the existing `InitializeCustomCode()` method, before the `// ** Wizard Insert Location`, call `InitialiseCode();`, which is then declared as a method within Script:

```c#
    private void InitialiseCode() 
    { 
        baqs = new Dictionary<string,DynData>(); 

        ddXXXX = new DynData("BAQNAME", oTrans); 
        ddXXXX.Initialise( 
            new string[] {"param1", "param2" ...}, //BAQ parameter names, or this line null if no parameters 
            new string[] {string.Empty, "-1" ...}, //BAQ parameter defaults, one for each parameter in the same order, or null if none 
            new string[] {"BAQ_KeyField1", "BAQ_KeyField2" ...}, //as many field identifiers as needed for a unique row reference 
            new string[] {"Caption1", "Caption2" ...}, //full list of the BAQ field CAPTIONS that need to be visible in a grid 
            new int[] {100, 80 ...}, //column width per visible field, one for each in the above string array 
            new string[] {"Caption1", "Caption2" ...}, //list of BAQ field captions that need special formating, or null if none 
            new string[] {"yyyy-MMM", "#,##0.00" ...}, //matching list of format strings to the above 
            grdXXXX); //the name of the EpiUltraGrid you want the data to appear in 
        baqs["BAQNAME"] = ddXXXX; 
    } 
```

Note that three of the array parameters are paired and MUST have matching numbers of elements: the first and second (parameters), fourth and fifth (captions and columns) and six and seventh (captions and formats). For technical convention reasons within the workings, DynData relies on the column captions rather than column names for many functions.
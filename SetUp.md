## DynData Initial Set-up

### Preparation

Within a customization of an Epicor client screen, in 'Script Editor', there is a 'Script' block.

*Outside* the Script code, between the 'using's and the Script, you can paste the entire code from `DynData.cs`.

Before saving, ensure there are references to custom assemblies `Ice.Contracts.BO.DynamicQuery.dll` and `Ice.Core.Session.dll`. The DynData classes are then usable to create objects within the Script code.

There will also need to be an EpiUltraGrid in the screen to hold the query. In some cases this grid will be hidden, but it works better if there is one.

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

The commented code needs to be adjusted to suit the particular BAQ and how it needs to be displayed. The `ddXXXX` should be the particular variable declared, and 'BAQNAME' is the name of the query.

The first parameter is a list of the names of the query parameters. Note that optional parameters can be treated as optional by DynData if the name is prefixed in this list with a hyphen, eg `-param1` (the parameter name in the query doesn't need the hyphen). The second parameter is a list of defaults for those parameters in string form. If the query has no parameters, both these arrays can be replaced with null, one for each method parameter.
The third parameter needs a list of query fields that together make a unique key for the row, and the query must be constructed to include such a set of fields. (Often this is easiest with a SysRowId field).
The fourth parameter is a list of query CAPTIONS (not field names) to be included in the screen grid used for the BAQ. Any captions not listed will be in the grid collection, but hidden from the user. The fifth parameter is a list of integer widths for the visible columns. To work predictably, these should add up to forty less than the grid width (twenty at the left of the grid for indicators and twenty at the right for a vertical scroll bar). If they total less, the columns will automatically expand, but if they total more than the user will need to scroll left and right to see them all.
The sixth parameter is a list of query captions that require particular display formats, and the seventh parameter is a list of those formats in the same order as they are listed in the sixth parameter. If no formats need to be specified, both parameters can be null instead of string arrays.
The eighth parameter is the name of the grid used in the screen for this BAQ.

Note that three of the array parameters are paired and MUST have matching numbers of elements: the first and second (parameters), fourth and fifth (captions and columns) and six and seventh (captions and formats). For technical convention reasons within the workings, DynData relies on the column captions rather than column names for many functions.

Within the existing `DestroyCustomCode()` method, call `DestroyCode()`, which is also declared as a method within Script:

```c#
    private void DestroyCode() 
    { 
        foreach (DynData dd in baqs.Values) 
        { 
            dd.CloseDown(); 
        } 
    } 
```

This is the basic requirement. When you have done this much, you can close the customization and the screen, and when you reopen the same customization the BAQ will be loaded, the data will show in the grid as loaded with the default parameters, and when you open customization mode the query will also show in the EpiDataView collection and be available for binding to controls.
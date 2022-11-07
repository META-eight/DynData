## DynData within a screen

When a DynData object is used in a customization, its data will appear in the list of EpiDataViews.

That EpiDataView can be used in all the usual ways, such as binding to controls.

The screen will not include the DynData object in its standard workings, though, so if you need user screen actions to affect it, those will need to be manually put in place.

### Update, Clear, Save

The DynData object has methods for each of these:

```c#
    ddXXXX.GetData(); // to force new data to be fetched
    ddXXXX.RefreshData(); // to get new data only if the query parameters have changed

    ddXXXX.Clear(); // to clear the query and adapter entirely

    ddXXXX.Save(); // to save changes to an updateable query back to the database
```

For these to happen when the user clicks the standard screen controls and menu items, use the `ToolClick` event. You can do this with the Form Event Wizard, or manually as follows:

In `InitializCustomCode()` after the Wizard Added Custom Method Calls:

```c#
    this.baseToolbarsManager.ToolClick += new ToolClickEventHandler(this.baseToolbarsManager_ToolClick);
```

In `DestroyCustomCode()` within Custom Code Disposal:

```c#
    this.baseToolbarsManager.ToolClick -= new ToolClickEventHandler(this.baseToolbarsManager_ToolClick);
```

The method these event handlers calls looks like this:

```c#
    private void baseToolbarsManager_ToolClick(object sender, ToolClickEventArgs args) 
    { 
        switch (args.Tool.Key) 
        { 
            case "SaveTool": 
                // loop to update all DynData objects in turn, or replace the loop with more granular code as needed
                foreach (DynData dd in baqs.Values) 
                { 
                    dd.Save(); 
                } 
                break; 
            case "RefreshTool": 
                // you can include a loop to refresh the BAQs here, too, or call data only for what is needed
                break;
            case "ClearTool":
                // clear needed DynData objects, by loop or otherwise
                break; 
        } 
    } 
```

### Changing query parameters

In many cases, when the data is refreshed, it will be because different data is required, and that is usually handled by changing the query parameter(s).

This can be done manually in the code like this:

```c#
    ddXXXX.UpdateParam("paramName","new param value");
```

(It will need doing for each parameter that needs to change.) You can do this in, for example, the event handler for an EpiDataView notification (using the `Initialize` option) or a row change.

If there is a control holding the data for the query parameter – either already on the screen, or added as a custom control for the user specifically for the query – then this can be linked directly to the query.

In the `Tag` property of the control add text in the following pattern:

`p:BAQNAME.parameterName`

You can add as many as needed for different DynData objects, separated by a space. If it's an optional parameter, add a hyphen before the parameter name, as for the Initialisation.

When the screen loads, the DynData object will automatically link the control. When calling `GetData()` the parameters will be updated from the controls, or the same update can be forced by calling `ddXXXX.ParamsFromControls()`.

There are two optional suffixes which can be used with controls that have text values:

`p:BAQNAME.parametername~LIKE`
`p:BAQNAME.parametername~LIKEALL`

When using the first, the value is passed to the query surrounded by wildcard characters, so if the BAQ uses the parameter in a 'LIKE' condition, it will be treated as as a 'contains' pattern match rather than an exact match.

The second behaves the same but replaces all spaces with wildcards too.

You can, if needed, also work directly with the stored query parameters, which can be accessed via `ddXXXX.CurrentParams()` – this will return a Dictionary of parameter names and current values.

### Bound controls

When you set the EpiBinding of a screen control to a field of a DynData EpiDataView, it will behave in the usual way, showing the relevant data for that field on the current active row of the view (and the grid, if that is visible). As the row changes, so will the data.

Note that if you bind a drop-down control (EpiCombo or BAQCombo) to a DynData field, the same field in the grid, if it's visible, will be changed to the matching type of control, so the grid will also have a drop-down for that field. If you want a drop-down in the grid *only* it can be useful to bind a control in this way and then set its visibility to false.
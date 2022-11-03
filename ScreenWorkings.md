### DynData within a screen

When a DynData object is used in a customization, its data will appear in the list of EpiDataViews.

That EpiDataView can be used in all the usual ways, such as binding to controls.

The screen will not include the DynData object in its standard workings, though, so if you need user screen actions to affect it, those will need to be manually put in place.

## Update, Clear, Save

The DynData object has methods for each of these:

```c#
    ddXXXX.GetData(); // to force new data to be fetched
    ddXXXX.RefreshData(); // to get new data only if the query parameters have changed

    ddXXXX.Clear(); // to clear the query and adapter entirely

    ddXXXX.Save(); // to save changes to an updateable query back to the database
```

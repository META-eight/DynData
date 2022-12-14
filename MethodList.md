### List of DynData Methods

`.Name` *returns string*  
Gets or sets the BAQ query name. Normally set during initialisation.

`.CloseDown()`  
Disposes of all elements of the object and sets variables to null.

`.AutoSelect` *returns bool*  
Gets or sets whether when data is fetched first time, a row will be made active without the user selecting one.

`.RefreshLimit` *returns int*  
Gets or sets a time value in milliseconds within which a new `GetData()` call will be ignored unless the query parameters have changed. By default this is set to 3000 on initialisation.

`.IsDirty` *returns bool*  
Gets whether the query data contains changes that have not been saved back to the database.

`.AllowMultiRow` *returns bool*  
Gets or sets whether the user can select a single row in the DynData grid or multiple rows. Default is false (user can only select a single row) as the behaviour is more what the user expects.

`.DefaultToLastRow` *returns bool*  
Gets or sets whether the LAST row in the set of data is made active when returned. By default it's the first row.

`.GetDataAfterSave` *returns bool*  
Gets or sets whether `GetData()` is automatically called when a save is completed. True by default.

`.GetEdv()` *returns EpiDataView*  
Gets the EpiDataView associated with the DynData object.

`.GetTable()` *returns DataTable*  
Gets the DataTable of query results.

`.GetGrid()` *returns EpiUltraGrid*  
Gets the grid control associated with the DynData object.

`.Clear()`  
Clears the data from the DynData object.

`.CurrentDataRow()` *returns DataRowView*  
Gets the active row of the EpiDataView associated with the DynData object, or null if there is no row active.

`.RowCount()` *returns int*  
Gets the number of rows returned by the query.

`.KeyNames()` *returns string array*  
Gets the column names used to make up a unique key for a query row.

`.CurrentKeys()` *returns string array*  
Gets the values of the key columns of the current query row.

`.UpdateKeys(string[] newkeys)`  
Sets the key row values to new values by passing in an array of values to use. Used to locate a required row that has those key values.

`.CurrentParams()` *returns string/string Dictionary*  
Gets the query parameter names and current values as key-value pairs.

`.ParamNames()` *returns string array*  
Gets the query parameter names.

`.UpdateParam(string key, string newval)`  
Updates the given query parameter ('key' is the parameter name) with a new value ('newval'), which will be used next time data is fetched from the query.

`.ResetParams()`  
Sets all query parameters back to the defaults set during initialisation.

`.ParamsFromControls()`  
Manually sets all query parameters to the values in any controls with the 'p:' text in the Tag property.

`.ManualFilters()` *returns string/string Dictionary*  
Gets any current filters applied to the grid, excluding ones set from tagged Controls, in pairs of column name and filter value.

`.ReplaceManualFilters(Dictionary<string,string> NewFilters)`  
Overrides any existing filters applied to the grid, excluding ones set from tagged Controls, with the new set of column name / filter value pairs.

`.ClearManualFilters()`  
Removes all filters from the grid apart from those set via tagged Controls.


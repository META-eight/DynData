





```c#
			ddcsCustomerSieve = new DynDataColumnSelect(ddCustomerSieve,oTrans);
			ddcsCustomerSieve.Initialise(
				new string[] {"Rank","CustID","Customer","Postcode","County","Account Manager","Sales Area","Terms","Credit Hold","Credit Limit","Grade","Potential","Sales Target","Target To Date","Rolling Sales","Rolling Thrpt","Year Sales","Year Thrpt","Sales Per Day","Quoted Per Day","Win Ratio","Touchpoints Per Day","GP","Invoice Balance","Credit Balance","Last Visit","Last MSI","Distance","Sheet Clad Sales","Flat Roof Sales","Drainage Sales","Rainscreen Sales","Safe Access Sales"},
				new int[] {    40,    60,      200,       70,        150,     120,              90,          50,     70,           70,            50,     70,         80,            80,              80,             80,             80,          80,          90,             100,             60,         130,                  40,  90,               90,              90,          90,        70,        110,               110,              110,             110,               110},
				new bool[] {   true,  true,    true,      true,      false,   false,            false,       true,   false,        false,         false,  false,      true,          false,           false,          false,          true,        false,       true,           true,            true,       false,                true,false,            true,            true,        true,      false,     false,             false,            false,           false,             false},
				grdColsVisible);
```
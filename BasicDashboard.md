## Creating a Basic DynData Dashboard

The DynData approach can be used in any customization, but one use is instead of a conventional Epicor dashboard. This takes some different steps.

### 1. Create a Placeholder BAQ

You will need a BAQ simply to be the base of the dashboard. No data from this will be used, so it should be as small and simple as possible.  
It can be from an accessible table that has one row, for example, and only needs to include a single field. It does need to be shared.

### 2. Create the Dashboard Base

Create a new dashboard, and bring in the placeholder BAQ as the query.  
Make a Tracker view of the query *with no fields from the query visible*. The Tracker should be completely blank. Then delete the original Grid view from the query so only the Tracker view remains.  
Deploy the dashboard to an assembly version, and create a menu item so the dashboard assembly can be opened directly from the client. This will be a completely blank screen for now.

### 3. Create the actual Dashboard BAQ(s)

You need at least one BAQ for the actual data.   
The only essential extra feature for DynData BAQs is that there needs to be a set of fields that acts as a unique key to each row. When the BAQ is based on a single main table it is often convenient to use the SysRowID of that table.  
It can be updateable, but doesn't need to be. If it is updateable, then the fields selected as updateable will be editable by the user in the final dashboard.  
As there is no restriction on the number of rows returned in a DynData dashboard, it is sensible to use Query Parameters to limit the data in the query to only the data needed.  
Colour highlighting is optional – see [Colours](Colours.md) if any is needed.

### 4. Create a Customization for the Dashboard Assembly

Open the dashboard in developer mode, and save a customization.  
Paste the code from [DynData](DynData.cs) between the `Using` statements at the top of the Script Editor panel, and the Script itself (which starts with `public class Script`).  
Add custom reference assemblies `Ice.Contracts.BO.DynamicQuery.dll` and `Ice.Core.Session.dll`.  
Add one EpiUltraGrid control per BAQ you want to use to the screen area. If you will want to use extra fields to show row details, parameters or filter the grid, then leave space for them.  
See [the set-up instructions](SetUp.md) for the code to use within the Script area.  
Save and close the customization, then set it as the default for the menu item.  
You can then re-open the customization (in developer mode) and add any other controls and workings you need. You will generally need to consider at least the things in [hooking into Epicor screen workings](ScreenWorkings.md), but the other instructions are more optional.  
Every time you open the screen using this customization after first setting it up, it will load the data from the BAQs you've included and they will be available as EpiDataViews and for further code work.  
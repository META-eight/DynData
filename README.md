# DynData
Epicor client DynamicQuery wrapper classes and utilities

### About

Epicor handles BAQs with the DynamicQuery business object.  
The DynData class enables BAQs to be used within a classic client screen more conveniently, by allowing objects to be created and persisted that include them.  
The approach is based on conventions rather than overriding any Epicor code.  

### Instructions

The base code required for the DynData classes is in [DynData](DynData.cs).  
It is not a custom DLL to be included in the client folder for maintainability reasons. It works by pasting the code into the customization of the screen where it's needed.

The basics of how to do that are in [Set-up Instructions](SetUp.md), followed by [Hooking into the Epicor Screen Workings](ScreenWorkings.md).

For a more basic introduction, there is [Creating a Basic Dashboard](BasicDashboard.md).

One of the main purposes of the DynData system is to make it easy to create user-friendly custom features for making the data clearer.  
It is possible to give the user very quick and responsive filters, for example, mostly with no code at all. Doing that is covered by [Filters](Filters.md).  
It is also possible to create very sophisticated colour features within the data grid, purely by adding some specific fields to the underlying BAQ and applying some conventions. A guide to the basics of this is in [Colours](Colours.md).

For additional custom work triggered by user interaction with the screen, custom code in the customization Script can use [DynData Events](Events.md).

### Note

This code was created for our own use and is not endorsed by Epicor.  
It is a series of wrappers and conventions on top of standard Epicor systems that have remained largely unchanged from ERP10.0 to the Kinetic classic client, but we have no control over how those systems may be changed or made obsolete in future.
It is documented here for ease of maintenance by METAeight clients where we have used this system on their behalf, and we take no responsibility for any other use.  
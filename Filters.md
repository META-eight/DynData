## Filtering DynData Grids

One of the most common features users want when accessing data is ways to filter it. The DynData classes make this simple and mostly non-technical.  

### Filter Controls

To create a user filter, add a control to the screen with the DynData grid. DO NOT bind it to a field.  

In the 'Tag" property, insert text in the following form:  

`f:BAQNAME.Field_Name~OPTIONALCOMPARISONTYPE~OPTIONALANDOR`  

Note that no code is required when doing this. During form load, an event handler will be created on `ValueChanged` for the control, and that will be used to filter the grid. Using this event means that filter changes are instantaneous as the user enters anything.  

The components of the Tag text are as follows:  

`f:` is always the same, and is the indicator on form load that this control is to be used as a filter. There can be multiple entries in the Tag property for a single control. Each one must have the same pattern starting with 'f:', and they must be separated by a space.  

`BAQNAME` is the name of the EpiDataView, which is also the name of the BAQ the specific DynData object is using.  

`Field_Name` is a valid field in the BAQ, NOT the caption but the actual name of the field. It doesn't need to be one that's visible in the grid – it can often be useful to filter on a hidden column.  

`OPTIONALCOMPARISONTYPE` sets the way the filter behaves. The default is 'EQUALS', and that is what is used if nothing is entered in this part (in which case the '~' should also be left off).  

Comparison types are:  

  * EQUALS – rows will only appear in the grid if the value in the column linked is exactly what the user entered. Note that this can sometimes confuse users, because it will start filtering as soon as they type if the control is a text box, and probably nothing will match the first few letters so the required rows won't appear until they've finished. For text, CONTAINS or STARTSWITH are usually better options.  

  * CONTAINS – only useful for text. Field values with what the user enters anywhere within them will allow the row they're on to be shown.  

  * GREATERTHAN – Field values more than entered by the user will be shown, not including the value entered itself.  

  * LESSTHAN – Field values less than entered by the user will be shown, not including the value entered itself.  

  * GREATERTHANOREQUALTO – As GREATHERTHAN, but including the value entered.  

  * LESSTHANOREQUALTO – As LESSTHAN, but including the value entered.  

  * STARTSWITH – only useful for text. Behaves like CONTAINS, but the text entered must appear at the start of the value in the field.  

  * LIKE – effectively behaves like CONTAINS, included for compatibility with Infragistics control behaviour, only for text. Values entered by the user will have wildcard characters added to the beginning and end before being pattern-matched with the field in the grid. For most purposes, CONTAINS is more predictable. Note that there is a further special behaviour for LIKE, though: if a further `~LIKEALL` is added to the text (in this case OPTIONALANDOR is no longer optional) then each space in the text entered will also be replaced by wildcards, so the text snippets will be matched in the order given even if other text appears between them.  

  * NOTEQUALS – opposite to EQUALS, will only exclude the exact value entered by the user.  

  * ENDSWITH – as STARTSWITH, but for text appearing at the end of values in the grid field.  

  * DOESNOTCONTAIN – opposite to CONTAINS, will only exclude values that have the text entered by the user within them.  

  * MATCH – included for compatibility with Infragistics control behaviour.  

  * DOESNOTMATCH – see MATCH.  

  * NOTLIKE – see LIKE.  

  * DOESNOTSTARTWITH – as STARTSWITH, but excludes grid values that have the user's entered text at the beginning.  

  * DOESNOTENDWITH – as ENDSWITH, but excludes grid values that have the user's entered text at the end.

For more information about the behaviour of these comparison types, see Infragitics documentation for `FilterComparisonOperator`.  

`OPTIONALANDOR` governs how multiple filters work together. The default is AND, in which case there is no need to specify, and this behaviour is the most predictable. All relevant filter controls will filter the grid, and the only rows shown will be those that match all criteria.

If OR is specified for all controls, then rows matching any one of the control criteria will be shown. This is usually not what the user will naturally expect, so it is important to label the filters carefully in this case.

If controls affecting the same grid are mixed between AND and OR then behaviour is not clearly defined.
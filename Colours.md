## Colours and Highlights in DynData Grids

DynData objects can colour the fields in a grid in more subtle ways than the built-in Epicor row rules, but the method is different. To achieve this flexibility, the DynData code has to bypass the EpiMagic that handles the formatting of the grid.  

### Base workings  

To colour any individual column in the BAQ grid, add an additional Calculated field to the BAQ, of type nvarchar, minimum length 7 characters.  

The output to this field must be a Hex colour code (also known as web colour or HTML colour) in the format '#FFFFF'. It can be hardcoded to make that column always that colour in the simplest form.  

The CAPTION (not the name) of the field takes the form of the name of the field to be coloured, as it appears when the BAQ is in use, followed by a space and 'Colour'. For example, 'OrderHed_OrderNum Colour' or 'Calculated_Status Colour'.  

When the DynData `FormatRows()` method is called, the hex colour will be used to provide the background colour of the column specified. This method is called automatically every time the data is refreshed, and can be called manually to alter colours when otherwise required.  

**To colour a whole row,** use the word 'All' in place of a column name in the caption of a dedicated calculated field, ie 'All Colour'.  

The DynData code also includes logic to evaulate the perceived brightness of the resulting colour to the human eye, and switches the field text between black and white automatically according to which is assessed to be the better contrast.

### Precedence  

Note that colour columns are processed in the order they appear.  

If you need a general row colour, this should be first in the list of fields. Any specific fields following it will be applied later and therefore override the more general one.  

At the moment there is no way of colouring multiple columns short of a whole row, so each column will need a separate field.  

### Variable colour  

Because the colour output is a field, each row is processed separately and can be a different value.  

The simplest way of doing this is a SQL 'CASE' statement in the field:

```sql
CASE
    WHEN Table_Field1 > Table_Field2 THEN '#FF0000'
    ELSE '#FFFFFF'
END
```

This can, of course, be arbitrarily complex, or broken into subformulas over different calculated fields if needed.  

As the output is a Hex colour, it is also possible to create graduated variations based on numerical values. If a number from some BAQ field can be coerced into a 0-255 integer range, that range can be used directly to generate the hexadecimal values, and the colour of the field will consequently be directly changed depending on the value shown.  

For the simplest example, assume a decimal BAQ field called 'Calculated_Value' that we already know can only have values between 0 and 255. Maybe it's already based on another set of fields.  

We want a pure white field when the value is at minimum, and pure red when at maximum. Because hex colours are RGB, this means that the first hex value remains 'FF' at all times, and the others vary in sync between 'FF' for white and '00' for red. So each is `255 - Value`, as a two-digit hexadecimal, which is `RIGHT(CONVERT(VARCHAR(8),CONVERT(VARBINARY(4),255-Value,2)),2)`. (This assumes Calculated_Value is in the same subquery so Epicor refers to it by its simple name.) . 

So the full SQL formula for this field would be

```sql
'#' + 'FF' + RIGHT(CONVERT(VARCHAR(8),CONVERT(VARBINARY(4),255-Value,2)),2) + RIGHT(CONVERT(VARCHAR(8),CONVERT(VARBINARY(4),255-Value,2)),2)
```

More complex versions can graduate between two different shades by dividing the difference between each RGB value by 256, or even between multiple shades – for example, white for a neutral value, increasing green for positive and increasing red for negative.
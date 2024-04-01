# Language resource 

     Culture.restext

.restext is a language resource. `Culture` is the language code.   
To add a new language resource, simply create and deploy this file.

# .restext format

The character encoding is UTF-8.  

 * Key=Text
 * Key:Pattern=Text

## Key
`Key` is a resource key.  
e.g., "BookAddressInfo.Bookmark=This is bookmark address."

If not defined, fall back to en.restext.

## Text
`Text` is the corresponding text.

If empty, fall back to en.restext.

 `{0}` is replaced by the argument programmatically.  
e.g., "BookAddressInfo.Page={0} pages"

`@Key` specifies a key to be replaced by another resource.
e.g., "BookConfig.ContentsSpace=Distance between pages in "@PageMode.WidePage" (pixels)"

## Pattern
`Pattern` is a regular expression to select a variation of the expression depending on the argument. Define it only if necessary.
For example, it is used when the expression changes in the plural.  
e.g., "BookAddressInfo.Page:1={0} page"


# ConvertRestext.ps1

Converts between language files (*.restext) and JSON. This is a utility tool, not a required feature.

See Get-Help for details.

> Get-Help .\ConvertRestext.ps1
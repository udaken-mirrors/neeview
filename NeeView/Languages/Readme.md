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

## Shortcut key name

Shortcut keys can be multilingual. 
Define only as much as you need.

### Key

General Key.

Prefix is `Key.` .  
See [.NET Key Enum](https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.key) for available names.

e.g., "Key.Enter=EINGABE"

### Modifier keys

Modifier key.

Prefix is `ModifierKeys.` .  
See [.NET ModifierKeys Enum](https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.modifierkeys) for available names.

e.g., "ModifierKeys.Control=STRG"

### Mouse action

Mouse action.

Prefix is `MouseAction.` .  
The available names are as follows

- LeftClick
- RightClick
- MiddleClick
- LeftDoubleClick
- RightDoubleClick
- MiddleDoubleClick
- XButton1Click
- XButton1DoubleClick
- XButton2Click
- XButton2DoubleClick
- WheelUp
- WheelDown
- WheelLeft
- WheelRight

e.g., "MouseAction.LeftClick=Linksklick"

### Modifier mouse buttons

Modified mouse button. Used when multiple mouse buttons are pressed at the same time.

Prefix is `ModifierMouseButtons.` .  
Available names are as follows

- LeftButton
- MiddleButton
- RightButton
- XButton1
- XButton2

e.g., "ModifierMouseButtons.LeftButton=LinkeTaste"

# ConvertRestext.ps1

Converts between language files (*.restext) and JSON. This is a utility tool, not a required feature.

See Get-Help for details.

> Get-Help .\ConvertRestext.ps1
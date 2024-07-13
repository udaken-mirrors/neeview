## ChangeLog

----

### 41.3
(2024-07-14)

#### Changed

- .NET 8.0.7
- 7z.dll ver 24.07

#### Fixed

- Fixed a bug that sometimes caused an error when switching page filters.
- Fixed a bug that caused a huge popup with a dummy icon in the page list.
- Fixed a bug that sometimes caused an error when switching books during animation frame generation.

----

### 41.2
(2024-05-31)

#### Changed

- Update 7z.dll to ver 24.06

#### Fixed

- Fixed a bug that the focus was not set correctly when switching lists in the thumbnail layout. 
- Fixed a problem updating the search filter in the PageList.

----

### 41.1
(2024-05-18)

#### Changed

- Update various libraries

#### Fixed

- Fixed a bug that prevented seamless loops from working.
- Fixed an issue where automatic background color setting is not reflected when a book is opened.

----

### 41.0
(2024-05-10)

#### Added

- Added Italian language.
- Direct editing of language resources. (/Languages/*.restext) 
- Auto scrolling is implemented. Long press also supported. By default, the wheel button toggles between modes.
- Added a new command parameter "In panorama mode, all pages are considered as one page" to the N-type scrolling command.
- Added book move priority setting. (Settings > Move > Book movement priority)
- Add ability to switch display when page is ready. Suppresses temporary display on page switching. (Settings > Move > Ready to page move)
- Added the ability to drop the bookshelf location icon and the information icon in the address bar to other apps

#### Changed

- ZIP version places DLL files in the Libraries folder.
- The current view is maintained as much as possible when switching books.
- Asynchronous pre-decompression of solid compressed archives
- Various library updates

#### Fixed

- Fixed a bug where the mouse button would sometimes enter long-press mode even when it was released.
- Fixed a bug in file manipulation of network folder search results in the bookshelf.
- Fixed a bug where an incorrect page was sometimes created when a playlist was opened as a book.
- Fixed a bug that sometimes caused incorrect behavior in bookshelf range selection. 
- Fixed a bug that prevented the Susie Plug-in all ON/OFF settings from working properly.
- Fixed incorrect panel display status flag in menus. 
- Reduced the problem of book thumbnails not being generated when files are added.
- Fixed a bug that window dragging did not work when a book was closed.
- Fixed a bug that shortcut archives were not recognized as pages.
- Fixed an issue where the app would sometimes crash when creating a bookmark folder in the folder tree.

----

Please see [here](https://bitbucket.org/neelabo/neeview/wiki/ChangeLog) for the previous change log.

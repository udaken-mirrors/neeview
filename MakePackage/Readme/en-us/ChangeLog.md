## ChangeLog

----

### 40.5
(2024-01-12)

#### New

- Added "Stretch Tracking" toggle button to the Scale section of the Navigator panel. This function corrects the scale according to the stretch mode for images that are rotated, etc.
- Added "Stretch" command to apply stretching to scale.

#### Changed

- Changed the display start position setting to selective. Added "Direction dependent, top". (Settings > View operation > Display start position)
- Script: nv.Config.View.IsViewStartPositionCenter is obsolete. Use nv.Config.View.ViewOrigin instead.
- Set the default for the "Toggle page mode" command to loop. The default for the mouse gesture to set the page mode is now a dedicated command and behaves the same as before.
- Switching is now performed when the same stretch mode is specified in the stretch mode specification command, so that the behavior is the same as before.
- Changed scale calculation method for "Auto stretch window" to be more natural.

#### Fixed

- Fixed a bug that the stretch mode was not applied to the stretch apply button in the navigator.
- Fixed a bug that sometimes prevented alphabetic word searches.
- Reduced frame dropping in VLC video.
- Fixed a bug that sometimes caused application errors with VLC videos.
- Fixed a bug that rotation information was not reflected correctly in VLC videos.
- Fixed a bug that disabled track designation when switching repeat settings in VLC video.
- Fixed a bug in VLC video where media with audio information only was sometimes not determined to be audio.

----

### 40.4
(2023-12-22)

#### New

- Added encoding setting for ZIP files when the UTF-8 flag is not set.

#### Fixed

- ZIP files now load in UTF-8 when the UTF-8 flag is set.
- Fixed a bug in which the file exclusion attribute differs between bookshelf and bookshelf search.
- Fixed a bug where changes were not applied even if the book was reopened after changing archive settings.
- Fixed a bug where QuickAccess property changes were sometimes not saved.
- Fixed a bug where the keyboard focus was not following the change of selected items on the bookshelf.
- Reduces flash when switching books

----

### 40.3
(2023-12-16)

#### New

- Add "Added dummy page to the first/last page" settings.

#### Fixed

- Fixed a bug that caused an error in copying.
- Fixed a bug that "Start loupe at standard magnification" of loupe did not work.
- Fixed an issue with page slider movement in two-page display that caused misalignment when moving from a single page.
- Corrected the number of pages displayed by the "Last Page" command in the two-page display.
- Fixed a bug in seamless loop in two-page display where "First/Last page alone" did not work.
- Adjust button widths on the settings page.
- Fixed an issue with the number of pages displayed when returning to the previous book in an end-of-page book move.
- Fixed a bug that sometimes moved two pages when moving one page.

----

### 40.2
(2023-12-12)

#### New

- Added Portuguese (pt-BR)

#### Fixed

- Fixed a bug that sometimes only one page is displayed even though the display mode is 2-page display mode.
- Fixed a bug that film centering did not work immediately after the start of filmstrip display.
- Fixed a bug that history may not be saved.
- Fixed a bug that the loupe did not work immediately after switching to panorama mode.
- Fixed a bug that loupe release on page move did not work.
- Fixed a bug that prevented searching by network path in the bookshelf.
- Fixed a bug in which the sub archive loading failure skip process did not work.
- Susie plugin Improved access timeout handling.

----

### 40.1
(2023-12-06)

#### Fixed

- Fixed a bug that data could not be read if the drag operation "Move (scale dependent)" parameter was specified in the previous version.
- Fixed a bug that sometimes only one page is displayed even though the display mode is 2-page display mode.
- Fixed a bug in which exclusion patterns were not applied in bookshelf searches.
- Corrected a typo in the help.

----

### 40.0
(2023-12-05)

#### Important

- Windows 10, Windows 11, and 64-bit operating systems only; packages for Windows 7, Windows 8.1, and 32-bit operating systems are not provided after version 40.0.
- It runs on .NET8. This framework is included as part of the application. There is no need to install it separately.
- For environments that already have .NET8 installed, we have prepared a package "NeeView40-fd.zip" that does not include. .NET8 runtime for x86 must also be installed when using the Susie plugin.
- The ZIP version cannot be overwritten and updated because the file structure has changed significantly. Please use Export/Import to migrate your data. The installer version can be updated as is.

#### User data storage location

- The user data storage location for the ZIP version has been consolidated into the "Profile" folder.
- The default storage location for user data can be specified with the NEEVIEW_PROFILE environment variable.

#### Revamped page display code

The page display program has been reworked to accommodate the implementation of panorama mode. We have tried to match the previous behavior as much as possible, but there is a possibility that the behavior when changing the scale or rotation may have changed.

- Added "Panorama" mode, which connects pages together.
- The page connection direction can be switched between portrait and landscape.
- Scroll time settings for various scroll commands are combined into one. (Settings > View operation > Scroll time)
- Enabled to switch pages with scrolling animation. (Settings > View operation > Page move time)

#### MainView window

The MainView window now functions as a new viewing mode, not just a windowed display.

- Assigned the F12 shortcut to the MainView window switching command.
- Added setting to display page list in main area when MainView window is displayed. (Settings > Main view)
- Added auto-hide setting for MainView window. (Settings > Main view)
- Changed behavior so that the MainView window is minimized with the close button.
- Minimize when ESC key is pressed in the MainView window.
- Added "Auto stretch window" to the main view window. (MainView window > Title bar context menu)
- Added a setting to the page list to move the focus to the main view when a page is selected. (PageList panel > Detail Menu)

#### Enhanced search 

- Search box added to various panels.
- Added search option. You can now search by tags contained in bookmarks and images. See "Search options help" for details.
- Individual deletion button of search history added.
- Added search history size setting. (Settings > General)

#### Enhanced video play

- Added setting to use libVLC (VLC media player) for video play. (Settings > Video)
- Added setting to display videos as pages. (Settings > Video)
- Added a control bar to the Navigator panel for control of videos displayed as pages. (Navigator panel > Detail Menu)
- Use AnimatedImage for GIF animation.
- Supports PNG animation.

#### Base scale

- Added base scale change command and mouse drag operation.
- Base scale values are now stored in book units.

#### Auto rotate

- Added forced left rotate and forced right rotate to Auto rotate.
- Auto rotate settings are now saved per book.

#### Other

- Many bug fixes.
- App icon change.
- Changed window maximization correction process. The window frame width setting when maximized has been abolished.
- Add "$FullPath" to the window title keyword.
- Added a setting to swap left and right commands by slider direction using the tilt wheel operation.
- The SusiePlugin folder can now be specified by relative path.
- Faster operation in panel thumbnail view.
- Added setting to limit panel width to within the window. (Settings > Panels)
- Panels can now be connected horizontally; the second and subsequent panels can only be connected in the current orientation.
- The date/time format of the panel is culture-dependent. This format is configurable. (Settings > General)
- panels, etc., to accept mouse commands when possible.
- The natural order was made closer to the Explorer's order.
- Additional text embedded in PNGs can now be displayed in the "Extras" group of the Information panel.
- History is now saved automatically.
- History update date and time are now displayed in the contents view of the history list.
- Implemented file renaming in the PageList.
- Supports deleting files in ZIP. To make it work, enable ZIP file editability in the settings.
- Added original size and keep dot settings to image output.
- Added "Seamless loop" for end-of-page behavior.
- Improved accuracy of slideshow timer.
- Added timer display to slideshow. (Settings > Slideshow)
- The target of the "Grid" can now be selected between the image and the screen.
- Redesigned book page. Images are now displayed as they are instead of thumbnails.
- The text element when copying a file is now basically absent.
- Playlist items can also be used to copy files.
- Script: Levels were introduced for compatibility. Changes that do not affect the operation, such as parameter name changes, are now only notified in the console.
- Script: nv.Playlist.Name Add. The name of the current playlist can be changed by assigning.
- Scrpit: Added GetMetaValue method to PageAccessor. You can get the meta information of an image.

----

Please see [here](https://bitbucket.org/neelabo/neeview/wiki/ChangeLog) for the previous change log.

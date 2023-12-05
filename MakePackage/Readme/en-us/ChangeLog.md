## ChangeLog

----

### 40.0
(2023-12-05)

#### Important

- Windows 10, Windows 11, and 64-bit operating systems only; packages for Windows 7, WIndows 8.1, and 32-bit operating systems are not provided after version 40.0.
- It runs on .NET8. This framework is included as part of the application. There is no need to install it separately.
- For environments that already have .NET8 installed, we have prepared a package "NeeView40-fd.zip" that does not include .
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

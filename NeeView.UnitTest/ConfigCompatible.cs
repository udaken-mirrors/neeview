using System.Text.Json;
using Xunit.Abstractions;

namespace NeeView.UnitTest
{
    public class ConfigCompatible
    {
        private readonly ITestOutputHelper _output;
        
        public ConfigCompatible(ITestOutputHelper testOutputHelper)
        {
            _output = testOutputHelper;
        }


        [Fact]
        public void AutoHideConfigCompatible()
        {
            var options = UserSettingTools.GetSerializerOptions();

            string json = """
            {
                  "AutoHideConfrictTopMargin": "AllowPixel",
                  "AutoHideConfrictBottomMargin": "Deny"
            }
            """;

            // ver.41
            var config = JsonSerializer.Deserialize<AutoHideConfig>(json, options);
            Assert.NotNull(config);
            Assert.Equal(AutoHideConflictMode.AllowPixel, config.AutoHideConflictTopMargin);
            Assert.Equal(AutoHideConflictMode.AllowPixel, config.AutoHideConfrictTopMargin);
            Assert.Equal(AutoHideConflictMode.Allow, config.AutoHideConfrictTopMargin_Typo);
            Assert.Equal(AutoHideConflictMode.Deny, config.AutoHideConflictBottomMargin);
            Assert.Equal(AutoHideConflictMode.Deny, config.AutoHideConfrictBottomMargin);
            Assert.Equal(AutoHideConflictMode.Allow, config.AutoHideConfrictBottomMargin_Typo);
        }

        [Fact]
        public void SystemConfigCompatible()
        {
            var options = UserSettingTools.GetSerializerOptions();

            string json = """
            {
                  "IsHiddenFileVisibled": true,
                  "IsOpenbookAtCurrentPlace": true,
                  "DestinationFodlerCollection":[
                      {
                        "Name": "Pictures",
                        "Path": "C:\\Pictures"
                      }
                   ],
            }
            """;

            // ver.41
            var config = JsonSerializer.Deserialize<SystemConfig>(json, options);
            Assert.NotNull(config);
            Assert.Equal(true, config.IsHiddenFileVisible);
            Assert.Equal(true, config.IsHiddenFileVisibled);
            Assert.Equal(false, config.IsHiddenFileVisibled_Typo);
            Assert.Equal(true, config.IsOpenBookAtCurrentPlace);
            Assert.Equal(true, config.IsOpenbookAtCurrentPlace);
            Assert.Equal(false, config.IsOpenbookAtCurrentPlace_Typo);
            Assert.Single(config.DestinationFolderCollection);
            Assert.Equal("Pictures", config.DestinationFolderCollection[0].Name);
            Assert.Equal("C:\\Pictures", config.DestinationFolderCollection[0].Path);
            Assert.Single(config.DestinationFodlerCollection);
            Assert.Equal("Pictures", config.DestinationFodlerCollection[0].Name);
            Assert.Equal("C:\\Pictures", config.DestinationFodlerCollection[0].Path);
            Assert.Equal(null, config.DestinationFodlerCollection_Typo);
        }
    }
}

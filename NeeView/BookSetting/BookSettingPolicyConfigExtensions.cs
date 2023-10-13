using System;
using System.Diagnostics;

namespace NeeView
{
    public static class BookSettingPolicyConfigExtensions
    {
        public static BookSettingConfig Mix(this BookSettingPolicyConfig self, BookSettingConfig def, BookSettingConfig current, BookSettingConfig? restore, bool isDefaultRecursive)
        {
            Debug.Assert(def != null);
            Debug.Assert(current != null);
            //Debug.Assert(string.IsNullOrEmpty(current.Page));

            var setting = new BookSettingConfig();

            var policyMap = new BookSettingPolicyConfigMap(self);

            var settingMap = new BookSettingConfigMap(setting);
            var defMap = new BookSettingConfigMap(def);
            var currentMap = new BookSettingConfigMap(current);
            var restoreMap = new BookSettingConfigMap(restore ?? def);

            foreach (BookSettingKey key in Enum.GetValues(typeof(BookSettingKey)))
            {
                switch (policyMap[key])
                {
                    case BookSettingSelectMode.Default:
                        settingMap[key] = defMap[key];
                        break;

                    case BookSettingSelectMode.Continue:
                        settingMap[key] = currentMap[key];
                        break;

                    case BookSettingSelectMode.RestoreOrDefault:
                    case BookSettingSelectMode.RestoreOrDefaultReset:
                        settingMap[key] = restore != null ? restoreMap[key] : defMap[key];
                        break;

                    case BookSettingSelectMode.RestoreOrContinue:
                        settingMap[key] = restore != null ? restoreMap[key] : currentMap[key];
                        break;
                }
            }

            if (isDefaultRecursive && restore == null)
            {
                setting.IsRecursiveFolder = true;
            }

            return setting;
        }

    }
}

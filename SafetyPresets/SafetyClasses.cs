using VRC;
using System.Collections.Generic;

namespace SafetyPresets
{
    public static class Classes
    {
        public class Presets
        {
            public List<SettingsPreset> ActualPresets{get;set;}

            public Presets(List<SettingsPreset> presestsList)
            {
                ActualPresets = presestsList;
            }
        }

        public class SettingsPreset
        {
            public int settingsPresetNum{get;set;}
            public string settingsPresetName{get;set;}
            public List<RankSetting> settingRanks{get;set;}
            
            public SettingsPreset(int presetnum,string presetname)
            {
                settingsPresetNum = presetnum;
                settingsPresetName = presetname;
            }
        }
        public class RankSetting
        {
            public UserSocialClass UserRank {get;set;}
            public Dictionary<string,bool> UserSettings{get;set;}
            public RankSetting(UserSocialClass uRank, Dictionary<string,bool> uSettings)
            {
                UserRank = uRank;
                UserSettings = uSettings;
            }
        }
    }
}
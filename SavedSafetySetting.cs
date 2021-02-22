using System.Collections.Generic;
using VRC;

namespace SafetyPresets
{
    class SavedSafetySetting
    {
        public int SaveIndex = 0;
        public string SettingName;
        public Dictionary<UserSocialClass, SettingContainer> Values = new Dictionary<UserSocialClass, SettingContainer>();

        public SavedSafetySetting(int SaveIndex, string SettingName)
        {
            this.SaveIndex = SaveIndex;
            this.SettingName = SettingName;
        }

        public void AddValue(UserSocialClass SocialClass, SafetySetting SafetySetting)
        {
            RemoveValue(SocialClass, SafetySetting);
            Values[SocialClass].SavedSettings.Add(SafetySetting);
        }

        public SafetySetting? GetValue(UserSocialClass SocialClass, int SafetyType)
        {
            if (!Values.ContainsKey(SocialClass))
                return null;

            return Values[SocialClass].SavedSettings.Find(st => st.SafetyType == SafetyType);
        }

        public void RemoveValue(UserSocialClass SocialClass, SafetySetting SafetySetting)
        {
            if (!Values.ContainsKey(SocialClass))
                Values[SocialClass] = new SettingContainer();

            Values[SocialClass].SavedSettings.Remove(SafetySetting);
        }
    }

    // Newtonsoft.Json is incredibly broken
    class SettingContainer
    {
        public List<SafetySetting> SavedSettings = new List<SafetySetting>();
    }

    struct SafetySetting {
        // this is an obfuscated type so cast it to int
        public int SafetyType;
        public bool Value;
    }
}

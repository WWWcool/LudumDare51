using System;

namespace Core.Windows
{
    [Serializable]
    public enum EPopupType
    {
        None = 0,
        SearchResults = 1,
        TagManager = 2,
        Settings = 3,
        ChooseLanguage = 4,
        PrivacyPolicy = 5,
        RemoveApprove = 6,
        SaveLoadTest = 7,
        Message = 8,
        TermsOfUse = 14,
    }
}
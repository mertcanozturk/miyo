using System;

namespace Miyo.Data
{
    [Serializable]
    public class ChildProfile
    {
        public string Id;
        public string ParentPlayerId;
        public string Name;
        public DateTime BirthDate;
        public int WeekdayLimitMinutes;
        public int WeekendLimitMinutes;
        public string ProfileImagePath;
        public DateTime CreatedAt;
    }
}

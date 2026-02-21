using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Miyo.Data;

namespace Miyo.Services.ChildProfile
{
    public interface IChildProfileService
    {
        UniTask CreateChildAsync(string parentPlayerId, string name, DateTime birthDate, int weekdayLimitMinutes, int weekendLimitMinutes);
        UniTask<List<Data.ChildProfile>> GetChildrenForParentAsync(string parentPlayerId);
        UniTask<Data.ChildProfile> GetChildAsync(string id);
        UniTask UpdateChildAsync(Data.ChildProfile profile);
        UniTask DeleteChildAsync(string id);
        UniTask<Data.ChildProfile> GetCurrentChildAsync();
        void SetCurrentChild(string childId);
    }
}

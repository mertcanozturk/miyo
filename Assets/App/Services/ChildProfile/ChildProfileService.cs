using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Miyo.Services.Save;

namespace Miyo.Services.ChildProfile
{
    public class ChildProfileService : IChildProfileService
    {
        private const string ProfilesKey = "child_profiles";

        private readonly ISaveService _saveService;
        private string _currentChildId;

        public ChildProfileService(ISaveService saveService)
        {
            _saveService = saveService;
        }

        public async UniTask CreateChildAsync(string parentPlayerId, string name, DateTime birthDate, int weekdayLimitMinutes, int weekendLimitMinutes)
        {
            var profiles = await LoadProfilesAsync();

            profiles.Add(new Data.ChildProfile
            {
                Id = Guid.NewGuid().ToString(),
                ParentPlayerId = parentPlayerId,
                Name = name,
                BirthDate = birthDate,
                WeekdayLimitMinutes = weekdayLimitMinutes,
                WeekendLimitMinutes = weekendLimitMinutes,
                CreatedAt = DateTime.UtcNow
            });

            await SaveProfilesAsync(profiles);
        }

        public async UniTask<List<Data.ChildProfile>> GetChildrenForParentAsync(string parentPlayerId)
        {
            var profiles = await LoadProfilesAsync();
            return profiles.Where(p => p.ParentPlayerId == parentPlayerId).ToList();
        }

        public async UniTask<Data.ChildProfile> GetChildAsync(string id)
        {
            var profiles = await LoadProfilesAsync();
            return profiles.FirstOrDefault(p => p.Id == id);
        }

        public async UniTask UpdateChildAsync(Data.ChildProfile profile)
        {
            var profiles = await LoadProfilesAsync();
            var index = profiles.FindIndex(p => p.Id == profile.Id);

            if (index < 0)
                throw new InvalidOperationException($"'{profile.Id}' ID'li çocuk profili bulunamadı.");

            profiles[index] = profile;
            await SaveProfilesAsync(profiles);
        }

        public async UniTask DeleteChildAsync(string id)
        {
            var profiles = await LoadProfilesAsync();
            profiles.RemoveAll(p => p.Id == id);
            await SaveProfilesAsync(profiles);
        }

        private async UniTask<List<Data.ChildProfile>> LoadProfilesAsync()
        {
            return await _saveService.LoadAsync(ProfilesKey, new List<Data.ChildProfile>());
        }

        private async UniTask SaveProfilesAsync(List<Data.ChildProfile> profiles)
        {
            await _saveService.SaveAsync(ProfilesKey, profiles);
        }

        public async UniTask<Data.ChildProfile> GetCurrentChildAsync()
        {
            var profiles = await LoadProfilesAsync();
            return profiles.FirstOrDefault(p => p.Id == _currentChildId);
        }

        public void SetCurrentChild(string childId)
        {
            _currentChildId = childId;
        }
    }
}

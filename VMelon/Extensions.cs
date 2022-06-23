using ProjectM;
using Unity.Collections;

namespace VMelon
{
    public static class Extensions
    {
        public static string LookupName(this PrefabGUID prefabGuid,
            NativeHashMap<PrefabGUID, FixedString128> nativeHashMap)
        {
            return (nativeHashMap.ContainsKey(prefabGuid)
                ? nativeHashMap[prefabGuid]
                : "GUID Not Found").ToString();
        }

        public static string LookupName(this PrefabGUID prefabGuid,
            PrefabCollectionSystem pfCollectionSystem)
        {
            return (pfCollectionSystem.PrefabNameLookupMap.ContainsKey(prefabGuid)
                ? pfCollectionSystem.PrefabNameLookupMap[prefabGuid]
                : "GUID Not Found").ToString();
        }

        public static string LookupName(this PrefabGUID prefabGuid)
        {
            var pfCollectionSystem = WorldUtility
                .FindWorld("Client_0")
                .GetExistingSystem<PrefabCollectionSystem>();

            return (pfCollectionSystem.PrefabNameLookupMap.ContainsKey(prefabGuid)
                ? pfCollectionSystem.PrefabNameLookupMap[prefabGuid]
                : "GUID Not Found").ToString();
        }
    }
}
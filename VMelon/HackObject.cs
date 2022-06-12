using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using MelonLoader;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Network;
using ProjectM.Scripting;
using ProjectM.UI;
using UnhollowerRuntimeLib;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using ExtensionMethods;


namespace ExtensionMethods
{
    public static class Extensions
    {
        public static string LookupName(this PrefabGUID prefabGuid, 
            NativeHashMap<PrefabGUID,FixedString128> nativeHashMap)
        {
            return (nativeHashMap.ContainsKey(prefabGuid) 
                ? nativeHashMap[prefabGuid] : "GUID Not Found").ToString();
        }
            
        public static string LookupName(this PrefabGUID prefabGuid, 
            PrefabCollectionSystem pfCollectionSystem)
        {
            return (pfCollectionSystem.PrefabNameLookupMap.ContainsKey(prefabGuid) 
                ? pfCollectionSystem.PrefabNameLookupMap[prefabGuid] : "GUID Not Found").ToString();
        }
            
        public static  string LookupName(this PrefabGUID prefabGuid)
        {
            var pfCollectionSystem = WorldUtility
                .FindWorld("Client_0")
                .GetExistingSystem<PrefabCollectionSystem>();
                
            return (pfCollectionSystem.PrefabNameLookupMap.ContainsKey(prefabGuid) 
                ? pfCollectionSystem.PrefabNameLookupMap[prefabGuid] : "GUID Not Found").ToString();
        }
            
    }
}

namespace VMelon
{


    [SuppressMessage("ReSharper", "RedundantOverriddenMember")]
    public class HackObject : MelonMod
    {
        private float fov_level = 60.0f;
        private PlayerEspItem local_player;
        private List<GenericEspItem> _genericEspItems;
        private List<PlayerEspItem> _playerEspItems;
        private Camera cam;
        
        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasLoaded(buildIndex, sceneName);
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            base.OnSceneWasInitialized(buildIndex, sceneName);
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            base.OnSceneWasUnloaded(buildIndex, sceneName);
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }

        public override void OnApplicationStart()
        {
            base.OnApplicationStart();
        }

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                fov_level += 5f;
                Camera.main.fieldOfView = fov_level;
                LoggerInstance.Msg($"Camera FOV: {Camera.main.fieldOfView}");
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                fov_level -= 5f;
                Camera.main.fieldOfView = fov_level;
                LoggerInstance.Msg($"Camera FOV: {Camera.main.fieldOfView}");
            }
            
            if (Input.GetKeyDown(KeyCode.F10))
            {
                cam = Camera.main;
                
                if(_genericEspItems == null)
                    _genericEspItems = new List<GenericEspItem>();
                if(_playerEspItems == null)
                    _playerEspItems = new List<PlayerEspItem>();
                _genericEspItems.Clear();
                _playerEspItems.Clear();
                
                
                
                var sb = new StringBuilder();
                
                
                var world = WorldUtility.FindWorld("Client_0");
                
                var prefab = world.GetExistingSystem<PrefabCollectionSystem>();
                var comp = new ComponentType(Il2CppType.Of<EntityCategory>(), ComponentType.AccessMode.ReadOnly);
                var query = prefab.EntityManager.CreateEntityQuery(new [] {comp});
                var nameMap = prefab.PrefabNameLookupMap;

                var entities = query.ToEntityArray(Allocator.Temp);

                sb.AppendLine("\n\n");
                foreach (var ent in entities)
                {

                    var cat = prefab.EntityManager.GetComponentData<EntityCategory>(ent);
                    if (cat.UnitCategory == UnitCategory.None || cat.UnitCategory == UnitCategory.CastleObject) continue;

                    var GUID = prefab.EntityManager.GetComponentData<PrefabGUID>(ent);


                    string name = GUID.LookupName();
                    
                    switch (cat.UnitCategory)
                    {
                        case UnitCategory.None:
                            continue;
                        case UnitCategory.PlayerVampire:
                            var cHud = prefab.EntityManager.GetComponentData<CharacterHUD>(ent);
                            if(!cHud.Name.IsEmpty)
                                name = cHud.Name.ToString();
                            _playerEspItems.Add(new PlayerEspItem(ent,prefab.EntityManager));
                            break;
                        case UnitCategory.Human:
                            _genericEspItems.Add(new GenericEspItem(ent, prefab.EntityManager, $"Human ({name})"));
                            break;
                        case UnitCategory.Demon:
                            _genericEspItems.Add(new GenericEspItem(ent, prefab.EntityManager, $"Demon ({name})"));
                            break;
                        case UnitCategory.Beast:
                            _genericEspItems.Add(new GenericEspItem(ent, prefab.EntityManager, $"Beast ({name})"));
                            break;
                        case UnitCategory.Undead:
                            _genericEspItems.Add(new GenericEspItem(ent, prefab.EntityManager, $"Undead ({name})"));
                            break;
                        
                    }

                    string bloodtype = "No Blood";
                    try
                    {
                        var blood = prefab.EntityManager.GetComponentData<Blood>(ent);
                        bloodtype = (nameMap.ContainsKey(blood.BloodType)
                            ? nameMap[blood.BloodType]
                            : "Unknown").ToString();
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }

                   
                    string unitType = cat.UnitCategory.ToString();
                    string mainCat = cat.MainCategory.ToString();
                    

                    var hp_str = "X/X/X";
                    try{
                        var health = prefab.EntityManager.GetComponentData<Health>(ent);
                        hp_str = $"{health.Value}/{health.MaxRecoveryHealth}/{health.MaxHealth._Value}";
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                    
                    var lvl = "-1";
                    try{
                        var _comp = prefab.EntityManager.GetComponentData<UnitLevel>(ent);
                        lvl = _comp.Level.ToString();
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                    
                    var _str = "-1";
                    try{
                        var _comp = prefab.EntityManager.GetComponentData<BloodConsumeSource>(ent);
                        _str = _comp.UnitBloodType.LookupName() 
                               + " %"+ _comp.BloodQuality.ToString("F0");
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }

                    sb.AppendLine(
                        $"[Name:{name}] [Unit Type:{unitType}] [Main Cat:{mainCat}]" +
                        $" [Blood:{_str}] [HP:{hp_str}] [Lvl: {lvl}]");
                    
                                        
                    // foreach (var componentType in prefab.EntityManager.GetComponentTypes(ent, Allocator.Temp))
                    // {
                    //     sb.AppendLine("\t" + componentType.ToString());
                    //
                    //
                    // }
                    //

                    //prefab.EntityManager.GetComponentData<UnitLevel>(ent).Level
                    //prefab.EntityManager.GetComponentData<UnitStats>(ent).
                    //prefab.EntityManager.GetComponentData<ResistanceData>(ent).
                    //prefab.EntityManager.GetComponentData<MiscAiGameplayData>(ent)
                    //prefab.EntityManager.GetComponentData<Vision>(ent)
                    //prefab.EntityManager.GetComponentData<AggroConsumer>(ent)

                }
                
                LoggerInstance.Msg(sb.ToString());

            }
            

        }

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();
        }

        public override void OnGUI()
        { 
            GUI.Label(new Rect(10, 10, 300, 50), $"FOV [{fov_level:F0}] [F8 | F9]");

            if( _playerEspItems != null)
            {
                foreach (var player in _playerEspItems)
                {
                    GUI.color = player._color;
                    
                    var spos = cam.WorldToScreenPoint(player.GetWorldPosition());
                    
                    GUI.Label(new Rect(spos.x, Screen.height-spos.y,300,50), player.ToString());
                }
            }
            
            if (_genericEspItems != null)
            {
                foreach (var beast in _genericEspItems)
                {
                    GUI.color = beast._color;
                    
                    var spos = cam.WorldToScreenPoint(beast.GetWorldPosition());
                    
                    GUI.Label(new Rect(spos.x, Screen.height-spos.y,300,50),beast.ToString());
                }
            
            }
            GUI.color = Color.white;
        }
        
        

        class EspItem
        {
            protected static Vector3 _localPlayerPosition;
            protected static EntityManager _entManager;
            protected Entity baseEntity;
            public Color _color; 
            protected string _name;
            protected float _distance;
            protected Vector3 _worldPosition;
        }
        class GenericEspItem : EspItem
        {
            public GenericEspItem(Entity ent, EntityManager entityManager, string name)
            {
                _color = Color.red;
                _distance = 0.0f;

                _entManager = entityManager;
                baseEntity = ent;
                _name = name;

                // Position
                UpdatePosition();
            }

            private void UpdatePosition()
            {
                _worldPosition = _entManager.GetComponentData<Translation>(baseEntity).Value;
            }
            public override string ToString()
            {
                _distance = Vector3.Distance(_localPlayerPosition, _worldPosition);
                return $"[{_distance:F0}] {_name}";
            }
            public Vector3 GetWorldPosition()
            {
                UpdatePosition();
                return _worldPosition;
            }
        }
        class PlayerEspItem : EspItem
        {
            public bool IsLocalPlayer = false;
            
            public PlayerEspItem(Entity ent, EntityManager entityManager)
            {
                _color = Color.red;
                _distance = 0.0f;

                _entManager = entityManager;
                baseEntity = ent;
                
                // Name (need once)
                var cHud = entityManager.GetComponentData<CharacterHUD>(ent);
                _name = cHud.Name.ToString();
                
                // IsLocal (need Once)
                IsLocalPlayer = cHud.TeamType == CharacterHUDSettings.TeamType.LocalPlayer;
                
                // Position
                UpdatePosition();
            }

            private void UpdatePosition()
            {
                _worldPosition = _entManager.GetComponentData<Translation>(baseEntity).Value;
                if (IsLocalPlayer)
                {
                    _localPlayerPosition = _worldPosition;
                }
            }
            public override string ToString()
            {
                _distance = Vector3.Distance(_localPlayerPosition, _worldPosition);
                return $"[{_distance:F0}] {_name}";
            }
            public Vector3 GetWorldPosition()
            {
                UpdatePosition();
                return _worldPosition;
            }
        }
    }

}
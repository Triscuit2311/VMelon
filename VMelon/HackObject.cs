using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using MelonLoader;
using ProjectM;
using UnhollowerRuntimeLib;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using VMelon.EspItems;
using VMelon.Utilities;


namespace VMelon
{
    [SuppressMessage("ReSharper", "RedundantOverriddenMember")]
    public class HackObject : MelonMod
    {
        private float fov_level = 60.0f;
        private PlayerEspItem local_player;
        private List<PlayerEspItem> _playerEspItems;
        private Camera cam;
        private GUIStyle _guiStyle;

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
                
                if(_playerEspItems == null)
                    _playerEspItems = new List<PlayerEspItem>();
                _playerEspItems.Clear();
                
                
                
                var sb = new StringBuilder();

                var world = WorldUtility.FindWorld("Client_0");
                
                var prefab = world.GetExistingSystem<PrefabCollectionSystem>();
                var comp = new ComponentType(Il2CppType.Of<EntityCategory>(), 
                    ComponentType.AccessMode.ReadOnly);
                var query = prefab.EntityManager.CreateEntityQuery(new [] {comp});
                var nameMap = prefab.PrefabNameLookupMap;

                var entities = query.ToEntityArray(Allocator.Temp);

                sb.AppendLine("\n\n");
                foreach (var ent in entities)
                {

                    var cat = prefab.EntityManager.GetComponentData<EntityCategory>(ent);
                    if (cat.UnitCategory == UnitCategory.None || cat.UnitCategory == UnitCategory.CastleObject) continue;

                    var GUID = prefab.EntityManager.GetComponentData<PrefabGUID>(ent);


                    var name = GUID.LookupName();
                    
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
                            //_genericEspItems.Add(new GenericEspItem(ent, prefab.EntityManager, $"Human ({name})"));
                            break;
                        case UnitCategory.Demon:
                           // _genericEspItems.Add(new GenericEspItem(ent, prefab.EntityManager, $"Demon ({name})"));
                            break;
                        case UnitCategory.Beast:
                            //_genericEspItems.Add(new GenericEspItem(ent, prefab.EntityManager, $"Beast ({name})"));
                            break;
                        case UnitCategory.Undead:
                            //_genericEspItems.Add(new GenericEspItem(ent, prefab.EntityManager, $"Undead ({name})"));
                            break;
                        default:
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
        
        public override void OnGUI()
        {
            _guiStyle = GUI.skin.label;
            _guiStyle.fontSize = 20;
            GUI.skin.label = _guiStyle;
            
            GUI.Label(new Rect(10, 10, 300, 50), $"FOV [{fov_level:F0}] [F8 | F9]");

            if( _playerEspItems != null)
            {
                foreach (var player in _playerEspItems)
                {
                    
                    
                    var spos = cam.WorldToScreenPoint(player.GetWorldPosition());
                    
                    GUI.color = Color.black;
                    GUI.Label(new Rect(spos.x+2, Screen.height-spos.y-2,500,50), player.ToString());
                        
                    GUI.color = player._color;
                    GUI.Label(new Rect(spos.x, Screen.height-spos.y,500,50), player.ToString());
                    
                    Render.DrawCircle(new Vector2(spos.x, Screen.height-spos.y), 100, 10,
                        true, 5f);
                    
                }
            }
            GUI.color = Color.white;
        }

    }

}
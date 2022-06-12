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

            if (Input.GetKeyDown(KeyCode.F11))
            {
                Cpt_EntityCategory = new ComponentType(Il2CppType.Of<EntityCategory>(),
                    ComponentType.AccessMode.ReadOnly);
                Cpt_InventoryOwner = new ComponentType(Il2CppType.Of<InventoryOwner>(),
                    ComponentType.AccessMode.ReadOnly);
                LoggerInstance.Msg("Getting world");
                ClientWorld = WorldUtility.FindWorld("Client_0");
                LoggerInstance.Msg("Getting PFCollection");
                ClientPFCollection = ClientWorld.GetExistingSystem<PrefabCollectionSystem>();
                LoggerInstance.Msg("Getting Name Lookup");
                PfNameLookupTable = ClientPFCollection.PrefabNameLookupMap;
                LoggerInstance.Msg("Getting Ent Manager");
                ClientEntityManager = ClientPFCollection.EntityManager;
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {


                cam = Camera.main;
                
                if(_playerEspItems == null)
                    _playerEspItems = new List<PlayerEspItem>();
                _playerEspItems.Clear();
                
                
                
                var sb = new StringBuilder();

               var query = ClientEntityManager.CreateEntityQuery(new[] {Cpt_EntityCategory});
                
                var entities = query.ToEntityArray(Allocator.Temp);

                sb.AppendLine("\n\n");
                foreach (var ent in entities)
                {

                    var cat = ClientEntityManager.GetComponentData<EntityCategory>(ent);
                    if (cat.UnitCategory == UnitCategory.None || cat.UnitCategory == UnitCategory.CastleObject) continue;

                    var GUID = ClientEntityManager.GetComponentData<PrefabGUID>(ent);
                    
                    var name = GUID.LookupName();
                    
                    switch (cat.UnitCategory)
                    {
                        case UnitCategory.None:
                            continue;
                        case UnitCategory.PlayerVampire:
                            var cHud = ClientEntityManager.GetComponentData<CharacterHUD>(ent);
                            if(!cHud.Name.IsEmpty)
                                name = cHud.Name.ToString();
                            _playerEspItems.Add(new PlayerEspItem(ent,ClientEntityManager));
                            break;
                        case UnitCategory.Human:
                            //_genericEspItems.Add(new GenericEspItem(ent, ClientEntityManager, $"Human ({name})"));
                            break;
                        case UnitCategory.Demon:
                           // _genericEspItems.Add(new GenericEspItem(ent, ClientEntityManager, $"Demon ({name})"));
                            break;
                        case UnitCategory.Beast:
                            //_genericEspItems.Add(new GenericEspItem(ent, ClientEntityManager, $"Beast ({name})"));
                            break;
                        case UnitCategory.Undead:
                            //_genericEspItems.Add(new GenericEspItem(ent, ClientEntityManager, $"Undead ({name})"));
                            break;
                        default:
                            break;
                    }

                    string bloodtype = "No Blood";
                    try
                    {
                        var blood = ClientEntityManager.GetComponentData<Blood>(ent);
                        bloodtype = (PfNameLookupTable.ContainsKey(blood.BloodType)
                            ? PfNameLookupTable[blood.BloodType]
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
                        var health = ClientEntityManager.GetComponentData<Health>(ent);
                        hp_str = $"{health.Value}/{health.MaxRecoveryHealth}/{health.MaxHealth._Value}";
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                    
                    var lvl = "-1";
                    try{
                        var _comp = ClientEntityManager.GetComponentData<UnitLevel>(ent);
                        lvl = _comp.Level.ToString();
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                    
                    var _str = "-1";
                    try{
                        var _comp = ClientEntityManager.GetComponentData<BloodConsumeSource>(ent);
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
                    
                                        
                    // foreach (var componentType in ClientEntityManager.GetComponentTypes(ent, Allocator.Temp))
                    // {
                    //     sb.AppendLine("\t" + componentType.ToString());
                    //
                    //
                    // }
                    //

                    //ClientEntityManager.GetComponentData<UnitLevel>(ent).Level
                    //ClientEntityManager.GetComponentData<UnitStats>(ent).
                    //ClientEntityManager.GetComponentData<ResistanceData>(ent).
                    //ClientEntityManager.GetComponentData<MiscAiGameplayData>(ent)
                    //ClientEntityManager.GetComponentData<Vision>(ent)
                    //ClientEntityManager.GetComponentData<AggroConsumer>(ent)

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
                    
                }
            }
            GUI.color = Color.white;
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

        }
        
        
        private bool _initialized = false;
        private World ClientWorld;
        private PrefabCollectionSystem ClientPFCollection;
        private ComponentType Cpt_EntityCategory;
        private ComponentType Cpt_InventoryOwner;
        private NativeHashMap<PrefabGUID, FixedString128> PfNameLookupTable;
        private EntityManager ClientEntityManager;
    }

}
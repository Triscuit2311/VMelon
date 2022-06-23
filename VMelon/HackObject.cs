using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MelonLoader;
using ProjectM;
using UnhollowerRuntimeLib;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using VMelon.EspItems;

namespace VMelon
{
    [SuppressMessage("ReSharper", "RedundantOverriddenMember")]
    public class HackObject : MelonMod
    {
        private static List<EspItem> _espItems;

        private static Camera _cam;
        private EntityManager _clientEntityManager;
        private PrefabCollectionSystem _clientPfCollection;

        private World _clientWorld;
        private ComponentType _cptEntityCategory;
        private ComponentType _cptInventoryOwner;

        private float _entUpdateTimer = 5f;

        private float _fovLevel = 60.0f;

        private GUIStyle _guiStyle;


        public override void OnApplicationStart()
        {
            LoggerInstance.Msg("FOV Changer & ESP - Triscuit#2311");
        }

        public override void OnUpdate()
        {
            HandleFovKeys();

            _entUpdateTimer -= Time.deltaTime;

            if (!(_entUpdateTimer <= 0f)) return;

            do
            {
                _entUpdateTimer = 5f;

                if (!UpdateEntitySystems()) break;

                _cam = Camera.main;

                if (_espItems == null)
                    _espItems = new List<EspItem>();

                lock (_espItems)
                {
                    _espItems.Clear();

                    var query = _clientEntityManager.CreateEntityQuery(new[] {_cptEntityCategory});
                    var entities = query.ToEntityArray(Allocator.Temp);

                    foreach (var unitEnt in entities) ParseEntity(unitEnt);
                } // Lock
            } while (false);
        }

        private void ParseEntity(Entity unitEnt)
        {
            var cat = _clientEntityManager.GetComponentData<EntityCategory>(unitEnt);
            if (cat.MainCategory != MainEntityCategory.Unit) return;
            if (cat.UnitCategory == UnitCategory.None || cat.UnitCategory == UnitCategory.CastleObject)
                return;

            switch (cat.UnitCategory)
            {
                case UnitCategory.None:
                    return;
                case UnitCategory.PlayerVampire:
                    _espItems.Add(new PlayerEspItem(unitEnt));
                    break;
                case UnitCategory.Human:
                case UnitCategory.Demon:
                case UnitCategory.Beast:
                case UnitCategory.Undead:
                    _espItems.Add(new NpcEspItem(unitEnt));
                    break;
                case UnitCategory.Mechanical:
                case UnitCategory.CastleObject:
                case UnitCategory.Fish:
                default:
                    break;
            }
        }

        private void HandleFovKeys()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                _fovLevel += 5f;
                Camera.main.fieldOfView = _fovLevel;
                LoggerInstance.Msg($"Camera FOV changed: {Camera.main.fieldOfView}");
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                _fovLevel -= 5f;
                Camera.main.fieldOfView = _fovLevel;
                LoggerInstance.Msg($"Camera FOV changed: {Camera.main.fieldOfView}");
            }
        }

        private bool UpdateEntitySystems()
        {
            if (_cptEntityCategory == null)
                _cptEntityCategory = new ComponentType(Il2CppType.Of<EntityCategory>(),
                    ComponentType.AccessMode.ReadOnly);

            if (_cptInventoryOwner == null)
                _cptInventoryOwner = new ComponentType(Il2CppType.Of<InventoryOwner>(),
                    ComponentType.AccessMode.ReadOnly);

            if (_clientWorld == null)
                _clientWorld = WorldUtility.FindWorld("Client_0");

            try
            {
                _clientPfCollection = _clientWorld.GetExistingSystem<PrefabCollectionSystem>();
                _clientEntityManager = _clientPfCollection.EntityManager;
                EspItem.Init(_clientEntityManager);
            }
            catch (Exception e)
            {
                LoggerInstance.Warning("Unable to get entity Manger, probably not loaded in yet.");
                return false;
            }

            return true;
        }


        public override void OnGUI()
        {
            _guiStyle = GUI.skin.label;
            _guiStyle.fontSize = 20;
            GUI.skin.label = _guiStyle;

            GUI.Label(new Rect(10, 10, 300, 100), $"VMelon - Triscuit#2311\nFOV [{_fovLevel:F0}] [F8 | F9]");


            if (_espItems != null)
            {
                // GUI.Label(new Rect(10, 50, 300, 50), $"Entities: [{EspItems.Count}]");
                foreach (var item in _espItems)
                {
                    if (!item.Exists()) continue;
                    var spos = _cam.WorldToScreenPoint(item.GetWorldPosition());
                    if (spos.z < 0) continue;
                    GUI.color = Color.black;
                    GUI.Label(new Rect(spos.x - 2, Screen.height - spos.y - 2, 600, 500),
                        item.ToString());
                    GUI.color = item.Color;
                    GUI.Label(new Rect(spos.x, Screen.height - spos.y, 600, 500),
                        item.ToString());
                }

                GUI.color = Color.white;
            }

            //GUI.DrawTexture(new Rect(100, 100, 100, 100), Texture2D.whiteTexture);
        }
    }
}
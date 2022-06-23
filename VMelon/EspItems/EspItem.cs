using System;
using System.Text;
using ProjectM;
using ProjectM.UI;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace VMelon.EspItems
{
    public abstract class EspItem
    {
        internal static bool Initialized;

        internal static Vector3 LocalPlayerPosition;
        protected static EntityManager EntManager;
        public Color Color;
        protected float Distance;
        public bool Exempt = false;

        protected Entity EntObj;
        protected string Name;
        protected Vector3 WorldPosition;
        protected abstract void UpdateComponents();
        protected internal abstract Vector3 GetWorldPosition();

        protected internal virtual bool Exists()
        {
            return EntManager.Exists(EntObj);
        }

        public static void Init(EntityManager entityManager)
        {
            if (Initialized) return;
            Initialized = true;
            EntManager = entityManager;
        }
    }

    internal class PlayerEspItem : EspItem
    {
        private Blood _blood;
        private Equipment _equipment;
        private Health _health;
        public readonly bool IsLocalPlayer;

        public PlayerEspItem(Entity ent)
        {
            if (!Initialized)
                throw new Exception("EspItem Base class not initialized");
            Color = Color.red;
            Distance = 0.0f;

            EntObj = ent;

            // Name (need once)
            var cHud = EntManager.GetComponentData<CharacterHUD>(ent);
            Name = cHud.Name.ToString();

            // IsLocal (need Once)
            IsLocalPlayer = cHud.TeamType == CharacterHUDSettings.TeamType.LocalPlayer;
            if (IsLocalPlayer)
                base.Exempt = true;

        }

        private void UpdatePosition()
        {
            WorldPosition = EntManager.GetComponentData<Translation>(EntObj).Value;
            if (IsLocalPlayer && !_health.IsDead) LocalPlayerPosition = WorldPosition;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            Distance = Vector3.Distance(LocalPlayerPosition, WorldPosition);

            sb.Append($"* [{Distance:F0}m] ");
            sb.Append($"{Name} [Lv: {_equipment.GetFullLevel()}]");
            sb.Append(
                $"\n[{_health.Value:F0}/{_health.MaxHealth._Value:F0} HP] [{_blood.Value:F0}/{_blood.MaxBlood._Value:F0} Bl]");
            sb.Append($"\n[{_blood.BloodType.LookupBlood()} {_blood.Quality}%]");
            return sb.ToString();
        }

        protected override void UpdateComponents()
        {
            _blood = EntManager.GetComponentData<Blood>(EntObj);
            _health = EntManager.GetComponentData<Health>(EntObj);
            _equipment = EntManager.GetComponentData<Equipment>(EntObj);
        }


        protected internal override Vector3 GetWorldPosition()
        {
            UpdatePosition();
            UpdateComponents();
            return WorldPosition;
        }
    }

    internal class NpcEspItem : EspItem
    {
        private float _bloodQuality;
        private string _bloodType;
        private Health _health;
        private UnitLevel _unitLevel;

        public NpcEspItem(Entity ent)
        {
            if (!Initialized)
                throw new Exception("EspItem Base class not initialized");

            _bloodQuality = 0;
            _bloodType = "None";

            Color = Color.yellow;
            Distance = 0.0f;

            EntObj = ent;

            var guid = EntManager.GetComponentData<PrefabGUID>(ent);

            Name = guid.LookupChar();
        }

        private void UpdatePosition()
        {
            WorldPosition = EntManager.GetComponentData<Translation>(EntObj).Value;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            Distance = Vector3.Distance(LocalPlayerPosition, WorldPosition);

            sb.Append($"* [{Distance:F0}m]");
            sb.Append($" {Name} [Lv: {_unitLevel.Level}]");
            sb.Append($"\n[{_health.Value:F0}/{_health.MaxHealth._Value:F0} HP] ");
            sb.Append($"[{_bloodType} {_bloodQuality:F0}%]");
            return sb.ToString();
        }

        protected override void UpdateComponents()
        {
            _health = EntManager.GetComponentData<Health>(EntObj);
            _unitLevel = EntManager.GetComponentData<UnitLevel>(EntObj);
            if (EntManager.HasComponent<BloodConsumeSource>(EntObj))
            {
                var bcs = EntManager.GetComponentData<BloodConsumeSource>(EntObj);
                _bloodQuality = bcs.BloodQuality;
                _bloodType = bcs.UnitBloodType.LookupBlood();
            }
        }

        protected internal override Vector3 GetWorldPosition()
        {
            UpdatePosition();
            UpdateComponents();
            return WorldPosition;
        }
    }
}
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
        protected static bool _initialized = false;
        protected static Vector3 _localPlayerPosition;
        protected static EntityManager _entManager;
        
        
        protected Entity baseEntity;
        public Color _color; 
        protected string _name;
        protected float _distance;
        protected Vector3 _worldPosition;
        protected abstract void UpdateComponents();
    }
    
    class PlayerEspItem : EspItem
    {
        public bool IsLocalPlayer = false;
        private Health _health;
        private Blood _blood;
        
        //private UnitLevel
        
        public PlayerEspItem(Entity ent, EntityManager entityManager)
        {
            if (!_initialized)
            {
                _initialized = true;
                EspItem._entManager = entityManager;
            }
            
            _color = Color.red;
            _distance = 0.0f;

            baseEntity = ent;
            
            // Name (need once)
            var cHud = entityManager.GetComponentData<CharacterHUD>(ent);
            _name = cHud.Name.ToString();
            
            // IsLocal (need Once)
            IsLocalPlayer = cHud.TeamType == CharacterHUDSettings.TeamType.LocalPlayer;
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
            var sb = new StringBuilder();
            _distance = Vector3.Distance(_localPlayerPosition, _worldPosition);

            sb.Append($"[{_distance:F0}] ");
            sb.Append($"{_name} ");
            sb.Append($"[{_health.Value:F0}/{_health.MaxHealth._Value:F0}] ");
            sb.Append($"[{_blood.BloodType.LookupBlood()} :" +
                      $" [{_blood.Value}/{_blood.MaxBlood._Value}] :" +
                      $" {_blood.Quality}%]");
            return sb.ToString();
        }

        protected override void UpdateComponents()
        {
            _blood = _entManager.GetComponentData<Blood>(baseEntity);
            _health = _entManager.GetComponentData<Health>(baseEntity);
        }

        public Vector3 GetWorldPosition()
        {
            UpdatePosition();
            UpdateComponents();
            return _worldPosition;
        }
    }
}
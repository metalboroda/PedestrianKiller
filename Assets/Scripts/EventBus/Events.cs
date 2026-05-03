using Assets.Scripts.Character;
using Assets.Scripts.Enums;
using Assets.Scripts.FSM;
using Assets.Scripts.GameManagement;
using Assets.Scripts.ScriptableObjects.Weapons;
using UnityEngine;

namespace Assets.Scripts.EventBus
{
    public static class Events
    {
        #region Game
        public struct GameStateChanged : IEvent
        {
            public IState<GameStateManager> State;
        }
        #endregion

        #region UI
        public struct UIButtonClicked : IEvent
        {
            public UiButtonType ButtonType;
        }

        public struct CanvasChanged : IEvent { }

        public struct SelectorItemPlayPressed : IEvent
        {
            public int Index;
            public int Rating;
        }

        public struct UiMusicClicked : IEvent { }

        public struct UiSfxClicked : IEvent { }

        public struct VibrationClicked : IEvent { }

        public struct TutorialCompleted : IEvent { }
        #endregion

        #region Audio
        public struct VoiceoverPlayed : IEvent
        {
            public bool IsVoiceoverPlayed;
        }
        #endregion

        #region CoinManager
        public struct CoinIncreased : IEvent
        {
            public int CoinAmount;
        }

        public struct BuyRequest : IEvent
        {
            public string RequestName;
            public int Price;
        }

        public struct BuyResponse : IEvent
        {
            public string ResponseName;
            public bool Response;
        }
        #endregion

        #region Shop
        public struct ShopItemSelected : IEvent { }
        #endregion

        #region Character
        public struct BodyPartDamaged : IEvent
        {
            public float BaseDamage;
            public BodyPartType BodyPartType;
            public float DamageMultiplier;
            public int CharacterID;

            public Vector3 HitPoint;
            public Vector3 HitNormal;
            public GameObject HitObject;

            public bool SpawnDecal;
        }

        public struct CharacterIsInjured : IEvent
        {
            public int CharacterID;
        }

        public struct CharacterIsDead : IEvent
        {
            public int CharacterID;
        }
        #endregion
        
        #region WeaponSystem
        public struct FireRequested : IEvent
        {
            public WeaponConfigSo Config;
            public Ray ShootingRay;
        }
        
        public struct AmmoChanged : IEvent
        {
            public int CurrentAmmo;
            public int MaxAmmo;
        }

        public struct ReloadStarted : IEvent { }
        #endregion

        #region LevelLogic
        public struct SpawnerInitialized : IEvent
        {
            public int NumberOfSpawns;
        }

        public struct PedestrianSpawned : IEvent
        {
            public int TotalSpawnedCount;
        }
        #endregion
    }
}
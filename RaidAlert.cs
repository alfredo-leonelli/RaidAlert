using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Oxide.Core.Plugins;

namespace Oxide.Plugins
{
    [Info("Raid Alert", "AnotherPanda", "1.0.0")]
    [Description("Alerts authorized players when their base is being raided")]

    class RaidAlert : CovalencePlugin
    {
        #region Definitions

        [PluginReference]
        private Plugin Clans;

        #endregion

        #region Config

        private float alertCooldown;
        private float worldSize;
        private const float gridSize = 150f;

        private Dictionary<ulong, float> lastAlertTime = new Dictionary<ulong, float>();

        #endregion

        #region Hooks

        private void Init()
        {
            LoadConfigValues();
        }

        private void LoadConfigValues()
        {
            if (Config["AlertCooldown"] == null) Config["AlertCooldown"] = 0f;
            if (Config["WorldSize"] == null) Config["WorldSize"] = 0f;

            alertCooldown = float.Parse(Config["AlertCooldown"].ToString());
            worldSize = float.Parse(Config["WorldSize"].ToString());

            SaveConfig();

            if (alertCooldown == 0 || worldSize == 0)
            {
                Puts("Configuration is not properly set.");
                Puts("Please edit the configuration file and set 'AlertCooldown' and 'WorldSize'.");
            }
        }

        protected override void LoadDefaultConfig()
        {
            Config["AlertCooldown"] = 0f;
            Config["WorldSize"] = 0f;
            SaveConfig();
        }

        private void OnEntityDeath(BaseCombatEntity entity, HitInfo info)
        {
            if (entity == null || info == null || info.Initiator == null || worldSize == 0)
            {
                return;
            }

            if (!(info.Initiator is BasePlayer attacker))
            {
                return;
            }

            string weaponName = info.WeaponPrefab?.ShortPrefabName ?? "Unknown";
            if (!IsRaidDamage(weaponName))
            {
                return;
            }

            BuildingPrivlidge tc = GetNearbyTC(entity);
            if (tc != null)
            {
                if (IsAuthorizedOrClanMember(attacker, tc))
                {
                    return;
                }

                string gridPosition = GetGridPosition(entity.transform.position);
                NotifyAuthorizedPlayers(tc, gridPosition);
            }
        }

        #endregion

        #region Utility Functions

        private bool IsRaidDamage(string weaponName)
        {
            List<string> raidWeapons = new List<string>
            {
                "rocket_launcher", "rocket_basic", "rocket_fire", "rocket_hv",
                "explosive.satchel.deployed", "satchel.charge.deployed",
                "explosive.timed.deployed", "c4.deployed",
                "grenade.f1.deployed", "grenade.beancan.deployed"
            };

            return raidWeapons.Any(weaponName.Contains);
        }

        private BuildingPrivlidge GetNearbyTC(BaseCombatEntity entity)
        {
            List<BuildingPrivlidge> tcList = new List<BuildingPrivlidge>();
            Vis.Entities(entity.transform.position, 30f, tcList);
            return tcList.FirstOrDefault();
        }

        private bool IsAuthorizedOrClanMember(BasePlayer attacker, BuildingPrivlidge tc)
        {
            foreach (var auth in tc.authorizedPlayers)
            {
                if (auth.userid == attacker.userID)
                {
                    return true;
                }
            }

            if (Clans != null)
            {
                string attackerClan = Clans?.Call<string>("GetClanOf", attacker.UserIDString);
                if (!string.IsNullOrEmpty(attackerClan))
                {
                    foreach (var auth in tc.authorizedPlayers)
                    {
                        string authorizedClan = Clans?.Call<string>("GetClanOf", auth.userid.ToString());
                        if (authorizedClan == attackerClan)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void NotifyAuthorizedPlayers(BuildingPrivlidge tc, string gridPosition)
        {
            foreach (var auth in tc.authorizedPlayers)
            {
                BasePlayer basePlayer = BasePlayer.FindByID(auth.userid);
                if (basePlayer != null && basePlayer.IsConnected && !HasRecentAlert(basePlayer.userID))
                {
                    SendMessage(basePlayer, $"Tu base en {gridPosition} está siendo raideada!");
                    lastAlertTime[basePlayer.userID] = Time.realtimeSinceStartup;
                }
            }
        }

        private bool HasRecentAlert(ulong userID)
        {
            return lastAlertTime.ContainsKey(userID) && (Time.realtimeSinceStartup - lastAlertTime[userID]) < alertCooldown;
        }

        private string GetGridPosition(Vector3 position)
        {
            float halfWorld = worldSize / 2f;

            int adjustedX = Mathf.FloorToInt((position.x + halfWorld) / gridSize);
            int adjustedZ = Mathf.FloorToInt((worldSize / gridSize) - ((position.z + halfWorld) / gridSize));

            char column = (char)('A' + adjustedX);
            return $"{column}{adjustedZ}";
        }

        private void SendMessage(BasePlayer player, string message)
        {
            player.ChatMessage(message);
        }

        #endregion
    }
}

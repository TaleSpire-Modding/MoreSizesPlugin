﻿using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using RadialUI;
using Bounce.Unmanaged;
using HarmonyLib;
using LordAshes;
using MoreSizesPlugin.Patches;
using PluginUtilities;

namespace MoreSizesPlugin
{

    public static class RadialExtensions
    {
        public static GameObject FindChild(this GameObject gameObject, string name, bool includeInactive = false)
        {
            Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>(includeInactive);
            RectTransform[] rectTransforms = gameObject.GetComponentsInChildren<RectTransform>(includeInactive);

            if (childTransforms != null)
                foreach (Transform transform in childTransforms)
                {
                    GameObject child = transform.gameObject;
                    if (child?.name == name)
                        return child;
                }

            if (rectTransforms != null)
                foreach (Transform transform in rectTransforms)
                {
                    GameObject child = transform.gameObject;
                    if (child?.name == name)
                        return child;
                }

            return null;
        }
    }

    [BepInPlugin(Guid, "More Sizes Plug-In", Version)]
    [BepInDependency(RadialUIPlugin.Guid)]
    [BepInDependency(AssetDataPlugin.Guid)]
    [BepInDependency(FileAccessPlugin.Guid)]
    [BepInDependency(SetInjectionFlag.Guid)]
    public class MoreSizesPlugin : BaseUnityPlugin
    {
        // constants
        private const string Guid = "org.hollofox.plugins.MoreSizesPlugin";
        private const string Version = "2.2.0.0";
        private const string key = "org.lordashes.plugins.extraassetsregistration.Aura.";
        private static CreatureGuid _selectedCreature;

        // Dictionaries
        private readonly Dictionary<CreatureGuid, List<string>> _knownCreatureEffects =
            new Dictionary<CreatureGuid, List<string>>();

        // Config
        private ConfigEntry<string> _customSizes;

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            Logger.LogInfo("In Awake for More Sizes");
            _customSizes = Config.Bind("Sizes", "List", JsonConvert.SerializeObject(new List<float>
            {
                0.5f,
                0.75f,
                1f,
                1.5f,
                2f,
                3f,
                4f,
                6f,
                8f,
                10f,
                15f,
                20f,
                25f,
                30f,
            }),new ConfigDescription("", null, new ConfigurationManagerAttributes
            {
                IsJSON = true
            }));
            var harmony = new Harmony(Guid);
            harmony.PatchAll();
            Logger.LogDebug("MoreSizes Plug-in loaded");

            ModdingTales.ModdingUtils.AddPluginToMenuList(this, "Hollofoxes'");

            RadialUIPlugin.HideDefaultEmotesGMItem(Guid,"Set Size");
            RadialUIPlugin.AddCustomButtonGMSubmenu("Set Size",
                new MapMenu.ItemArgs
                {
                    Action = HandleSubmenus,
                    Icon = FileAccessPlugin.Image.LoadSprite("MoreSizesPlugin.png"),
                    CloseMenuOnActivate = false,
                    Title = "Set Size",
                }
                ,Reporter);

            // StatMessaging
            AssetDataPlugin.Subscribe("*", HandleRequest);
        }

        private void HandleSubmenus(MapMenuItem arg1, object arg2)
        {
            CreaturePresenter.TryGetAsset(_selectedCreature, out CreatureBoardAsset asset);
            var c = asset;
            if (!_knownCreatureEffects.ContainsKey(c.CreatureId) || _knownCreatureEffects[c.CreatureId].Count == 0)
                OpenResizeMini(arg1, arg2);
            else OpenSelectAsset(arg1, arg2);
        }

        private void OpenSelectAsset(MapMenuItem arg1, object arg2)
        {
            CreaturePresenter.TryGetAsset(_selectedCreature, out CreatureBoardAsset asset);
            var c = asset;
            MapMenu mapMenu = MapMenuManager.OpenMenu(c.transform.position + Vector3.up * CreatureMenuBoardPatch._hitHeightDif, true);

            mapMenu.AddItem(new MapMenu.ItemArgs
            {
                Title = "Set creature size",
                Action = OpenResizeMini,
                CloseMenuOnActivate = false,
                Icon = Icons.GetIconSprite("creaturesize")
            });

            foreach (var effect in _knownCreatureEffects[c.CreatureId])
            {
                var x = effect.Replace(key, "");
                
                mapMenu.AddItem(new MapMenu.ItemArgs
                {
                    Title = $"Set {x.Replace("_", " ")} size",
                    Action = OpenResizeEffect,
                    Obj = effect,
                    CloseMenuOnActivate = false,
                    Icon = Icons.GetIconSprite("MagicMissile")
                });
            }
        }

        private void OpenResizeEffect(MapMenuItem arg1, object arg2)
        {
            string effect = (string)arg2;
            CreaturePresenter.TryGetAsset(_selectedCreature, out CreatureBoardAsset asset);
            var c = asset;
            MapMenu mapMenu = MapMenuManager.OpenMenu(c.transform.position + Vector3.up * CreatureMenuBoardPatch._hitHeightDif, true);
            var sizes = JsonConvert.DeserializeObject<List<float>>(_customSizes.Value);
            foreach (var size in sizes)
            {
                if (size < 1) AddSizeEffect(mapMenu, size, effect, Icons.GetIconSprite("05x05"));
                else if (size < 2) AddSizeEffect(mapMenu, size, effect, Icons.GetIconSprite("1x1"));
                else if (size < 3) AddSizeEffect(mapMenu, size, effect, Icons.GetIconSprite("2x2"));
                else if (size < 4) AddSizeEffect(mapMenu, size, effect, Icons.GetIconSprite("3x3"));
                else AddSizeEffect(mapMenu, size, effect, Icons.GetIconSprite("4x4"));
            }
        }

        private void OpenResizeMini(MapMenuItem arg1, object arg2)
        {
            CreaturePresenter.TryGetAsset(_selectedCreature, out CreatureBoardAsset asset);
            var c = asset;
            MapMenu mapMenu = MapMenuManager.OpenMenu(c.transform.position + Vector3.up * CreatureMenuBoardPatch._hitHeightDif, true);
            var sizes = JsonConvert.DeserializeObject<List<float>>(_customSizes.Value);
            foreach (var size in sizes)
            {
                if (size < 1) AddSize(mapMenu, size, Icons.GetIconSprite("05x05"));
                else if (size < 2) AddSize(mapMenu, size, Icons.GetIconSprite("1x1"));
                else if (size < 3) AddSize(mapMenu, size, Icons.GetIconSprite("2x2"));
                else if (size < 4) AddSize(mapMenu, size, Icons.GetIconSprite("3x3"));
                else AddSize(mapMenu, size, Icons.GetIconSprite("4x4"));
            }
        }

        private void AddSize(MapMenu mapMenu, float x, Sprite icon = null)
        {
            mapMenu.AddItem(new MapMenu.ItemArgs
            {
                Title = $"{x}x{x}",
                Action = Menu_Scale,
                Obj = x,
                CloseMenuOnActivate = true,
                Icon = icon
            });
        }

        private void AddSizeEffect(MapMenu mapMenu, float x, string effect, Sprite icon = null)
        {
            mapMenu.AddItem(new MapMenu.ItemArgs
            {
                Title = $"{x}x{x}",
                Action = Scale_Effect,
                Obj = new effectResize{key = effect, value = x},
                CloseMenuOnActivate = true,
                Icon = icon
            });
        }

        private void Scale_Effect(MapMenuItem arg1, object arg2)
        {
            var er = (effectResize) arg2;
            AssetDataPlugin.SetInfo(_selectedCreature.ToString(), $"size.{er.key}", JsonConvert.SerializeObject(er));
        }

        private bool Reporter(NGuid arg1, NGuid arg2)
        {
            _selectedCreature = new CreatureGuid(arg2);
            return true;
        }

        private void Menu_Scale(MapMenuItem item, object obj)
        {
            var fetch = AssetDataPlugin.ReadInfo(_selectedCreature.ToString(), Guid);
            dto scale;
            if (!string.IsNullOrWhiteSpace(fetch))
            {
                scale = JsonConvert.DeserializeObject<dto>(fetch);
            }
            else
            {
                CreaturePresenter.TryGetAsset(_selectedCreature, out var asset);
                scale = new dto
                {
                    X = asset.transform.localScale.x,
                    Y = asset.transform.localScale.y,
                    Z = asset.transform.localScale.z
                };
            }

            scale.value = (float) obj;
            CreatureManager.SetCreatureScale(_selectedCreature ,0, (float)obj);
            AssetDataPlugin.SetInfo(_selectedCreature.ToString(), Guid, JsonConvert.SerializeObject(scale));
        }

        public static void SetValue(object o, string methodName, object value)
        {
            var mi = o.GetType().GetField(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi != null) mi.SetValue(o, value);
        }

        public void HandleRequest(AssetDataPlugin.DatumChange change)
        {
            if (change.key.Contains($"size.{key}"))
            {
                var previous = JsonConvert.DeserializeObject<effectResize>(change.previous.ToString());
                var er = JsonConvert.DeserializeObject<effectResize>(change.value.ToString());

                var goName = change.key.Replace($"size.{key}", "");
                var AssetName = $"CustomAura:{change.source}.{goName}";

                var creatureId = new CreatureGuid(change.source);
                CreaturePresenter.TryGetAsset(creatureId, out CreatureBoardAsset asset);

                var me = asset.gameObject.FindChild(AssetName);
                me.gameObject.SetActive(false);
                me.gameObject.SetActive(true);

                var all = me.transform.GetComponentsInChildren<ParticleSystem>();
                var allT = me.transform.GetComponentsInChildren<Transform>();

                if (change.action == AssetDataPlugin.ChangeAction.modify)
                {
                    foreach (var p in all)
                    {
                        p.transform.localScale /= previous.value;
                    }
                    foreach (var t in allT)
                    {
                        t.localScale = t.transform.localScale / previous.value;
                    }
                }
                if (change.action != AssetDataPlugin.ChangeAction.remove)
                {
                    foreach (var p in all)
                    {
                        p.transform.localScale *= er.value;
                    }
                    foreach (var t in allT)
                    {
                        t.localScale = t.transform.localScale * er.value;
                    }
                }

                Logger.LogDebug($"Change to size: ({change.value}, {er.value})");
            }
            else if (change.key.Contains(key))
            {
                var x = change.key.Replace(key, "");
                if (change.action == AssetDataPlugin.ChangeAction.add)
                {
                    if (!_knownCreatureEffects.ContainsKey(new CreatureGuid(change.source)))
                        _knownCreatureEffects[new CreatureGuid(change.source)] = new List<string>();
                    _knownCreatureEffects[new CreatureGuid(change.source)].Add(change.key);
                }
                else if (change.action == AssetDataPlugin.ChangeAction.remove)
                {
                    if (_knownCreatureEffects.ContainsKey(new CreatureGuid(change.source)))
                        _knownCreatureEffects[new CreatureGuid(change.source)].Remove(change.key);
                }
            }
            if (change.key == Guid)
            {
                var creatureId = new CreatureGuid(change.source);
                var size = JsonConvert.DeserializeObject<dto>(change.value.ToString());
                CreaturePresenter.TryGetAsset(creatureId, out CreatureBoardAsset asset);
                SetValue(asset, "_scaleTransitionValue", 0f);
                SetValue(asset, "_targetScale", size.value);
            }
        }
    }
}

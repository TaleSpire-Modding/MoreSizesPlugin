using System.Collections.Generic;
using System.Linq;
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
using RadialUI.Extensions;

namespace MoreSizesPlugin
{

    [BepInPlugin(Guid, "HolloFoxes' More Sizes Plug-In", Version)]
    [BepInDependency(RadialUIPlugin.Guid)]
    [BepInDependency(StatMessaging.Guid)]
    public class MoreSizesPlugin : BaseUnityPlugin
    {
        // constants
        private const string Guid = "org.hollofox.plugins.MoreSizesPlugin";
        private const string Version = "2.0.0.0";
        private const string key = "org.lordashes.plugins.extraassetsregistration.Aura.";
        private static CreatureGuid _selectedCreature;

        // Dictionaries
        private readonly Dictionary<CreatureGuid, List<string>> _knownCreatureEffects =
            new Dictionary<CreatureGuid, List<string>>();

        /*private readonly Dictionary<CreatureGuid, List<effectResize>> _CreatureAuras =
            new Dictionary<CreatureGuid, List<effectResize>>();

        private readonly Dictionary<CreatureGuid, List<effectResize>> _CreatureAuraQueue =
            new Dictionary<CreatureGuid, List<effectResize>>();
        */
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
                6,
                8,
                10,
                15,
                20,
                25,
                30,
            }));
            var harmony = new Harmony(Guid);
            harmony.PatchAll();
            Debug.Log("MoreSizes Plug-in loaded");

            ModdingTales.ModdingUtils.Initialize(this, Logger);

            RadialUIPlugin.HideDefaultEmotesGMItem(Guid,"Set Size");
            RadialUIPlugin.AddCustomButtonGMSubmenu("Set Size",
                new MapMenu.ItemArgs
                {
                    Action = HandleSubmenus,
                    Icon = Icons.GetIconSprite("creaturesize"),
                    CloseMenuOnActivate = false,
                    Title = "Set Size",
                }
                ,Reporter);

            // StatMessaging
            StatMessaging.Subscribe("*", HandleRequest);
        }

        /*
        void Update()
        {
            List<CreatureGuid> cgDone = new List<CreatureGuid>();
            foreach (var key in _CreatureAuraQueue.Keys) {
                CreaturePresenter.TryGetAsset(_selectedCreature, out var asset);
                var entries = _CreatureAuraQueue[key];
                List<effectResize> done = new List<effectResize>();
                foreach (var entry in entries)
                {
                    var me = asset.gameObject.FindChild(entry.key);
                    if (me != null)
                    {
                        var all = me.transform.GetComponentsInChildren<ParticleSystem>();
                        foreach (var p in all) {
                            p.transform.localScale = p.transform.localScale * entry.value;
                        }
                        done.Add(entry);
                    }
                }
                entries.RemoveAll(e => done.Contains(e));
                if (entries.Count == 0) cgDone.Add(key);
            }
            foreach (var done in cgDone)
            {
                _CreatureAuraQueue.Remove(done);
            }
        }
        */

        private void HandleSubmenus(MapMenuItem arg1, object arg2)
        {
            CreaturePresenter.TryGetAsset(_selectedCreature, out CreatureBoardAsset asset);
            var c = asset.Creature;
            if (!_knownCreatureEffects.ContainsKey(c.CreatureId) || _knownCreatureEffects[c.CreatureId].Count == 0)
                OpenResizeMini(arg1, arg2);
            else OpenSelectAsset(arg1, arg2);
        }

        private void OpenSelectAsset(MapMenuItem arg1, object arg2)
        {
            CreaturePresenter.TryGetAsset(_selectedCreature, out CreatureBoardAsset asset);
            var c = asset.Creature;
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
            var c = asset.Creature;
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
            var c = asset.Creature;
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
            StatMessaging.SetInfo(_selectedCreature, $"size.{er.key}", JsonConvert.SerializeObject(er));
        }

        private bool Reporter(NGuid arg1, NGuid arg2)
        {
            _selectedCreature = new CreatureGuid(arg2);
            return true;
        }

        private void Menu_Scale(MapMenuItem item, object obj)
        {
            var fetch = StatMessaging.ReadInfo(_selectedCreature, Guid);
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
                    X = asset.CreatureLoaders[0].transform.localScale.x,
                    Y = asset.CreatureLoaders[0].transform.localScale.y,
                    Z = asset.CreatureLoaders[0].transform.localScale.z
                };
            }

            scale.value = (float) obj;
            CreatureManager.SetCreatureScale(_selectedCreature ,0, (float)obj);
            StatMessaging.SetInfo(_selectedCreature, Guid, JsonConvert.SerializeObject(scale));
        }

        public static void SetValue(object o, string methodName, object value)
        {
            var mi = o.GetType().GetField(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi != null) mi.SetValue(o, value);
        }

        public void HandleRequest(StatMessaging.Change[] changes)
        {
            foreach (StatMessaging.Change change in changes)
            {
                if (change.key.Contains($"size.{key}"))
                {
                    var previous = JsonConvert.DeserializeObject<effectResize>(change.previous);
                    var er = JsonConvert.DeserializeObject<effectResize>(change.value);

                    var goName = change.key.Replace($"size.{key}", "");
                    var AssetName = $"CustomAura:{change.cid}.{goName}";
                    
                    var creatureId = change.cid;
                    CreaturePresenter.TryGetAsset(creatureId, out CreatureBoardAsset asset);

                    var me = asset.gameObject.FindChild(AssetName);
                    me.gameObject.SetActive(false);
                    me.gameObject.SetActive(true);

                    var all = me.transform.GetComponentsInChildren<ParticleSystem>();
                    var allT = me.transform.GetComponentsInChildren<Transform>();

                    if (change.action == StatMessaging.ChangeType.modified)
                    {
                        foreach (var p in all)
                        {
                            p.transform.localScale = p.transform.localScale / previous.value;
                        }
                        foreach (var t in allT)
                        {
                            t.localScale = t.transform.localScale / previous.value;
                        }
                    }
                    if (change.action != StatMessaging.ChangeType.removed)
                    {
                        foreach (var p in all)
                        {
                            p.transform.localScale = p.transform.localScale * er.value;
                        }
                        foreach (var t in allT)
                        {
                            t.localScale = t.transform.localScale * er.value;
                        }
                    }

                    Debug.Log($"Change to size: ({change.value}, {er.value})");
                }
                else if (change.key.Contains(key))
                {
                    var x = change.key.Replace(key,"");
                    var AssetName = $"CustomAura:{change.cid}.{x}";
                    if (change.action == StatMessaging.ChangeType.added)
                    {
                        if (!_knownCreatureEffects.ContainsKey(change.cid))
                            _knownCreatureEffects[change.cid] = new List<string>();
                        _knownCreatureEffects[change.cid].Add(change.key);
                        /*
                        if (_CreatureAuras.ContainsKey(change.cid) &&
                            _CreatureAuras[change.cid].Any(e => e.key == change.key))
                        {
                            if (!_CreatureAuraQueue.ContainsKey(change.cid))
                            {
                                _CreatureAuraQueue.Add(change.cid, new List<effectResize>());
                            }
                            var entry = _CreatureAuras[change.cid].Single(e => e.key == change.key);
                            _CreatureAuraQueue[change.cid].Add(entry);
                        }*/
                    } else if (change.action == StatMessaging.ChangeType.removed)
                    {
                        if (_knownCreatureEffects.ContainsKey(change.cid))
                            _knownCreatureEffects[change.cid].Remove(change.key);
                    }
                }
                if (change.key == Guid)
                {
                    var creatureId = change.cid;
                    var size = JsonConvert.DeserializeObject<dto>(change.value);
                    CreaturePresenter.TryGetAsset(creatureId, out CreatureBoardAsset asset);
                    SetValue(asset, "_scaleTransitionValue",0f);
                    SetValue(asset, "_targetScale", size.value);
                }
            }

        }
    }
}

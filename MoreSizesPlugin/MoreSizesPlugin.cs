using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using RadialUI;
using Bounce.Unmanaged;
using LordAshes;

namespace MoreSizesPlugin
{

    [BepInPlugin(Guid, "HolloFoxes' More Sizes Plug-In", Version)]
    [BepInDependency(RadialUIPlugin.Guid)]
    [BepInDependency(StatMessaging.Guid)]
    public class MoreSizesPlugin : BaseUnityPlugin
    {
        // constants
        private const string Guid = "org.hollofox.plugins.MoreSizesPlugin";
        private const string Version = "1.4.0.0";

        private ConfigEntry<string> CustomSizes;

        /// <summary>
        /// Awake plugin
        /// </summary>
        void Awake()
        {
            Logger.LogInfo("In Awake for More Sizes");

            Debug.Log("MoreSizes Plug-in loaded");

            ModdingTales.ModdingUtils.Initialize(this, Logger);

            // Remove Initial Buttons to Replace
            RemoveSize(0.5f);
            RemoveSize(1);
            RemoveSize(2);
            RemoveSize(3);
            RemoveSize(4);

            CustomSizes = Config.Bind("Sizes", "List", JsonConvert.SerializeObject(new List<float>
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
            var sizes = JsonConvert.DeserializeObject<List<float>>(CustomSizes.Value);
            foreach (var size in sizes)
            {
                if (size < 1) AddSize(size, Icons.GetIconSprite("05x05"));
                else if (size < 2) AddSize(size, Icons.GetIconSprite("1x1"));
                else if (size < 3) AddSize(size, Icons.GetIconSprite("2x2"));
                else if (size < 4) AddSize(size, Icons.GetIconSprite("3x3"));
                else AddSize(size, Icons.GetIconSprite("4x4"));
            }

            // StatMessaging
            StatMessaging.Subscribe(Guid, HandleRequest);
        }

        private void AddSize(float x, Sprite icon = null)
        {
            RadialUIPlugin.AddOnSubmenuSize(
                Guid + $"{x}x{x}",
                new MapMenu.ItemArgs
                {
                    Title = $"{x}x{x}<size=0>new",
                    Action = Menu_Scale,
                    Obj = x,
                    CloseMenuOnActivate = true,
                    Icon = icon
                }, Reporter
            );
        }

        private static void RemoveSize(float x)
        {
            RadialUIPlugin.AddOnRemoveSubmenuSize(Guid, $"{x}x{x}");
        }

        private static CreatureGuid _selectedCreature;

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

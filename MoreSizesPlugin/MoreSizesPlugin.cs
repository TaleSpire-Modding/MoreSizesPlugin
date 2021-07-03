using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx;
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
        private const string Version = "1.0.1.0";

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

            // Register Group Menus in a branch
            AddSize(0.5f, Icons.GetIconSprite("05x05"));
            AddSize(0.75f, Icons.GetIconSprite("05x05"));
            AddSize(1, Icons.GetIconSprite("1x1"));
            AddSize(2, Icons.GetIconSprite("2x2"));
            AddSize(3, Icons.GetIconSprite("3x3"));
            AddSize(4, Icons.GetIconSprite("4x4"));
            AddSize(5, Icons.GetIconSprite("4x4"));
            AddSize(6, Icons.GetIconSprite("4x4"));
            AddSize(7, Icons.GetIconSprite("4x4"));
            AddSize(8, Icons.GetIconSprite("4x4"));
            AddSize(10, Icons.GetIconSprite("4x4"));
            AddSize(15, Icons.GetIconSprite("4x4"));
            AddSize(20, Icons.GetIconSprite("4x4"));
            AddSize(25, Icons.GetIconSprite("4x4"));
            AddSize(30, Icons.GetIconSprite("4x4"));

            // StatMessaging
            StatMessaging.Subscribe(Guid, HandleRequest);
        }

        private void AddSize(float x, Sprite icon = null)
        {
            RadialUIPlugin.AddOnSubmenuSize(
                Guid + $"{x}x{x}",
                new MapMenu.ItemArgs
                {
                    Title = $"{x}x{x}",
                    Action = Menu_Scale,
                    Obj = ((object) x),
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
                Debug.Log(fetch);
                scale = JsonConvert.DeserializeObject<dto>(fetch);
            }
            else
            {
                CreaturePresenter.TryGetAsset(_selectedCreature, out var asset);
                scale = new dto
                {
                    X = asset.CreatureLoaders[0].transform.localScale.x,
                    Y = asset.CreatureLoaders[0].transform.localScale.y,
                    Z = asset.CreatureLoaders[0].transform.localScale.z,
                };
            }

            scale.value = (float) obj;
            StatMessaging.SetInfo(_selectedCreature, Guid, JsonConvert.SerializeObject(scale));


            CreatureManager.SetCreatureScale(_selectedCreature, (float) obj);
        }

        private static Sprite sprite(string FileName)
        {
            var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Texture2D tex = new Texture2D(32, 32);
            tex.LoadImage(System.IO.File.ReadAllBytes(dir + "\\" + FileName));
            return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Looping method run by plugin
        /// </summary>
        void Update()
        {

        }

        public void HandleRequest(StatMessaging.Change[] changes)
        {
            Debug.Log(changes);
            foreach (StatMessaging.Change change in changes)
            {
                if (change.key == Guid)
                {
                    var creatureId = change.cid;
                    var size = JsonConvert.DeserializeObject<dto>(change.value);
                    CreaturePresenter.TryGetAsset(creatureId, out var asset);
                    Debug.Log(asset.CreatureLoaders[0].transform.localScale);
                    Vector3 localScale = new Vector3(size.X, size.Y, size.Z);
                    if (size.value > 4)
                    {
                        asset.CreatureLoaders[0].transform.localScale = localScale * size.value / 4f;
                    }
                    else
                    {
                        asset.CreatureLoaders[0].transform.localScale = localScale;
                    }
                    Debug.Log(asset.CreatureLoaders[0].transform.localScale);
                }
            }

        }
    }
}

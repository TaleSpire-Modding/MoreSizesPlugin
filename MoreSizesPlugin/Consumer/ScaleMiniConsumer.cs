using Bounce.Unmanaged;
using MoreSizesPlugin.Consumer.Messages;
using RPCPlugin.Interfaces;
using UnityEngine;

namespace MoreSizesPlugin.Consumer
{
    [InitOnLoad]
    internal class ScaleMiniConsumer : RpcConsumer<ScaleMini>
    {
        private static readonly ScaleMiniConsumer instance = new ScaleMiniConsumer();
        static ScaleMiniConsumer() { } // Make sure it's truly lazy

        public static ScaleMiniConsumer Instance { get { return instance; } }

        // My constructor (If I wanted something to happen)
        private ScaleMiniConsumer() : base()
        {
        }

        /// <summary>
        /// Event that's triggered once receiving the message that was sent
        /// </summary>
        public override void Handle(ScaleMini message)
        {
            Debug.Log($"Message Received, {message.size}, {message.cid}");
            
            if (NGuid.TryParseHexString(message.cid, out NGuid guid))
            {
                CreaturePresenter.TryGetAsset(new CreatureGuid(guid), out CreatureBoardAsset asset);
                var t = asset.gameObject.transform.GetChild(0).GetChild(0);
                t.localScale = new Vector3(message.size, message.size, message.size);
            }
        }

    }
}

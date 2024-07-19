using RPCPlugin.Interfaces;
using System;
using ZeroFormatter;

namespace MoreSizesPlugin.Consumer.Messages
{
    [ZeroFormattable]
    public class ScaleMini : RpcMessage
    {
        [Index(0)]
        public virtual float size { get; set; }
        

        //internal CreatureGuid creatureId { get; set; }

        [Index(1)]
        public virtual string cid { get; set; }

        public ScaleMini()
        {

        }

        // Serialize to Binary
        public override byte[] Value()
        {
            UnityEngine.Debug.Log($"size: {size}, cid {cid}");
            return ZeroFormatterSerializer.Serialize(this);
        }

        // Construct from Binary
        public ScaleMini(byte[] data)
        {
            UnityEngine.Debug.Log("data:" + data.ToString());
            var temp = ZeroFormatterSerializer.Deserialize<ScaleMini>(data);
            size = temp.size;
            cid = temp.cid;
            UnityEngine.Debug.Log($"size: {size}, cid {cid}");
        }
    }
}

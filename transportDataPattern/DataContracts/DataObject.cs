using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;


namespace transportDataParrern
{
    [DataContract]
    [KnownType("GetKnownTypes")]
    public class ServerDataObject
    {
        [DataMember]
        public Candle[][] NewCandles { get; set; }
        [DataMember]
        public Trade[] NewTrades { get; set; }
        [DataMember]
        public DateTimeOffset TerminalTime { get; set; }
        [DataMember]
        public DateTimeOffset LastEnterTime { get; set; }
        [DataMember]
        public DateTimeOffset LastExitTime { get; set; }

        static Type[] GetKnownTypes()
        { 
            return new Type[]
            {
                typeof(TimeFrameCandle), typeof(RangeCandle), typeof(VolumeCandle), 
                typeof(TickCandle), typeof(PnFCandle), typeof(RenkoCandle)
            };
        }
    }
}

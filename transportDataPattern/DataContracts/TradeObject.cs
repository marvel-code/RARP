using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;


namespace transportDataParrern
{
    [DataContract]
    public class TradeState {
        [DataMember]
        public int RuleId { get; set; }
        [DataMember]
        public bool LongOpen { get; set; }
        [DataMember]
        public bool LongClose { get; set; }
        [DataMember]
        public bool ShortOpen { get; set; }
        [DataMember]
        public bool ShortClose { get; set; }
        [DataMember]
        public AdditionalDataStruct AdditionalData { get; set; }
    }
    
    public struct AdditionalDataStruct
    {
        public string  message;

        public decimal adx_val;
        public decimal adx_dip;
        public decimal adx_dim;
        public decimal adx_val_p;
        public decimal adx_dip_p;
        public decimal adx_dim_p;

        public decimal total;
        public decimal total_p;
        public decimal buy;
        public decimal buy_p;
        public decimal sell;
        public decimal sell_p;

        public int Candles_N;
        public int AllTrades_N;

        public DateTime Open_Time;
        public DateTime Close_Time;
        public DateTime Open_Trades_Time;
        public DateTime Close_Trades_Time;
    }
}
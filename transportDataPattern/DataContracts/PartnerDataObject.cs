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
    public class SecuritiesRow
    {
        [DataMember]
        public string code { get; set; }
    }
    
    [DataContract]
    public class DerivativePortfolioRow
    {
        [DataMember]
        public decimal variationMargin { get; set; }
        [DataMember]
        public decimal beginAmount { get; set; }
    }
    [DataContract]
    public class DerivativePositionsRow
    {
        [DataMember]
        public int currentPosition { get; set; }
    }
    
    [DataContract]
    public class TradeData
    {
        [DataMember]
        public long id { get; set; }
        [DataMember]
        public string dateTime { get; set; }
        [DataMember]
        public decimal price { get; set; }
        [DataMember]
        public int volume { get; set; }
        [DataMember]
        public long orderNumber { get; set; }
    }
    [DataContract]
    public class OrderData
    {
        [DataMember]
        public long id { get; set; }
        [DataMember]
        public long derivedOrderId { get; set; }
        [DataMember]
        public string dateTime { get; set; }
        [DataMember]
        public string secCode { get; set; }
        [DataMember]
        public decimal price { get; set; }
        [DataMember]
        public int volume { get; set; }
        [DataMember]
        public int balance { get; set; }
        [DataMember]
        public string side { get; set; }
        [DataMember]
        public string state { get; set; }
        [DataMember]
        public string comment { get; set; }
    }
    [DataContract]
    public class StopOrderData
    {
        [DataMember]
        public long id { get; set; }
        [DataMember]
        public string dateTime { get; set; }
        [DataMember]
        public string secCode { get; set; }
        [DataMember]
        public decimal price { get; set; }
        [DataMember]
        public decimal stopPrice { get; set; }
        [DataMember]
        public int volume { get; set; }
        [DataMember]
        public int balance { get; set; }
        [DataMember]
        public string side { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public string state { get; set; }
        [DataMember]
        public string comment { get; set; }
    }

    // MAIN
    [DataContract]
    public class PartnerDataObject
    {
        [DataMember]
        public decimal Position_PNL { get; set; }
        [DataMember]
        public decimal Position_PNL_MAX { get; set; }
        [DataMember]
        public bool Is_Trading { get; set; }
        [DataMember]
        public string lastEnterDirection { get; set; }
        [DataMember]
        public List<SecuritiesRow> securitiesData { get; set; }
        [DataMember]
        public List<DerivativePortfolioRow> derivativePortfolioData { get; set; }
        [DataMember]
        public List<DerivativePositionsRow> derivativePositionsData { get; set; }
        [DataMember]
        public List<TradeData> tradesData { get; set; }
        [DataMember]
        public List<OrderData> ordersData { get; set; }
        [DataMember]
        public List<StopOrderData> stopOrdersData { get; set; }
    }
}
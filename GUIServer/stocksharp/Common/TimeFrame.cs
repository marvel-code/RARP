using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StockSharp.Algo.Strategies;
using StockSharp.Algo.Candles;
using StockSharp.Algo;
using StockSharp.BusinessEntities;

using Ecng.Common;

namespace stocksharp
{
    public partial class TimeFrame
    {
        private ServiceContracts.ProcessingData _processingData;
        
        public decimal period { get { return (decimal)Period.TotalSeconds; } }
        public decimal currentPrice { get { return GetCandle().ClosePrice; } }

        public TimeSpan Period { get; private set; }
        public List<Candle> Buffer { get; private set; }

        public List<ADX> adx { get; private set; }
        public List<MA> ma { get; private set; }
        public List<ROC> roc { get; private set; }
        public List<KAMA> kama { get; private set; }
        public List<BBW> bbw { get; private set; }
        public Vol volume { get { return Volume[0]; } }
        public List<Vol> Volume { get; private set; }

        public TimeFrame(ServiceContracts.ProcessingData processingData, int _Period_Seconds)
        {
            _processingData = processingData;
            Buffer = new List<Candle>();

            adx = new List<ADX>();
            ma = new List<MA>();
            roc = new List<ROC>();
            kama = new List<KAMA>();
            bbw = new List<BBW>();
            Volume = new List<Vol>();

            Period = TimeSpan.FromSeconds(_Period_Seconds);
        }
        // Обработка свечи
        public void Process_Candle(Candle candle)
        {
            if (candle == null)
                throw new Exception("candle");

            Add_Candle(candle);
        }
        // Упорядоченное добавление свечи в буффер
        public void Add_Candle(Candle _candle)
        {
            var _buffer = Buffer;

            if (_buffer.Count == 0)
                _buffer.Add(_candle);
            {
                // Находим свечу, совпадающую с текущей
                Candle candle_same = _buffer.Find(c => c.Time == _candle.Time);
                if (candle_same != null)
                    // Если нашли, заменяем
                    _buffer[_buffer.IndexOf(candle_same)] = _candle;
                else
                {   // Вставляем упорядоченно
                    int k = -1;
                    for (int i = 0; i < _buffer.Count; i++)
                    {
                        if (_buffer[i].Time > _candle.Time)
                        {
                            k = i;
                            break;
                        }
                    }
                    if (k == -1)
                        _buffer.Add(_candle);
                    else
                        _buffer.Insert(k, _candle);
                }
            }

            Buffer = _buffer;
        }
        // Обновляем индикаторы
        public void Update_Indicators()
        {
            var _buffer = Buffer;

            if (_buffer.Count == 0)
                return;
            // VOLUME
            foreach (var ind in Volume)
            {
                // Если разница между последней свечёй и последним значением больше периода
                if (_buffer[_buffer.Count - 1].Time - ind.Get_OpenTime() > Period && ind.Get_OpenTime().TimeOfDay != new TimeSpan(14, 0, 0).Add(-Period))
                {
                    ind.Reset();
                    for (int i = 0; i < _buffer.Count; i++)
                        ind.Update(_buffer[i]);
                }
                else
                    ind.Update(_buffer[_buffer.Count - 1]);
            }
            // ADX
            foreach (var ind in adx)
            {
                // Если разница между последней свечёй и последним значением больше периода
                if (_buffer[_buffer.Count - 1].Time - ind.Get_OpenTime() > Period && ind.Get_OpenTime().TimeOfDay != new TimeSpan(14, 0, 0).Add(-Period))
                {
                    ind.Reset();
                    for (int i = 0; i < _buffer.Count - 1; i++)
                        ind.Update(_buffer.GetRange(i, 2));
                }
                else
                    ind.Update(_buffer);
            }
            // BBW
            foreach (var ind in bbw)
            {
                // Если разница между последней свечёй и последним значением больше периода
                if (_buffer[_buffer.Count - 1].Time - ind.Get_OpenTime() > Period && ind.Get_OpenTime().TimeOfDay != new TimeSpan(14, 0, 0).Add(-Period))
                {
                    ind.Reset();
                    for (int i = 1; i < _buffer.Count; i++)
                        ind.Update(_buffer.GetRange(0, i));
                }
                else
                    ind.Update(_buffer);
            }
            // MA
            foreach (var ind in ma)
            {
                // Если разница между последней свечёй и последним значением больше периода
                if (_buffer[_buffer.Count - 1].Time - ind.Get_OpenTime() > Period && ind.Get_OpenTime().TimeOfDay != new TimeSpan(14, 0, 0).Add(-Period))
                {
                    ind.Reset();
                    for (int i = 0; i < _buffer.Count; i++)
                        ind.Update(_buffer[i]);
                }
                else
                    ind.Update(_buffer[_buffer.Count - 1]);
            }
            // ROC
            foreach (var ind in roc)
            {
                // Если разница между последней свечёй и последним значением больше периода
                if (_buffer[_buffer.Count - 1].Time - ind.Get_OpenTime() > Period && ind.Get_OpenTime().TimeOfDay != new TimeSpan(14, 0, 0).Add(-Period))
                {
                    ind.Reset();
                    for (int i = 1; i < _buffer.Count; i++)
                        ind.Update(_buffer.GetRange(0, i));
                }
                else
                    ind.Update(_buffer);
            }
            // KAMA
            foreach (var ind in kama)
            {
                // Если разница между последней свечёй и последним значением больше периода
                if (_buffer[_buffer.Count - 1].Time - ind.Get_OpenTime() > Period && ind.Get_OpenTime().TimeOfDay != new TimeSpan(14, 0, 0).Add(-Period))
                {
                    ind.Reset();
                    for (int i = 1; i < _buffer.Count; i++)
                        ind.Update(_buffer.GetRange(0, i));
                }
                else
                    ind.Update(_buffer);
            }

            Buffer = _buffer;
        }
        // Проверка целостности буффера
        public int Check_Buffer_Valid()
        {

            int result = 0;
            for (int i = 1; i < Buffer.Count; i++)
                if (Buffer[i].Time != Buffer[i - 1].Time.Add(Period) && Buffer[i - 1].Time.Add(Period).AddHours(10) != Buffer[i].Time && Buffer[i - 1].Time.TimeOfDay.Hours != 23 && Buffer[i].Time.TimeOfDay != new TimeSpan(14, 0, 0).Add(Period))
                {
                    result = i;
                }
            return result;
        }
        // Взятие свечи из буффера
        public Candle GetCandle(int _shift = 0)
        {
            return Buffer.Count <= _shift ? null : Buffer[Buffer.Count - 1 - _shift];
        }
    }
}
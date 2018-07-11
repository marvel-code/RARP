using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUIServer
{   
    public class PartnerInfo
    {
        public string login { get;  set; }
        public bool isOnline { get;  set; }
        public bool allowTrade { get;  set; }
        public bool is_Trading { get;  set; }
        public string lastConnectionTime { get; set; }

        public PartnerInfo(string _login, bool _allowTrade)
        {
            login = _login;
            isOnline = false;
            allowTrade = _allowTrade;
            is_Trading = false;
        }
    }

    public static class PartnersManager
    {
        // Partners info
        private static List<PartnerInfo> _partnersInfo;
        
        private static void UpdateDgvPartnersInfoSource() { MainWindow.Instance.UpdateDgvPartnersInfoSource(); }

        // Actions on partners list
        public static List<PartnerInfo> GetPartnersInfo() { return _partnersInfo; }
        public static void UpdatePartnerState(string login, bool isOnline)
        {
            _partnersInfo.Find(x => x.login == login).isOnline = isOnline;

            UpdateDgvPartnersInfoSource();
        }
        public static void UpdatePartnerLastConnectionTime(string login, DateTime dateTime)
        {
            _partnersInfo.Find(x => x.login == login).lastConnectionTime = dateTime.ToString(@"yyyy/MM/dd HH:mm:ss");

            UpdateDgvPartnersInfoSource();
        }
        public static void UpdatePartnerTradingState(string login, bool is_Trading)
        {
            _partnersInfo.Find(x => x.login == login).is_Trading = is_Trading;

            UpdateDgvPartnersInfoSource();
        }

        public static void UploadPartnersInfoToFile()
        {
            try
            {
                File.WriteAllText(Globals.partnersDB_fullFileName, JsonConvert.SerializeObject(_partnersInfo));

                LogManager.Log(LogType.Info, "Список клиентов сохранен.");
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "Ошибка сохранение списка клиентов: {0}", ex.ToString());
            }
        }
        public static void DownloadPartnersInfoFromFile() 
        {
            try
            {
                string[] fileLines;
                if (File.Exists(Globals.partnersDB_fullFileName) && (fileLines = File.ReadAllLines(Globals.partnersDB_fullFileName)).Length != 0)
                {
                    var deserialized = JsonConvert.DeserializeObject<List<PartnerInfo>>(fileLines[0]);
                    if (deserialized == null)
                        throw new Exception("Список пуст");
                    _partnersInfo = deserialized;
                    _partnersInfo.ForEach(x => x.isOnline = false);

                    LogManager.Log(LogType.Info, "Список клиентов загружен.");
                }
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "Ошибка загрузки списка клиентов: {0}", ex.ToString());
            }
            
            if (_partnersInfo == null)
                _partnersInfo = new List<PartnerInfo>();
        }

        public static void AddPartnerInfo(PartnerInfo newClientInfo)
        {
            try
            {
                if (MessageBox.Show(string.Format("Добавить клиента `{0}`?", newClientInfo.login), "Добавление клиента", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                    // Check: login exist
                    if (_partnersInfo.Exists(x => x.login == newClientInfo.login))
                {
                    LogManager.Log(LogType.Info, "Клиент `{0}` не добавлен. Такой клиент уже существует.", newClientInfo.login);

                    return;
                }

                // Success
                _partnersInfo.Add(newClientInfo);
                
                LogManager.Log(LogType.Info, "Клиент `{0}` добавлен. Разрешено торговать - {1}.", newClientInfo.login, newClientInfo.allowTrade ? "ДА" : "НЕТ");

                UpdateDgvPartnersInfoSource();
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "Ошибка добавления клиента: {0}", ex);
            }
        }
        public static void EditPartnerInfo(string login, bool allowTrade, ushort strategyNumber)
        {
            if (MessageBox.Show(string.Format("Изменить клиента `{0}`?\nНовые настройки: Разрешено торговать - {1}. Номер стратегии - {2}", login, allowTrade ? "ДА" : "НЕТ", strategyNumber), "Изменение клиента", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                // Check: login exist
                if (!_partnersInfo.Exists(x => x.login == login))
                {
                    LogManager.Log(LogType.Info, "Клиент `{0}` не изменён. Такого клиента не существует.", login);

                    return;
                }

                // Success
                var target = _partnersInfo.Find(x => x.login == login);
                target.allowTrade = allowTrade;

                LogManager.Log(LogType.Info, "Клиент `{0}` изменён. Разрешено торговать - {1}. Номер стратегии - {2}", login, allowTrade ? "ДА" : "НЕТ", strategyNumber);

                UpdateDgvPartnersInfoSource();
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "Ошибка изменения клиента: {0}", ex);
            }
        }
        public static void RemovePartnerInfo(string login)
        {
            if (MessageBox.Show(string.Format("Удалить клиента `{0}`?", login), "Удаление клиента", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                // Check: login exist
                if (!_partnersInfo.Exists(x => x.login == login))
                {
                    LogManager.Log(LogType.Info, "Клиент `{0}` не удалён. Такого клиента не существует.", login);

                    return;
                }

                // Success
                _partnersInfo.Remove(_partnersInfo.Find(x => x.login == login));

                LogManager.Log(LogType.Info, "Клиент `{0}` удалён.", login);

                UpdateDgvPartnersInfoSource();
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "Ошибка удаления клиента: {0}", ex);
            }
        }
    }
}
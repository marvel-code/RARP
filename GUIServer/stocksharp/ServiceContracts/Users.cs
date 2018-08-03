using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Windows.Forms;

using GUIServer;

namespace stocksharp.ServiceContracts
{
    public class UserManager
    {
        private static volatile bool doStopUserThread;
        private static Thread threadUserTimeoutCheck;

        public static List<string> usersList { get; private set; }
        public static Dictionary<string, DateTime> connectedUsers;

        public static void Init(List<string> _usersList)
        {
            usersList = _usersList;
            connectedUsers = new Dictionary<string, DateTime>();

            threadUserTimeoutCheck = new Thread(() =>
            {
                doStopUserThread = false;
                while (!doStopUserThread)
                {
                    Action action = () => Process_UserTimeoutCheck();
                    MainWindow.Instance.BeginInvoke(action);

                    Thread.Sleep(30 * 1000);
                }
            });
            threadUserTimeoutCheck.Start();
        }
        public static void Dispose()
        {
            //doStopUserThread = true;
            threadUserTimeoutCheck.Abort();
        }
        
        public static void ReaffirmUserConnection(string username)
        {
            if (true || connectedUsers.ContainsKey(username))
            {
                Update_UserData(username);
            }
            else
            {
                Log.addLog(LogType.Warn, "Попытка поддержки соединения с неизвестным юзером: `{0}`", username);
            }
        }

        public static bool Process_UserConnect(string username, string comment = "")
        {
            if (!usersList.Contains(username)/* || connectedUsers.Keys.Contains(username) */|| PartnersManager.GetPartnersInfo().Find(x => x.login == username) != null && !PartnersManager.GetPartnersInfo().Find(x => x.login == username).allowTrade)
            {
                Log.addLog(LogType.Info, "{0}: CONNECTION FAILED {1}", username, comment);
                return false;
            }

            connectedUsers.Add(username, DateTime.Now);
            PartnersManager.UpdatePartnerState(username, true);

            Log.addLog(LogType.Info, "{0}: connected {1}", username, comment);
            return true;
        }
        public static bool Process_UserDisconnect(string username, string comment = "")
        {
            connectedUsers.Remove(username);
            PartnersManager.UpdatePartnerState(username, false);
            PartnersManager.UpdatePartnerTradingState(username, false);

            Log.addLog(LogType.Info, "{0}: disconnected {1}", username, comment);
            return true;
        }
        public static void Process_UserTimeoutCheck()
        {
            for (int i = 0; i < connectedUsers.Count; i++)
            {
                var userInfo = connectedUsers.ElementAt(connectedUsers.Count - 1 - i);
                if (DateTime.Now.Subtract(userInfo.Value) > TimeSpan.FromMinutes(0.5))
                {
                    Process_UserDisconnect(userInfo.Key, "TIMEOUT");
                }
            }
        }

        // костыли
        public static void Update_UserData(string username)
        {
            // Time
            if (connectedUsers.ContainsKey(username))
            {
                connectedUsers[username] = DateTime.Now;
                PartnersManager.UpdatePartnerLastConnectionTime(username, DateTime.Now);
            }

            // Tables
            //MainWindow.Instance.UpdateDgvsPartnerDataSource(username);
        }
    }
}
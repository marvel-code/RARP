﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ServiceModel;
using System.ServiceModel.Description;

using stocksharp.ServiceContracts;
using SharedProject;

namespace GUIServer
{
    public static class MyServerManager
    {
        private static ServiceHost host;

        private static void host_Opened(object sender, EventArgs e)
        {
            LogManager.Log(LogType.Info, "Сервер открыт.");

            ProcessingData.Init();
            UserManager.Init(PartnersManager.GetPartnersInfo().Select(x => x.login).ToList());
        }
        private static void host_Opening(object sender, EventArgs e)
        {
            LogManager.Log(LogType.Debug, "Открытие сервера...");
        }
        private static void host_Closed(object sender, EventArgs e)
        {
            LogManager.Log(LogType.Info, "Сервер закрыт.");
        }
        private static void host_Closing(object sender, EventArgs e)
        {
            LogManager.Log(LogType.Debug, "Закрытие сервера...");

            UserManager.Dispose();
            host = null;
        }


        public static bool isServerOpened { get { return host == null ? false : host.State == CommunicationState.Opened; } }

        public static void OpenServer()
        {
            try
            {
                Uri address = new Uri("http://localhost:8010/WorkService");
                WSHttpBinding binding = new WSHttpBinding(SecurityMode.None, true);

                binding.Security = new WSHttpSecurity() { Mode = SecurityMode.None };

                int sizeMb = 500;
                binding.MaxReceivedMessageSize = sizeMb * 1024 * 1024;

                host = new ServiceHost(typeof(WorkService), address);
                host.AddServiceEndpoint(typeof(IWorkService), binding, "");
                host.Description.Behaviors.Add(new ServiceMetadataBehavior { HttpGetEnabled = true });
                host.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");
                

                host.Opening += new EventHandler(host_Opening);
                host.Opened += new EventHandler(host_Opened);
                host.Closing += new EventHandler(host_Closing);
                host.Closed += new EventHandler(host_Closed);

                host.Open();
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "Ошибка открытия сервера: {0}",  ex.ToString());
            }
        }
        public static void CloseServer()
        {
            try
            {
                if (isServerOpened)
                    host.Close();
            }
            catch (Exception ex)
            {
                LogManager.Log(LogType.Error, "Ошибка закрытия сервера: {0}", ex.ToString());
            }
        }
    }
}
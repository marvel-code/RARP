using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace transportDataParrern
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IWorkService
    {
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        string InitConnection(string username);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        TradeState GetTradeState(PartnerDataObject partnerDataObject, ServerDataObject dataObj, NeedAction needAction);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        List<int> GetTimeFramePeriods();
        
        [OperationContract(IsInitiating = false, IsTerminating = false)]
        void LogAction(string action);

        [OperationContract(IsInitiating = false, IsTerminating = false)]
        void ReaffirmConnection();
        
        [OperationContract(IsInitiating = false, IsTerminating = true)]
        void TerminateConnection();
    }
}
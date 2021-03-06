﻿using System;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace Oms.Transport.Wcf
{
    /// <summary>
    /// Allows us to adorn the IService contract with this
    /// attribute to indicate that a specialized DataContractSerializer
    /// should be used that has preserveObjectReferences set true
    /// </summary>

    public class ReferencePreservingDataContractFormatAttribute
            : Attribute, IOperationBehavior
    {
        #region IOperationBehavior Members
        public void AddBindingParameters(OperationDescription description,
            BindingParameterCollection parameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription description,
            System.ServiceModel.Dispatcher.ClientOperation proxy)
        {
            IOperationBehavior innerBehavior =
              new ReferencePreservingDataContractSerializerOperationBehavior(description);
            innerBehavior.ApplyClientBehavior(description, proxy);
        }

        public void ApplyDispatchBehavior(OperationDescription description,
            System.ServiceModel.Dispatcher.DispatchOperation dispatch)
        {
            IOperationBehavior innerBehavior =
              new ReferencePreservingDataContractSerializerOperationBehavior(description);
            innerBehavior.ApplyDispatchBehavior(description, dispatch);
        }

        public void Validate(OperationDescription description)
        {
        }

        #endregion
    }
}

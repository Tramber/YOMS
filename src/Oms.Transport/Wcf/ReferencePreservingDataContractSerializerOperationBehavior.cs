using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;

namespace Oms.Transport.Wcf
{
    /// <summary>
    /// A specialized DataContractSerializer that has 
    /// preserveObjectReferences set true, which allows for
    /// circular references to be serialized
    /// </summary>
    public class ReferencePreservingDataContractSerializerOperationBehavior :
        DataContractSerializerOperationBehavior
    {
        #region Ctor
        public ReferencePreservingDataContractSerializerOperationBehavior(
            OperationDescription operationDescription)
            : base(operationDescription) { }
        #endregion

        #region Public Methods

        public override XmlObjectSerializer CreateSerializer(Type type,
            XmlDictionaryString name, XmlDictionaryString ns,
            IList<Type> knownTypes)
        {
            return new DataContractSerializer(type, name, ns, knownTypes,
                2147483646 /*maxItemsInObjectGraph*/,
                false/*ignoreExtensionDataObject*/,
                true/*preserveObjectReferences*/,
                null/*dataContractSurrogate*/);
        }
        #endregion
    }
}
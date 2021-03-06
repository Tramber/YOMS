﻿using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Microsoft.Practices.Unity;

namespace Oms.Transport.Wcf
{
    public class UnityInstanceContextExtension : IExtension<InstanceContext>
    {
        private IUnityContainer _childContainer;

        public IUnityContainer GetChildContainer(IUnityContainer container)
        {
            if (container == null)
            {
                throw new ArgumentNullException("container");
            }

            return _childContainer ?? (_childContainer = container.CreateChildContainer());
        }

        public void DisposeOfChildContainer()
        {
            if (_childContainer != null)
            {
                _childContainer.Dispose();
            }
        }

        public void Attach(InstanceContext owner)
        {
        }

        public void Detach(InstanceContext owner)
        {
        }
    }
}
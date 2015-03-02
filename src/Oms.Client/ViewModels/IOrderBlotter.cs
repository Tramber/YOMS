﻿using System.Collections;

namespace Oms.Client.ViewModels
{
    public interface IOrderBlotter
    {
        bool CanUnGroupOrders(object arg);
        bool CanGroupOrders(IList list);
        void UnGroupOrders(object obj);
        void GroupOrders(IList list);
    }
}
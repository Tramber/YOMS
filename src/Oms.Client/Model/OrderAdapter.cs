using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Oms.Transport.Contracts.Dto;


namespace Oms.Client.Model
{
    public class OrderAdapter : ObservableObject
    {
        private double _quantity;
        private int _groupId;
        public int Id { get; set; }

        public double Quantity
        {
            get { return _quantity; }
            set
            {
                SetProperty(ref _quantity, value);
            }
        }

        public double Price { get; set; }

        public OrderDtoWay Way { get; set; }

        public SecurityDto Security { get; set; }

        public UserDto Owner { get; set; }

        public UserDto Creator { get; set; }

        public string Version { get; set; }

        public DateTime CreationDate { get; set; }

        public OrderDtoType OrderType { get; set; }

        public OrderDtoValidityType Validity { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int GroupId
        {
            get { return _groupId; }
            set
            {
                SetProperty(ref _groupId, value);
            }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebMvc.Models.Orders;

namespace WebMvc.Services.Orders
{
    public class OrderService : IOrderService
    {

        // Lisa you have to implement this interface to get The Real Data from the OrderMicroservice
        Task<Order> IOrderService.GetOrderByIdAsync(int orderId)
        {
            throw new NotImplementedException();
        }

        Task<List<Order>> IOrderService.GetOrdersByBuyerAsync(string buyerId, int page, int take)
        {
            throw new NotImplementedException();
        }

        Task<List<OrderTicket>> IOrderService.GetOrderTicketByOrderIdAsync(int orderId, int page, int take)
        {
            throw new NotImplementedException();
        }
    }
}

﻿using OnlineTakeawayStore.StaticDomainEvents.Model;
using OnlineTakeawayStore.StaticDomainEvents.Model.Events;
using OnlineTakeawayStore.StaticDomainEvents.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineTakeawayStore.Application.EventHandlers
{
    public class EmailBlacklistedCustomerOnOrderInvalidation : IHandleEvents<FoodDeliveryOrderRejectedDueToBlacklistedCustomer>
    {
        private IEmailer emailer;

        public EmailBlacklistedCustomerOnOrderInvalidation(IEmailer emailer)
        {
            this.emailer = emailer;
        }

        public void Handle(FoodDeliveryOrderRejectedDueToBlacklistedCustomer @event)
        {
            emailer.NotifyBlacklistedCustomerRejection(@event.Order.CustomerId);
        }
    }
      
}

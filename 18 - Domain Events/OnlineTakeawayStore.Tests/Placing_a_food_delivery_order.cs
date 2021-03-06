﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using OnlineTakeawayStore.Application;
using OnlineTakeawayStore.Domain;
using OnlineTakeawayStore.StaticDomainEvents.Model;
using OnlineTakeawayStore.StaticDomainEvents.Infrastructure;

namespace OnlineTakeawayStore.Tests
{
    [TestClass]
    public class Placing_a_food_delivery_order
    {
        // application service collaborators
        static INotificationChannel client = MockRepository.GenerateStub<INotificationChannel>();
        static IRestaurantConnector connector = MockRepository.GenerateStub<IRestaurantConnector>();

        // services used in event handlers (previously would have also been collaborators)
        static IEmailer emailer = MockRepository.GenerateStub<IEmailer>();
        static IFoodDeliveryOrderRepository repository = MockRepository.GenerateStub<IFoodDeliveryOrderRepository>();
        static ICustomerBehaviorChecker checker = MockRepository.GenerateStub<ICustomerBehaviorChecker>();

        // test data
        static Guid customerId = Guid.NewGuid();
        static DateTime deliveryTime = DateTime.Now.AddHours(1);
        static List<int> menuItemÌds = new List<int> { 46, 23, 921 };
        static Guid restaurantId = Guid.NewGuid();

        [ClassInitialize]
        public static void When_placing_a_food_delivery_order(TestContext ctx)
        {
            DomainEvents.ClearAll();

            DomainHandlersRegister.WireUpDomainEventHandlers(repository, checker);
            ServiceLayerHandlersRegister.WireUpDomainEventHandlers(emailer);
            checker.Stub(x => x.IsBlacklisted(customerId)).Return(false);

            var service = new FoodDeliveryOrderService(client, connector);
            var request = new PlaceFoodDeliveryOrderRequest
            {
                CustomerId = customerId,
                DeliveryTime = deliveryTime,
                MenuItemIds = menuItemÌds,
                RestaurantId = restaurantId
            };
            service.PlaceFoodDeliveryOrder(request);
        }

        [TestMethod]
        public void The_order_will_be_acknowledged_with_an_email()
        {
            emailer.AssertWasCalled(e => e.SendFoodDeliveryOrderAcknowledgement(customerId));
        }

        [TestMethod]
        public void A_real_time_notification_of_order_acknowledged()
        {
            client.AssertWasCalled(c => c.Publish("ORDER_ACKNOWLEDGED_"), x => x.IgnoreArguments());
            var arg = client.GetArgumentsForCallsMadeOn(c => c.Publish(""))[0][0];
            Assert.IsTrue(arg.ToString().StartsWith("ORDER_ACKNOWLEDGED"));
        }

        [TestMethod]
        public void A_record_of_the_order_will_be_saved()
        {
            // this is how you get arguments passed into methods with rhino mocks
            // in this case, getting the order to check the repository saved it
            FoodDeliveryOrder savedOrder = (FoodDeliveryOrder)repository.GetArgumentsForCallsMadeOn(r => r.Save(null), x => x.IgnoreArguments())[0][0];

            // the order that was saved must match the details of the customer's order
            Assert.AreEqual(customerId, savedOrder.CustomerId);
            Assert.AreEqual(restaurantId, savedOrder.RestaurantId);
            Assert.AreEqual(menuItemÌds, savedOrder.MenuItemIds);
            Assert.AreEqual(deliveryTime, savedOrder.RequestedDeliveryTime);
        }
    }
      
}

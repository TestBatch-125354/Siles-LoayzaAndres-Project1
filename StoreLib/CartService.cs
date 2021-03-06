using System;
using System.Collections.Generic;
using Serilog;
using StoreDB;
using StoreDB.Models;

namespace StoreLib
{
    public class CartService : ICartService
    {
        private ICartRepo repo;
        //public Customer Customer { get; private set; }
        //public Location Location { get; private set; }
        //public Cart Cart { get; private set; }

        public CartService(ICartRepo repo /*, Customer customer, Location location*/)
        {
            this.repo = repo;
            //this.Customer = customer;
            //this.Location = location;
            //CreateCartIfItDoesntExist();
            //this.Cart = repo.GetCart(Customer.Id, Location.Id);
        }

        public void AddToCart(CartItem item)
        {
            try 
            {
                if (repo.HasCartItem(item))
                {
                    repo.GetCartItem(item.CartId, item.ProductId).Quantity += item.Quantity;
                }
                else
                {
                    repo.AddCartItem(item);
                }
                Log.Information($"Added {item.Quantity} of {item.ProductId} to cart {item.CartId}.");
            }
            catch (Exception)
            {
                Log.Warning($"Failed to add {item.ProductId} to cart.");
                throw;
            }
        }

        private void CreateCartIfItDoesntExist(int customerId, int locationId)
        {
            if (!repo.HasCart(customerId, locationId))
            {
                Cart cart = new Cart();
                cart.LocationId = locationId;
                cart.CustomerId = customerId;
                cart.Items = new List<CartItem>();
                repo.AddCart(cart);
                Log.Information($"Created new cart at location {locationId} for customer {customerId}.");
            }
        }

        public void EmptyCart(int cartId)
        {
            repo.EmptyCart(cartId);
            Log.Information($"Emptied cart {cartId}.");
        }

        public Cart GetCart(int customerId, int locationId)
        {
            CreateCartIfItDoesntExist(customerId, locationId);
            return repo.GetCart(customerId, locationId);
        }

        public List<CartItem> GetCartItems(int cartId)
        {
            return repo.GetCartItems(cartId);
        }

        public decimal GetCost(int cartId)
        {
            List<CartItem> items = repo.GetCartItems(cartId);
            decimal total = 0;
            foreach (var item in items)
            {
                total += (item.Quantity * item.Price);
            }
            return total;
        }

        /// <summary>
        /// Assumes the specified Order has LocationId, CustomerId, 
        /// ReturnAddress, and DestinationAddress defined.
        /// </summary>
        public void PlaceOrder(Order order)
        {
            Cart cart = repo.GetCart(order.CustomerId, order.LocationId);
            order.CreationTime = DateTime.Now;
            order.Items = new List<OrderItem>();

            foreach (CartItem c in GetCartItems(cart.Id))
            {
                order.Items.Add(new OrderItem()
                {
                    Product = c.Product,
                    Quantity = c.Quantity,
                    Price = c.Price
                });
            }
            order.Cost = GetCost(cart.Id);
            repo.PlaceOrderTransaction(order.LocationId, cart.Id, order);
            Log.Information($"Placed new order for customer {order.CustomerId} at location {order.LocationId}.");
        }

        public Order PlaceOrder(int customerId, int locationId, Address returnAdd, Address destAdd)
        {
            Cart cart = repo.GetCart(customerId, locationId);
            Order order = new Order()
            {
                LocationId = locationId,
                CustomerId = customerId,
                ReturnAddress = returnAdd,
                DestinationAddress = destAdd,
                CreationTime = DateTime.Now,
                Items = new List<OrderItem>(),
            };
            foreach (CartItem c in GetCartItems(cart.Id))
            {
                order.Items.Add(new OrderItem()
                {
                    Product=c.Product, Quantity=c.Quantity, Price=c.Price
                });
            }
            order.Cost = GetCost(cart.Id);
            repo.PlaceOrderTransaction(locationId, cart.Id, order);
            Log.Information($"Placed new order for customer {customerId} at location {locationId}.");

            return order;
        }

        public void RemoveCartItem(CartItem cartItem)
        {
            repo.RemoveCartItem(cartItem);
            Log.Information($"Removed item {cartItem.ProductId} from cart {cartItem.CartId}.");
        }
    }
}
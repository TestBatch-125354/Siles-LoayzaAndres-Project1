﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StoreDB.Models;
using StoreLib;

namespace StoreAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("Get")]
        [Produces("application/json")]
        public IActionResult GetCart(int customerId, int locationId)
        {
            try
            {
                return Ok(_cartService.GetCart(customerId, locationId));
            }
            catch (Exception e)
            {
                Log.Error($"Could not get cart. {e.Message}");
                return NotFound(e);
            }
        }

        [HttpPost("AddItem")]
        public IActionResult AddToCart(CartItem cartItem)
        {
            try
            {
                _cartService.AddToCart(cartItem);
                return StatusCode(201);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpDelete("Empty")]
        public IActionResult EmptyCart(int cartId)
        {
            try
            {
                _cartService.EmptyCart(cartId);
                return StatusCode(204);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpGet("GetItems")]
        [Produces("application/json")]
        public IActionResult GetItems(int cartId)
        {
            try
            {
                return Ok(_cartService.GetCartItems(cartId));
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpDelete("RemoveItem")]
        public IActionResult RemoveItem(CartItem cartItem)
        {
            try
            {
                _cartService.RemoveCartItem(cartItem);
                return StatusCode(204);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpPost("PlaceOrder")]
        public IActionResult PlaceOrder(Order order)
        {
            try
            {
                _cartService.PlaceOrder(order);
                return StatusCode(201);
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}

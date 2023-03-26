using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using System.Net;

namespace RedMangoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private ApiResponse _response;

        public ShoppingCartController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            _response = new ApiResponse();
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetShoppingCart(string userId)
        {
            try
            {
                ShoppingCart shoppingCart;
                if (string.IsNullOrEmpty(userId))
                {
                    //_response.IsSuccess = false;
                    //_response.StatusCode = HttpStatusCode.BadRequest;
                    //return BadRequest(_response);
                    shoppingCart = new ShoppingCart();
                }
                else
                {
                    shoppingCart = _dbContext.ShoppingCarts
                    .Include(s => s.CartItems).ThenInclude(s => s.MenuItem).FirstOrDefault(s => s.UserId == userId);
                }

                //if (shoppingCart == null)
                //{
                //    _response.StatusCode = HttpStatusCode.NotFound;
                //    _response.IsSuccess = false;
                //    return NotFound();
                //}

                if (shoppingCart.CartItems != null && shoppingCart.CartItems.Count != 0)
                {
                    shoppingCart.CartTotal = shoppingCart.CartItems.Sum(c => c.Quantity * c.MenuItem.Price);
                }

                _response.StatusCode = HttpStatusCode.OK;
                _response.Result = shoppingCart;
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return Ok(_response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantity)
        {
            ShoppingCart shoppingCart = _dbContext.ShoppingCarts.Include(s => s.CartItems).FirstOrDefault(s => s.UserId == userId);
            MenuItem menuItem = _dbContext.MenuItems.FirstOrDefault(m => m.Id == menuItemId);

            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            if (shoppingCart == null && updateQuantity > 0) //新增ShoppingCart
            {
                ShoppingCart newCart = new ShoppingCart()
                {
                    UserId = userId,
                };
                _dbContext.ShoppingCarts.Add(newCart);
                _dbContext.SaveChanges();

                CartItem newCartItem = new CartItem()
                {
                    MenuItemId = menuItemId,
                    Quantity = updateQuantity,
                    ShoppingCartId = newCart.Id,
                    MenuItem = null,
                };
                _dbContext.CartItems.Add(newCartItem);
                _dbContext.SaveChanges();
            }
            else //Update ShoppingCart
            {

                CartItem cartItemInCart = shoppingCart.CartItems.FirstOrDefault(m => m.MenuItemId == menuItemId);
                if (cartItemInCart == null) //新增CartItem
                {
                    CartItem newCartItem = new CartItem()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantity,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItem = null,
                    };
                    _dbContext.CartItems.Add(newCartItem);
                    _dbContext.SaveChanges();
                }
                else //Update CartItem
                {
                    int newQuantity = cartItemInCart.Quantity + updateQuantity;
                    if (updateQuantity == 0 || newQuantity <= 0) //remove
                    {
                        _dbContext.CartItems.Remove(cartItemInCart);
                        if (shoppingCart.CartItems.Count() == 1)
                        {
                            _dbContext.ShoppingCarts.Remove(shoppingCart);
                        }
                        _dbContext.SaveChanges();
                    }
                    else
                    {
                        cartItemInCart.Quantity = newQuantity;
                        _dbContext.SaveChanges();
                    }
                }
            }

            return _response;
        }
    }
}

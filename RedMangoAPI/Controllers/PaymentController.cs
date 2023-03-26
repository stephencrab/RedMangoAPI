using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using Stripe;
using System.Net;

namespace RedMangoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private ApiResponse _response;
        private readonly IConfiguration _configuration;

        public PaymentController(ApplicationDbContext dbContext, IConfiguration configuration,
            RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            this._dbContext = dbContext;
            this._configuration = configuration;
            this._response = new ApiResponse();
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> MakePayment(string userId)
        {
            ShoppingCart cart = _dbContext.ShoppingCarts
                .Include(s => s.CartItems)
                .ThenInclude(c => c.MenuItem)
                .FirstOrDefault(s => s.UserId == userId);

            if (cart == null || cart.CartItems == null || cart.CartItems.Count() == 0)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return BadRequest();
            }

            #region Create Payment Intent
            StripeConfiguration.ApiKey = _configuration.GetValue<string>("StripeSettings:SecretKey");
            cart.CartTotal = cart.CartItems.Sum(c => c.Quantity * c.MenuItem.Price);

            var options = new PaymentIntentCreateOptions
            {
                Amount = Convert.ToInt32(cart.CartTotal * 100),
                Currency = "twd",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };
            var service = new PaymentIntentService();
            var response = service.Create(options);
            cart.StripePaymentIntentId = response.Id;
            cart.ClientSecret = response.ClientSecret;
            #endregion

            _response.Result = cart;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
    }
}

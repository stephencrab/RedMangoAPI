using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using RedMangoAPI.Models.Dto;
using RedMangoAPI.Utility;
using System.Net;
using System.Text.Json;

namespace RedMangoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private ApiResponse _response;
        private readonly IMapper _mapper;
        public OrderController(ApplicationDbContext dbContext, IMapper mapper)
        {
            this._dbContext = dbContext;
            this._response = new ApiResponse();
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetOrders(string? userId, string searchString,
            string status, int pageNumber = 1, int pageSize = 5)
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders = _dbContext.OrderHeaders.Include(o => o.OrderDetails)
                    .ThenInclude(o => o.MenuItem)
                    .OrderByDescending(o => o.OrderHeaderId);

                if (!string.IsNullOrEmpty(userId))
                {
                    orderHeaders = orderHeaders.Where(o => o.ApplicationUserId == userId);
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    orderHeaders = orderHeaders.Where(o => o.PickupName.ToUpper().Contains(searchString.ToUpper()) ||
                    o.PickupEmail.ToUpper().Contains(searchString.ToUpper()) ||
                    o.PickupPhoneNumber.ToUpper().Contains(searchString.ToUpper()));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    orderHeaders = orderHeaders.Where(o => o.Status.ToUpper() == status.ToUpper());
                }

                Pagination pagination = new Pagination
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = orderHeaders.Count(),
                };

                Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagination));

                _response.Result = orderHeaders.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
            }

            return _response;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse>> GetOrders(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var orderHeaders = _dbContext.OrderHeaders.Include(o => o.OrderDetails)
                    .ThenInclude(o => o.MenuItem)
                    .Where(o => o.OrderHeaderId == id);

                if (orderHeaders.Count() == 0)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = orderHeaders;
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
            }

            return _response;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateOrder([FromBody] OrderHeaderCreateDTO orderHeaderDTO)
        {
            try
            {
                OrderHeader order = new OrderHeader()
                {
                    ApplicationUserId = orderHeaderDTO.ApplicationUserId,
                    PickupName = orderHeaderDTO.PickupName,
                    PickupEmail = orderHeaderDTO.PickupEmail,
                    PickupPhoneNumber = orderHeaderDTO.PickupPhoneNumber,
                    OrderTotal = orderHeaderDTO.OrderTotal,
                    OrderDate = DateTime.Now,
                    StripePaymentIntentID = orderHeaderDTO.StripePaymentIntentID,
                    TotalItems = orderHeaderDTO.TotalItems,
                    Status = string.IsNullOrEmpty(orderHeaderDTO.Status) ? SD.status_pending : orderHeaderDTO.Status,
                };

                if (ModelState.IsValid)
                {
                    _dbContext.OrderHeaders.Add(order);
                    _dbContext.SaveChanges();
                    foreach (var orderDetailDTO in orderHeaderDTO.OrderDetailsDTO)
                    {
                        OrderDetails orderDetails = new OrderDetails()
                        {
                            OrderHeaderId = order.OrderHeaderId,
                            ItemName = orderDetailDTO.ItemName,
                            MenuItemId = orderDetailDTO.MenuItemId,
                            Price = orderDetailDTO.Price,
                            Quantity = orderDetailDTO.Quantity,
                        };
                        _dbContext.OrderDetails.Add(orderDetails);
                    }
                    _dbContext.SaveChanges();

                    _response.StatusCode = HttpStatusCode.Created;
                    _response.Result = order;
                    order.OrderDetails = null;
                    return Ok(_response);
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
            }

            return _response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderHeader(int id, [FromBody] OrderHeaderUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.OrderHeaderId)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }

                var orderFromDb = _dbContext.OrderHeaders.FirstOrDefault(o => o.OrderHeaderId == id);

                if (orderFromDb == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }

                var updateOrderHeader = this._mapper.Map(updateDTO, orderFromDb);
                _dbContext.SaveChanges();
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.Message);
            }

            return _response;
        }
    }
}

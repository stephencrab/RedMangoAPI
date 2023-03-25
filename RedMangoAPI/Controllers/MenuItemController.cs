using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMangoAPI.Data;
using RedMangoAPI.Models;
using RedMangoAPI.Models.Dto;
using RedMangoAPI.Services;
using RedMangoAPI.Utility;
using System.Net;

namespace RedMangoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private ApiResponse _response;
        private readonly IBlobService _blobService;
        public MenuItemController(ApplicationDbContext dbContext, IBlobService blobService)
        {
            this._dbContext = dbContext;
            this._response = new ApiResponse();
            this._blobService = blobService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            _response.Result = _dbContext.MenuItems;
            _response.StatusCode = HttpStatusCode.OK;

            return Ok(_response);
        }

        [HttpGet("{Id:int}", Name = "GetMenuItem")]
        public async Task<IActionResult> GetMenuItem(int Id)
        {
            if (Id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            MenuItem menuItem = _dbContext.MenuItems.FirstOrDefault(m => m.Id == Id);

            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }

            _response.Result = menuItem;
            _response.StatusCode = HttpStatusCode.OK;

            return Ok(_response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO menuItemCreate)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemCreate.File == null || menuItemCreate.File.Length == 0)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemCreate.File.FileName)}";
                    MenuItem menuItem = new MenuItem()
                    {
                        Name = menuItemCreate.Name,
                        Price = menuItemCreate.Price,
                        Category = menuItemCreate.Category,
                        SpecialTag = menuItemCreate.SpecialTag,
                        Description = menuItemCreate.Description,
                        Image = await _blobService.UploadBlob(fileName, SD.Storage_Container, menuItemCreate.File)
                    };
                    _dbContext.MenuItems.Add(menuItem);
                    _dbContext.SaveChanges();
                    _response.Result = menuItem;
                    _response.StatusCode = HttpStatusCode.Created;

                    return CreatedAtRoute("GetMenuItem", new { id = menuItem.Id }, _response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.ToString());
            }

            return _response;
        }

        [HttpPut("{Id:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int Id, [FromForm] MenuItemUpdateDTO menuItemUpdate)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemUpdate == null || Id != menuItemUpdate.Id)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }

                    MenuItem menuItemFromDb = await _dbContext.MenuItems.FirstOrDefaultAsync(m => m.Id == Id);

                    if (menuItemFromDb == null)
                    {
                        return BadRequest();
                    }

                    menuItemFromDb.Name = menuItemUpdate.Name;
                    menuItemFromDb.Price = menuItemUpdate.Price;
                    menuItemFromDb.Category = menuItemUpdate.Category;
                    menuItemFromDb.SpecialTag = menuItemUpdate.SpecialTag;
                    menuItemFromDb.Description = menuItemUpdate.Description;

                    if (menuItemUpdate.File != null && menuItemUpdate.File.Length > 0)
                    {
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemUpdate.File.FileName)}";
                        await _blobService.DeleteBlob(menuItemFromDb.Image.Split('/').Last(), SD.Storage_Container);
                        menuItemFromDb.Image = await _blobService.UploadBlob(fileName, SD.Storage_Container, menuItemUpdate.File);
                    }

                    _dbContext.MenuItems.Update(menuItemFromDb);
                    _dbContext.SaveChanges();
                    _response.StatusCode = HttpStatusCode.NoContent;

                    return Ok(_response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.ToString());
            }

            return _response;
        }

        [HttpDelete("{Id}")]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int Id)
        {
            try
            {
                if (Id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }

                MenuItem menuItemFromDb = await _dbContext.MenuItems.FirstOrDefaultAsync(m => m.Id == Id);

                if (menuItemFromDb == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    return NotFound();
                }

                await _blobService.DeleteBlob(menuItemFromDb.Image.Split('/').Last(), SD.Storage_Container);
                _dbContext.MenuItems.Remove(menuItemFromDb);
                _dbContext.SaveChanges();
                _response.StatusCode = HttpStatusCode.NoContent;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages.Add(ex.ToString());
            }

            return _response;
        }
    }
}

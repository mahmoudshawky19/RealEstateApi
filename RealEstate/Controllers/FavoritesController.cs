using Application.DTOs.Favorite;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RealEstate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService favoriteService;
        public FavoritesController(IFavoriteService favoriteService)
        {
            this.favoriteService = favoriteService;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriteResponseDto>>> GetAllFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();
            var favorites = await favoriteService.GetAllFavoritesAsync(userId);
            return Ok(favorites);
        }
        [HttpPost("{itemId}")]
        public async Task<ActionResult<FavoriteResponseDto>> AddToFavorites(int itemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();
            var favorite = await favoriteService.AddToFavoritesAsync(userId, itemId);
            return Ok(favorite);
        }
        [HttpDelete("{itemId}")]
        public async Task<ActionResult<bool>> RemoveFromFavorites(int itemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();
            var result = await favoriteService.RemoveFromFavoritesAsync(userId, itemId);
            if(!result)
            {
                return NotFound();
            }
            return Ok(result);
        }
        [HttpGet("isFavorite/{itemId}")]
        public async Task<ActionResult<bool>> IsFavorite(int itemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            var isFavorite = await favoriteService.IsFavoriteAsync(userId, itemId);
            return Ok(isFavorite);
        }

    }
}

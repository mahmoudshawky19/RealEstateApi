using Application.DTOs.Favorite;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApplicationDbContext context;
        public FavoriteService(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<FavoriteResponseDto>> GetAllFavoritesAsync(string userId)
        {
            var favorites = await context.Favorites.Where(f => f.UserId == userId).Include(f => f.Property).ThenInclude(f => f.Images).ToListAsync();
            return favorites.Select(f => new FavoriteResponseDto
            {
                PropertyId = f.PropertyId,
                CreatedAt = f.CreatedAt,
                Price = f.Property.Price,
                PrimaryImageUrl = f.Property.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                PropertyTitle = f.Property.Title
            });
        }
        public async Task<FavoriteResponseDto> AddToFavoritesAsync(string userId, int propertyId)
        {
            var property = await context.Properties
    .Include(p => p.Images)
    .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property == null)
                throw new KeyNotFoundException("Property not found.");
            var existingFavorite = await context.Favorites
        .AnyAsync(f => f.UserId == userId && f.PropertyId == propertyId);

            if (existingFavorite)
                throw new InvalidOperationException("Property is already in favorites.");
            var favorite = new Domain.Entities.Favorite
            {
                Property = property,
                UserId = userId,
                PropertyId = propertyId,
                CreatedAt = DateTime.UtcNow
            };
            context.Favorites.Add(favorite);
            await context.SaveChangesAsync();
            return new FavoriteResponseDto
            {
                PropertyId = favorite.PropertyId,
                CreatedAt = favorite.CreatedAt,
                Price = favorite.Property.Price,
                PrimaryImageUrl = favorite.Property.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
                PropertyTitle = favorite.Property.Title
            };
        }
        public async Task<bool> RemoveFromFavoritesAsync(string userId, int propertyId)
        {
            var favorite = await context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);
            if (favorite == null)
            {
                return false;
            }
            context.Favorites.Remove(favorite);
            await context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsFavoriteAsync(string userId, int propertyId)
        {
            return await context.Favorites.AnyAsync(f => f.UserId == userId && f.PropertyId == propertyId);
        }

    }
}

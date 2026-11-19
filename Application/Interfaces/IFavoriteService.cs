using Application.DTOs.Favorite;

namespace Application.Interfaces;

public interface IFavoriteService
{
    Task<IEnumerable<FavoriteResponseDto>> GetAllFavoritesAsync(string userId);

    Task<bool> IsFavoriteAsync(string userId, int propertyId);

    Task<FavoriteResponseDto> AddToFavoritesAsync(string userId, int propertyId);

    Task<bool> RemoveFromFavoritesAsync(string userId, int propertyId);
}
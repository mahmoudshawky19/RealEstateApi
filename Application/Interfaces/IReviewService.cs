using Application.DTOs.Review;

namespace Application.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewResponseDto> CreateReviewAsync(CreateReviewDto createReviewDto, string userId);
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByPropertyIdAsync(int propertyId);
        Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, UpdateReviewDto updateReviewDto, string userId);
        Task<bool> DeleteReviewAsync(int reviewId, string userId);
        Task<decimal> GetAverageRatingByPropertyIdAsync(int propertyId);

    }
}

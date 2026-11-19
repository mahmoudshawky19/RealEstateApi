using Application.DTOs.Review;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        public ReviewService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }
        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByPropertyIdAsync(int propertyId)
        {
            var reviews = await _context.Reviews.AsNoTracking()
                .Where(r => r.PropertyId == propertyId)
                .ToListAsync();

            return reviews.Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                PropertyId = r.PropertyId,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt,
                ReviewerName = _context.Users
    .Where(u => u.Id == r.UserId)
    .Select(u => u.FullName)
    .FirstOrDefault() ?? string.Empty
            });
        }
        public async Task<ReviewResponseDto> CreateReviewAsync(CreateReviewDto createReviewDto, string userId)
        {
            var property = await _context.Properties.FindAsync(createReviewDto.PropertyId);
            if(property == null)
            {
                throw new ArgumentException("Property not found.");
            }
            if(property.Status != PropertyStatus.Approved)
            {
                throw new InvalidOperationException("Cannot review a property that is not approved.");
            }
            if(property.UserId == userId)
            {
                throw new InvalidOperationException("You cannot review your own property.");
            }
            var alreadyReviewed = await _context.Reviews.AnyAsync(r =>
       r.PropertyId == createReviewDto.PropertyId &&
       r.UserId == userId);

            if (alreadyReviewed)
                throw new InvalidOperationException("You have already reviewed this property.");

            var review = new Review
            {
                UserId = userId,
                PropertyId = createReviewDto.PropertyId,
                Rating = createReviewDto.Rating,
                Comment = createReviewDto.Comment,
                CreatedAt = DateTime.UtcNow
            };
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

             await _notificationService.CreateNotificationAsync(
    property.UserId,
    "New Review",
    $"Someone reviewed your property: {property.Title}. Rating: {review.Rating}/5"
);
            return new ReviewResponseDto
            {
                Id = review.Id,
                PropertyId = review.PropertyId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                ReviewerName = userId // Assuming you have a way to get the user's name from their ID
            };
        }
        public async Task<ReviewResponseDto> UpdateReviewAsync(int reviewId, UpdateReviewDto updateReviewDto, string userId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);
            if (review == null)
            {
                throw new Exception("Review not found or you do not have permission to update this review.");
            }
            review.Rating = updateReviewDto.Rating;
            review.Comment = updateReviewDto.Comment;
            review.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return new ReviewResponseDto
            {
                Id = review.Id,
                PropertyId = review.PropertyId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                ReviewerName = userId // Assuming you have a way to get the user's name from their ID
            };
        }
        public async Task<bool> DeleteReviewAsync(int reviewId, string userId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);
            if (review == null)
            {
                throw new Exception("Review not found or you do not have permission to delete this review.");
            }
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<decimal> GetAverageRatingByPropertyIdAsync(int propertyId)
        {
            var averageRating = await _context.Reviews
                .Where(r => r.PropertyId == propertyId)
                .Select(r => (decimal?)r.Rating)
                .AverageAsync();

            return Math.Round(averageRating ?? 0m, 2);
        }
    }
}

using Application.DTOs.Review;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RealEstate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService reviewService;
        public ReviewsController(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }
        [HttpGet("property/{propertyId}")]
        public async Task<IActionResult> GetReviewsByPropertyId(int propertyId)
        {
            var reviews = await reviewService.GetReviewsByPropertyIdAsync(propertyId);
            return Ok(reviews);
        }
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto createReviewDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var review = await reviewService.CreateReviewAsync(createReviewDto, userId);
            return CreatedAtAction(nameof(GetReviewsByPropertyId), new { propertyId = review.PropertyId }, review);
        }
        [HttpPut("{reviewId}")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDto updateReviewDto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var review = await reviewService.UpdateReviewAsync(reviewId, updateReviewDto, userId);
            return Ok(review);
        }
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            await reviewService.DeleteReviewAsync(reviewId, userId);
            return NoContent();
        }
        [HttpGet("property/{propertyId}/average-rating")]
        public async Task<IActionResult> GetAverageRatingByPropertyId(int propertyId)
        {
            var averageRating = await reviewService.GetAverageRatingByPropertyIdAsync(propertyId);
            return Ok(averageRating);
        }
    }
}

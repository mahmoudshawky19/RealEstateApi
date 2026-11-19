using Application.DTOs.Inquiry;
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
    public class InquiriesController : ControllerBase
    {
        private readonly IInquiryService inquiryService;
        public InquiriesController(IInquiryService inquiryService)
        {
            this.inquiryService = inquiryService;
        }
        [HttpGet]
        public async Task<IActionResult> GetSentInquiries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();
            var inquiries = await inquiryService.GetSentInquiriesAsync(userId);
            return Ok(inquiries);
        }
        [HttpGet("received")]
        public async Task<IActionResult> GetReceivedInquiries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();
            var inquiries = await inquiryService.GetReceivedInquiriesAsync(userId);
            return Ok(inquiries);
        }
        [HttpGet("{inquiryId}")]
        public async Task<IActionResult> GetInquiryById(int inquiryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();
            var inquiry = await inquiryService.GetInquiryByIdAsync( inquiryId , userId);
            if (inquiry == null)
            {
                return NotFound();
            }
            return Ok(inquiry);
        }
        [HttpPost]
        public async Task<IActionResult> CreateInquiry([FromBody] CreateInquiryDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var inquiry = await inquiryService.CreateInquiryAsync(request, userId);
            return CreatedAtAction(
                nameof(GetInquiryById),
                new { inquiryId = inquiry.Id },
                inquiry
            );
        }
        [HttpPut("{inquiryId}/status")]
        public async Task<IActionResult> UpdateInquiryStatus(int inquiryId, [FromBody] UpdateInquiryStatusDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

           var inquiry = await inquiryService.UpdateInquiryStatusAsync(inquiryId, request, userId );
            if(inquiry == null)
            {
                return NotFound();
            }   
            return Ok(inquiry);
        }

    }
}

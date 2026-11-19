
using Application.DTOs.Inquiry;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class InquiryService : IInquiryService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    public InquiryService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    // Get inquiries sent by the current user
    public async Task<IEnumerable<InquiryResponseDto>> GetSentInquiriesAsync(string userId)
    {
        return await _context.Inquiries
            .Where(i => i.UserId == userId)
            .Select(i => new InquiryResponseDto
            {
                Id = i.Id,
                Message = i.Message,
                Status = i.Status,
                UserId = i.UserId,
                PropertyId = i.PropertyId,
                PropertyTitle = i.Property.Title,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();
    }

    // Get inquiries received by the current property owner
    public async Task<IEnumerable<InquiryResponseDto>> GetReceivedInquiriesAsync(string userId)
    {
        return await _context.Inquiries
            .Where(i => i.Property.UserId == userId)
            .Select(i => new InquiryResponseDto
            {
                Id = i.Id,
                Message = i.Message,
                Status = i.Status,
                UserId = i.UserId,
                PropertyId = i.PropertyId,
                PropertyTitle = i.Property.Title,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .ToListAsync();
    }

    // Get inquiry details if the user is sender or property owner
    public async Task<InquiryResponseDto?> GetInquiryByIdAsync(int inquiryId, string userId)
    {
        return await _context.Inquiries
            .Where(i => i.Id == inquiryId &&
                       (i.UserId == userId || i.Property.UserId == userId))
            .Select(i => new InquiryResponseDto
            {
                Id = i.Id,
                Message = i.Message,
                Status = i.Status,
                UserId = i.UserId,
                PropertyId = i.PropertyId,
                PropertyTitle = i.Property.Title,
                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    // Create a new inquiry
    public async Task<InquiryResponseDto> CreateInquiryAsync(
        CreateInquiryDto createInquiryDto,
        string userId)
    {
        var property = await _context.Properties
            .FirstOrDefaultAsync(p => p.Id == createInquiryDto.PropertyId);

        if (property == null)
            throw new KeyNotFoundException("Property not found.");

        if (property.UserId == userId)
            throw new InvalidOperationException(
                "You cannot send an inquiry about your own property.");

        var inquiry = new Inquiry
        {
            Message = createInquiryDto.Message,
            Status = InquiryStatus.New,
            PropertyId = createInquiryDto.PropertyId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Inquiries.Add(inquiry);

        await _context.SaveChangesAsync();

        // Create a notification for the property owner
        await _notificationService.CreateNotificationAsync(
            property.UserId,
            "New Inquiry",
            $"You have a new inquiry for your property: {property.Title}"
        );

        return new InquiryResponseDto
        {
            Id = inquiry.Id,
            Message = inquiry.Message,
            Status = inquiry.Status,
            UserId = inquiry.UserId,
            PropertyId = inquiry.PropertyId,
            PropertyTitle = property.Title,
            CreatedAt = inquiry.CreatedAt,
            UpdatedAt = inquiry.UpdatedAt
        };
    }

    // Update inquiry status (property owner only)
    public async Task<InquiryResponseDto?> UpdateInquiryStatusAsync(
        int inquiryId,
        UpdateInquiryStatusDto updateInquiryStatusDto,
        string userId)
    {
        var inquiry = await _context.Inquiries
            .Include(i => i.Property)
            .FirstOrDefaultAsync(i => i.Id == inquiryId);

        if (inquiry == null)
            return null;

        if (inquiry.Property.UserId != userId)
            throw new UnauthorizedAccessException(
                "You are not allowed to update this inquiry.");

        if (!Enum.IsDefined(updateInquiryStatusDto.Status))
            throw new ArgumentException("Invalid inquiry status.");

        inquiry.Status = updateInquiryStatusDto.Status;
        inquiry.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new InquiryResponseDto
        {
            Id = inquiry.Id,
            Message = inquiry.Message,
            Status = inquiry.Status,
            UserId = inquiry.UserId,
            PropertyId = inquiry.PropertyId,
            PropertyTitle = inquiry.Property.Title,
            CreatedAt = inquiry.CreatedAt,
            UpdatedAt = inquiry.UpdatedAt
        };
    }
}

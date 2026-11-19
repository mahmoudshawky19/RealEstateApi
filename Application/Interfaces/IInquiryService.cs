
using Application.DTOs.Inquiry;

namespace Application.Interfaces;

public interface IInquiryService
{
    Task<IEnumerable<InquiryResponseDto>> GetSentInquiriesAsync(string userId);

    Task<IEnumerable<InquiryResponseDto>> GetReceivedInquiriesAsync(string userId);

    Task<InquiryResponseDto?> GetInquiryByIdAsync(int inquiryId, string userId);

    Task<InquiryResponseDto> CreateInquiryAsync(
        CreateInquiryDto createInquiryDto,
        string userId);

    Task<InquiryResponseDto?> UpdateInquiryStatusAsync(
        int inquiryId,
        UpdateInquiryStatusDto updateInquiryStatusDto,
        string userId);
}

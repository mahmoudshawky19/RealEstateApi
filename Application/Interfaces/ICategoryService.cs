using Application.DTOs.Category;
using Application.DTOs.Property;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<CategoryResponseDto> GetCategoryByIdAsync(int categoryId);
        Task<CategoryResponseDto> CreateAsync(
                CreateCategoryDto request);

        Task<bool> UpdateAsync(
            int id,
            UpdateCategoryDto request);

        Task<bool> DeleteAsync(
            int id);

    }
}

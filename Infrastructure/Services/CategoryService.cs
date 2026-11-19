using Application.DTOs.Category;
using Application.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext context;
        public CategoryService(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await context.Categories.ToListAsync();
            return categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CreatedAt = c.CreatedAt
            });
        }
        public async Task<CategoryResponseDto> GetCategoryByIdAsync(int categoryId)
        {
            var category = await context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return null;
            }
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt
            };
        }
        public async Task<CategoryResponseDto> CreateAsync(CreateCategoryDto request )
        {
            var category = new Domain.Entities.Category
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CreatedAt = category.CreatedAt
            };
        }
        public async Task<bool> UpdateAsync(int id, UpdateCategoryDto request )
        {
            var category = await context.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }
            category.Name = request.Name;
            category.Description = request.Description;
            await context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteAsync(int id )
        {
            var category = await context.Categories.FindAsync(id);
            if (category == null)
            {
                return false;
            }
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return true;
        }

    }
}
 
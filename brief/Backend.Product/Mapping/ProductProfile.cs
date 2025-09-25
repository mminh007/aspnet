using AutoMapper;
using Backend.Product.Models.Entities;
using Backend.Product.Models.DTOs;

namespace Backend.Product.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            // Create mapping
            CreateMap<ProductDTOModel, ProductModel>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Category.StoreId, opt => opt.MapFrom(src => src.Category.StoreId))
                .ForMember(dest => dest.Category.CategoryId, opt => opt.MapFrom(src => src.Category.CategoryId));
                
            // Update mapping 
            CreateMap<UpdateProductModel, ProductModel>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            
            CreateMap<ProductModel, ProductSellerDTO>();

            
            CreateMap<ProductModel, ProductBuyerDTO>();

            
            CreateMap<CategoryModel, CategoryDTO>().ReverseMap();
        }
    }
}

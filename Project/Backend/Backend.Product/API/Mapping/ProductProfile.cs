using AutoMapper;
using Common.Models;
using Common.Models.Requests;
using DAL.Models.Entities;
using static Common.Models.DTOs;

namespace API.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            // Create mapping
            CreateMap<DTOs.ProductDTO, ProductModel>()
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

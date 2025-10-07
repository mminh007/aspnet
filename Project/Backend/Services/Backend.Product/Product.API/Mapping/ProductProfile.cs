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
                .ForMember(dest => dest.StoreId, opt => opt.MapFrom(src => src.StoreId))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.Category, opt => opt.Ignore()); // quan trọng

            // Update mapping 
            CreateMap<UpdateProductModel, ProductModel>()
                    .ForMember(dest => dest.ProductImage, opt => opt.Ignore()) 
                    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


            CreateMap<ProductModel, ProductSellerDTO>();


            CreateMap<ProductModel, ProductBuyerDTO>();


            CreateMap<CategoryModel, CategoryDTO>().ReverseMap();

            CreateMap<ProductModel, OrderProductDTO>();
        }
    }
}

using AutoMapper;
using Store.Common.Models.Requests;
using Store.Common.Models.Responses;
using Store.DAL.Models.Entities;


namespace API.Mapping
{
    public class StoreProfile : Profile
    {
        public StoreProfile()
        {
            // Create mapping
            //CreateMap<DTOs.ProductDTO, ProductModel>()
            //    .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            //    .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            //    .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            //    .ForMember(dest => dest.StoreId, opt => opt.MapFrom(src => src.StoreId))
            //    .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            //    .ForMember(dest => dest.Category, opt => opt.Ignore()); // quan trọng

            //// Update mapping 
            //CreateMap<UpdateProductModel, ProductModel>()
            //        .ForMember(dest => dest.ProductImage, opt => opt.Ignore()) 
            //        .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<StoreModel, StoreDTO>().ReverseMap();

            CreateMap<UpdateStoreModel, StoreModel>()
            .ForMember(dest => dest.StoreId, opt => opt.Ignore())
            .ForMember(dest => dest.StoreCategorySlug, opt => opt.Ignore()) // Không map slug
            .ForMember(dest => dest.StoreImage, opt => opt.Ignore()) // Xử lý thủ công trong service
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null)); // ✅ chỉ map khi != null


        }
    }
}

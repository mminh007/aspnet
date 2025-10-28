using AutoMapper;
using static Order.Common.Models.DTOs;
using Order.DAL.Models.Entities;
using Order.Common.Models.Requests;

namespace Order.API.Mapping
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            // Cart
            CreateMap<CartDTO, CartModel>().ReverseMap();
            CreateMap<CartItemDTO, CartItemModel>()
                                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                                .ReverseMap();
            CreateMap<RequestItemToCartModel, CartItemModel>().ReverseMap();

            // Order
            CreateMap<OrderDTO, OrderModel>().ReverseMap();
            CreateMap<OrderItemDTO, OrderItemModel>().ReverseMap();
            CreateMap<UpdateOrderRequest, OrderModel>().ReverseMap();

            CreateMap<ShippingModel, ShipDTO>();

            // Order
            CreateMap<OrderModel, OrderDTO>()
                .ForMember(d => d.ShippingInfo, o => o.MapFrom(s => s.Shipping));  // <<< map khác tên
            CreateMap<OrderDTO, OrderModel>()
                .ForMember(d => d.Shipping, o => o.Ignore());
        }
    }
}
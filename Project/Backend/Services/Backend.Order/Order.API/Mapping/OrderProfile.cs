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
        }
    }
}
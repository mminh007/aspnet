using AutoMapper;
using Payment.Common.Models.Requests;
using Payment.DAL.Models.Entities;
using static Payment.Common.Models.DTOs;

namespace Payment.API.Mapping
{
    public class PaymentProfile : Profile
    {
        public PaymentProfile()
        {
            // Create mapping CreatePaymentRequestDTO -> PaymentModel
            CreateMap<PaymentRequest, PaymentModel>()
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore()) // auto-gen
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore()) // Default: Pending
                .ForMember(dest => dest.TransactionId, opt => opt.Ignore())
                .ForMember(dest => dest.GatewayResponse, opt => opt.Ignore())
                .ForMember(dest => dest.FailureReason, opt => opt.Ignore());

            // ConfirmPaymentRequestDTO mapping TransactionId info
            CreateMap<ConfirmPaymentRequest, PaymentModel>()
                .ForMember(dest => dest.TransactionId, opt => opt.MapFrom(src => src.PaymentIntentId))
                .ForAllMembers(opt => opt.Ignore());

            // Mapping entity -> DTO
            CreateMap<PaymentModel, PaymentDTO>().ReverseMap();
                
        }
    }
}

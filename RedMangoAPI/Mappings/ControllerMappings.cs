using AutoMapper;
using RedMangoAPI.Models.Dto;
using RedMangoAPI.Models;

namespace RedMangoAPI.Mappings
{
    public class ControllerMappings : Profile
    {
        public ControllerMappings()
        {
            this.CreateMap<OrderHeaderUpdateDTO, OrderHeader>()
                .ForMember(dest => dest.PickupName, opt => opt.Condition(src => src.PickupName != null))
                .ForMember(dest => dest.PickupPhoneNumber, opt => opt.Condition(src => src.PickupPhoneNumber != null))
                .ForMember(dest => dest.PickupEmail, opt => opt.Condition(src => src.PickupEmail != null))
                .ForMember(dest => dest.StripePaymentIntentID, opt => opt.Condition(src => src.StripePaymentIntentID != null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        }
    }
}

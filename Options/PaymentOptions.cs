using System;
using System.ComponentModel.DataAnnotations; // required and range attributes

namespace TmsApi
{
    public class PaymentOptions
    {
        // gateway url
        [Required(ErrorMessage = "The GatewayUrl field is required.")]
        public required string GatewayUrl { get; init; }

        // highest deposit amount in birr
        [Range(100, 100000, ErrorMessage = "MaxDepositBirr must be between 100 and 100,000 Birr.")]
        public decimal MaxDepositBirr { get; init; }
    }
}
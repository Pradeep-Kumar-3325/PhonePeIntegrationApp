namespace PhonePeIntegrationApp.Models
{
    public class OrderDetailModel
    {
        public string OrderId { get; set; }

        public int UserId { get; set; }

        public string CustomerName { get; set; }

        public int OrderAmount { get; set; }

        public string? MobileNumber { get; set; }

        public string? Email { get; set; }
    }
}

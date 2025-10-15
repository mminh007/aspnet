namespace Frontend.Helpers
{
    public static class OrderStatusExtensions
    {
        public static string ToVietnamese(this string status) => status switch
        {
            "Pending" => "Chờ Thanh Toán",
            "Processing" => "Đang Thanh Toán",
            "Paid" => "Đã Thanh Toán",
            "Completed" => "Hoàn Thành",
            "Failed" => "Thất Bại",
            "Cancelled" => "Đã Hủy",
            "Refunded" => "Hoàn Tiền",
            _ => status
        };
    }
}

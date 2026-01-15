using System.Text.Json;
using Microsoft.AspNetCore.Http;
using WebApplication1.VMs;

namespace WebApplication1.Helpers
{
    public static class CartBadge
    {
        public static int GetBorrowCartQty(HttpContext ctx)
        {
            var json = ctx.Session.GetString("BORROW_CART_V1");
            if (string.IsNullOrWhiteSpace(json)) return 0;

            try
            {
                var items = JsonSerializer.Deserialize<List<BorrowCartItemDto>>(json) ?? new();
                return items.Sum(x => x.Qty);
            }
            catch
            {
                return 0;
            }
        }
    }
}

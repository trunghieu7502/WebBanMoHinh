using DoAnCoSo_WebBanMoHinh.Models;

namespace DoAnCoSo_WebBanMoHinh.Models.Services
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(HttpContext context, VNPayRequestModel model);
        VNPayResponseModel PaymentExecute(IQueryCollection collections);
    }
}

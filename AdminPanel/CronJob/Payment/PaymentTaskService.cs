using Iyzipay.Request;
using NPOI.SS.Formula.Functions;
using Quartz;
using Service.Implementations.User;
using System.Globalization;
using System.Xml;

namespace AdminPanel.CronJob.Payment
{
    public class PaymentTaskService : IJob
    {
        private readonly ILogger<PaymentTaskService> _logger;
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        public PaymentTaskService(ILogger<PaymentTaskService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _userService = new UserService();
            _configuration = configuration;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await RunJob();
        }

        private async Task RunJob()
        {
            var plans = _userService.GetExpiredOrDuePlansNotMatchedWithPayments();
            if (plans != null)
            {
                foreach (var item in plans)
                {
                    var organization = _userService.GetOrganizationById(item.OrganizationId);
                    var priceUSD = _userService.GetSubscription(item.PlanId, item.IsYearly);
                    var card = _userService.GetCard(item.OrganizationId);
                    var adminUser = _userService.GetPaymentUser(item.OrganizationId);

                    var _iyzicoOptions = new Iyzipay.Options
                    {
                        ApiKey = _configuration["Iyzico:ApiKey"],
                        SecretKey = _configuration["Iyzico:SecretKey"],
                        BaseUrl = _configuration["Iyzico:BaseUrl"],
                    };

                    var priceInTRY = CalculatePriceInTRY(Convert.ToDecimal(priceUSD.Price));

                    var paymentRequest = new CreatePaymentRequest
                    {
                        Locale = Iyzipay.Model.Locale.TR.ToString(),
                        ConversationId = Guid.NewGuid().ToString(),
                        Price = priceInTRY.ToString("F2", CultureInfo.InvariantCulture),
                        PaidPrice = priceInTRY.ToString("F2", CultureInfo.InvariantCulture),
                        Currency = Iyzipay.Model.Currency.TRY.ToString(),
                        Installment = 1,
                        BasketId = "basket-001",
                        PaymentChannel = Iyzipay.Model.PaymentChannel.WEB.ToString(),
                        PaymentGroup = Iyzipay.Model.PaymentGroup.PRODUCT.ToString(),

                        PaymentCard = new Iyzipay.Model.PaymentCard
                        {
                            CardToken = card.CardToken,
                            CardUserKey = card.CardUserKey
                        },

                        Buyer = new Iyzipay.Model.Buyer
                        {
                            Id = adminUser.Id.ToString(),
                            Name = organization.Name,
                            Surname = "-",
                            GsmNumber = organization.Phone,
                            Email = adminUser.Mail,
                            IdentityNumber = organization.TaskNumber,
                            RegistrationAddress = organization.Address,
                            Ip = "78.179.30.10",
                            City = organization.Address,
                            Country = "Turkey"
                        },

                        BillingAddress = new Iyzipay.Model.Address
                        {
                            ContactName = organization.Name,
                            City = organization.Address,
                            Country = "Turkey",
                            Description = "Fatura Adresi",
                            ZipCode = organization.ZipCode
                        },

                        ShippingAddress = new Iyzipay.Model.Address
                        {
                            ContactName = organization.Name,
                            City = organization.Address,
                            Country = "Turkey",
                            Description = "Teslimat Adresi",
                            ZipCode = organization.ZipCode
                        },

                        BasketItems = new List<Iyzipay.Model.BasketItem>
                        {
                            new Iyzipay.Model.BasketItem
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Abonelik Paketi",
                                Category1 = "Abonelik",
                                ItemType = Iyzipay.Model.BasketItemType.VIRTUAL.ToString(),
                                Price = priceInTRY.ToString("F2", CultureInfo.InvariantCulture)
                            }
                        }
                    };

                    var payment = await Iyzipay.Model.Payment.Create(paymentRequest, _iyzicoOptions);

                    if (payment.Status == "success")
                    {
                        var deleteFail = _userService.DeletePaymentFail(item.OrganizationId);
                        var nextAmount = _userService.UpdateCronJobNextPaymentDatePlan(item.Id, item.IsYearly, item.NextPaymentDate);
                        var proccessSuccess = _userService.AddPaymentSuccess(item.OrganizationId, item.NextPaymentDate);
                        var isPayment = _userService.UpdateIsPaymentSuccessPlan(item.OrganizationId);
                    }
                    else
                    {
                        var deleteFail = _userService.DeletePaymentFail(item.OrganizationId);
                        var proccessFail = _userService.AddPaymentFail(item.OrganizationId, payment.ErrorMessage);
                        var isPayment = _userService.UpdateIsPaymentFailPlan(item.OrganizationId);
                    }
                }
            }
            return;
        }

        private decimal GetCurrentUsdTryRate()
        {
            try
            {
                var xmlUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlUrl);

                var usdNode = xmlDoc.SelectSingleNode("//Currency[@CurrencyCode='USD']");

                var rateString = usdNode.SelectSingleNode("BanknoteSelling")?.InnerText;

                if (decimal.TryParse(rateString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal rate))
                {
                    return rate;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kur bilgisi alınamadı: " + ex.Message);
            }

            return 0m;
        }

        private decimal CalculatePriceInTRY(decimal priceInUSD)
        {
            var rate = GetCurrentUsdTryRate();
            return Math.Round(priceInUSD * rate, 2);
        }
    }
}

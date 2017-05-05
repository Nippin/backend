using Backend;
using Nancy;

namespace Endpoint
{
    public sealed class ExcelModule : NancyModule
    {
        public ExcelModule()
        {
            Get("/excel/{id}/with/description", async _ =>
            {
                using (var gate = new VatPaymentChecker())
                {
                    await gate.Initialize();
                    var result = await gate.Check(_.id);

                    return result;
                }
            });
        }
    }
}

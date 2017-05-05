using Backend;
using Nancy;

namespace Endpoint
{
    public sealed class VatinModule : NancyModule
    {
        public VatinModule()
        {
            Get("/vatin/{id}", async _ =>
            {
                using (var gate = new VatPaymentChecker())
                {
                    await gate.Initialize();
                    var result = await gate.Check(_.id);

                    return Response.AsJson(new { Response = result });
                }
            });
        }
    }
}

using Akka.Actor;
using Backend;
using Nancy;
using Nancy.Responses;
using OpenQA.Selenium;
using System;
using System.IO;

namespace Endpoint
{
    public sealed class ScreenshotModule : NancyModule
    {
        public ScreenshotModule(IActorRef pageActor)
        {
            Get("/api/screenshot/{id}", async (_) =>
            {
                try
                {
                    Console.WriteLine("Dupa1");
                    var id = (string)_.id;
                    var response = await pageActor.Ask<PageActor.CheckVatinReply>(new PageActor.CheckVatinAsk(id, DateTime.Now));
                    Console.WriteLine("Dupa2");
                    if (!response.Done) return HttpStatusCode.BadGateway;

                    Console.WriteLine("Dupa3");

                    var screenshot = new Screenshot(response.Screenshot);

                    // to convert Selenium screenshot to a file, we need temporarly use file
                    var fileName = Path.GetTempFileName();
                    screenshot.SaveAsFile(fileName, ScreenshotImageFormat.Png);

                    Console.WriteLine("Dupa4");

                    // https://blog.kulman.sk/returning-files-in-nancyfx/
                    var result = new StreamResponse(() => new RemovebleFileStream(fileName), "image/png");

                    Console.WriteLine("Dupa5");

                    var status = response.Status == PageActor.CheckVatinReply.VatinPayerStatus.IsTaxPayer
                        ? "Aktywny"
                        : "Inny";
                    return result.AsAttachment($"{id} {DateTime.Now:yyyy-MM-dd} {status}.png");
                }
                catch (Exception e)
                {
                    Console.WriteLine("DupaException");
                    Console.WriteLine(e);
                    return HttpStatusCode.Conflict;
                }
            });
        }
    }

    public class RemovebleFileStream : FileStream
    {
        private readonly string fileName;

        public RemovebleFileStream(string fileName)
            : base(fileName, FileMode.Open, FileAccess.Read)
        {
            this.fileName = fileName;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            try
            {
                File.Delete(fileName);
            }
            catch (IOException)
            {

            }
        }
    }
}

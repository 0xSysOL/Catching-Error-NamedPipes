using DataLayer;
using Models;
using Setting___HR_Client___WS_Server;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ErrorLoggingService
{
    public class Worker(ILogger<Worker> logger) : BackgroundService
    {

        Queue<MErrorInformation> QMErrors = new Queue<MErrorInformation>();
        CustomNamedPipeServer costumNamedPipe;

        public override async Task StartAsync(CancellationToken cancellationToken)
        {

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {

            if (QMErrors != null && QMErrors.Count > 0)
                clsLogErrorToDatabase.LogGroup(QMErrors);


            costumNamedPipe?.Disconnect();
            costumNamedPipe?.Dispose();
            await base.StopAsync(cancellationToken);
        }

        public void SaveData()
        {



        }
        public void InitializNamedPipeServer()
        {
            string? NamePipe = clsUtilities.GetNamePipe();
            if (NamePipe == null)
            {
                logger.LogError("Faild To Getting Name of pipe in App.config");
                return;
            }

            costumNamedPipe = new CustomNamedPipeServer(NamePipe, PipeDirection.In);

        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {





            int CountErrorLoop = 0;

            InitializNamedPipeServer();

            while (!stoppingToken.IsCancellationRequested && CountErrorLoop <= 50)
            {

                try
                {

                    if (!costumNamedPipe.IsConnected())
                        await costumNamedPipe.WaitForConnectionAsync(stoppingToken);


                    string? Values = "";

                    Values = await costumNamedPipe.ReadStringAsync();
                    if (!costumNamedPipe.IsConnected())
                        throw new Exception("Value That are want to Read it was null");
                    else if (Values == null)
                        continue;



                    QMErrors.Enqueue(JsonSerializer.Deserialize<MErrorInformation>(Values));


                    if (!clsLogErrorToDatabase.LogGroup(QMErrors))
                        throw new Exception("Value Not Saved In Database");


                    CountErrorLoop = 0;
                }
                catch (Exception e)
                {
                    costumNamedPipe.Dispose();
                    InitializNamedPipeServer();

                    CountErrorLoop++;
                    logger.LogError(e.Message);
                    Console.WriteLine(e.Message);
                }

            }



        }



    }



}


internal class CustomNamedPipeServer
{

    NamedPipeServerStream NPSS;

    public CustomNamedPipeServer(string NamePipe, PipeDirection direction)
    {
        NPSS = new NamedPipeServerStream(NamePipe, direction);
    }

    public bool IsConnected() => NPSS.IsConnected;

    public bool CanRead() => NPSS.CanRead;

    public async Task WaitForConnectionAsync(CancellationToken token) => await NPSS.WaitForConnectionAsync(token);


    public async Task<string?> ReadStringAsync()
    {

        StreamString stream = new StreamString(NPSS);
        if (NPSS.IsConnected)
        {
            string? Value = await stream.ReadString();
            return Value;

        }
        return null;
    }

    public void Disconnect()
    {
        if (NPSS != null && NPSS.IsConnected)
            NPSS.Disconnect();


    }

    public void Dispose()
    {
        if (NPSS != null)
            NPSS.Dispose();

    }

}

internal class StreamString
{
    private Stream ioStream;
    private UnicodeEncoding streamEncoding;

    public StreamString(Stream ioStream)
    {
        this.ioStream = ioStream;
        streamEncoding = new UnicodeEncoding();
    }

    public async Task<string> ReadString()
    {


        int len;
        len = ioStream.ReadByte() * 256;
        len += ioStream.ReadByte();

        if (len < 0)
            return null;

        var inBuffer = new byte[len];
        ioStream?.Read(inBuffer, 0, len);
        return streamEncoding.GetString(inBuffer);




    }


}




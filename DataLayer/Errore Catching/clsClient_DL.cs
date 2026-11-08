using Models;
using Setting___HR_Client___WS_Server;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using static Data_Layer.Utilities_DB;
namespace Data_Layer.Errore_Catching
{




    public static class clsClient_DL
    {
        private static NamedPipeClientStream PipeClientStream = new NamedPipeClientStream(".", GetValuesFromAppConfig.GetValueInTag("NameOfPipe").Item1, PipeDirection.Out, PipeOptions.WriteThrough);
        private static SemaphoreSlim slim = new SemaphoreSlim(1, 1);
        private static TaskProcessor<MErrorInformation> QMError = new TaskProcessor<MErrorInformation>();

        private static bool ConnectToServer()
        {
            if (PipeClientStream == null) return false;


            try
            {
                if (!PipeClientStream.IsConnected)
                    PipeClientStream.Connect(2000);

                return true;
            }
            catch
            {
                return false;

            }

        }
        public static async Task<bool> WriteToPipeStringAsync(MErrorInformation? StrError)
        {
            if (StrError == null) return false;

            try
            {
                StreamString SS = new StreamString(PipeClientStream);
                await SS.WriteStringAsync(JsonSerializer.Serialize(StrError));
            }
            catch
            {
                return false;
            }

            return true;
        }
        public static async Task WriteWithQueue(MErrorInformation StrError)
        {
            if (!await WriteToPipeStringAsync(StrError))
            {
                await QMError.Enqueue(StrError);
                return;
            }



            while (QMError.Count > 0)
            {
                if (!await WriteToPipeStringAsync(QMError.Peek()))
                    break;

                QMError.Dequeue();
            }


        }
        public static async Task Start(MErrorInformation StrError)
        {
            //Interlocked.Increment(ref waitingThreadsCount);


            await slim.WaitAsync();

            try
            {
                if (!ConnectToServer())
                    await QMError.Enqueue(StrError);
                else
                    await WriteWithQueue(StrError);


            }
            finally
            {


                slim.Release();
                //Interlocked.Decrement(ref waitingThreadsCount);

            }



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


        public async Task<int> WriteStringAsync(string outString)
        {


            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            await ioStream.FlushAsync();

            return outBuffer.Length + 2;
        }
    }


}

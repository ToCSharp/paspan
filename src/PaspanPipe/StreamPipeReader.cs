using System.Buffers;
using System.IO.Pipelines;

namespace Paspan;

public class StreamPipeReader
{
    //// buffer, isFinalBlock, return consumed
    //public Func<ReadOnlySequence<byte>, bool, long> ProcessBuffer;
    //bool stop;
    protected PipeWriter? writer;
    protected PipeReader? reader;

    public async Task ParseStream(Stream stream, CancellationToken token = default)
    {
        var pipe = new Pipe();
        writer = pipe.Writer;
        reader = pipe.Reader;
        Task writing = FillPipeAsync(stream, writer, token);
        //Task reading = ReadPipeAsync(pipe.Reader, token);

        await writing;
        //await Task.WhenAll(reading, writing);

    }

    async Task FillPipeAsync(Stream stream, PipeWriter writer, CancellationToken token)
    {
        const int minimumBufferSize = 4096;

        while (true)
        {
            // Allocate at least 4096 bytes from the PipeWriter
            Memory<byte> memory = writer.GetMemory(minimumBufferSize);
            try
            {
                //int bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None);
                int bytesRead = await stream.ReadAsync(memory, token);
                if (bytesRead == 0)
                {
                    break;
                }
                // Tell the PipeWriter how much was read from the Socket
                writer.Advance(bytesRead);
            }
            catch /*(Exception ex)*/
            {
                //LogError(ex);
                break;
            }

            // Make the data available to the PipeReader
            FlushResult result = await writer.FlushAsync();

            if (result.IsCompleted)
            {
                break;
            }
        }

        // Tell the PipeReader that there's no more data coming
        writer.Complete();
    }

    public async Task<(ReadOnlySequence<byte> Buffer, bool IsFinalBlock)> ReadAsync(CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ReadResult result = await reader.ReadAsync(token);
        return (result.Buffer, result.IsCompleted);
    }

    public bool Read(out ReadOnlySequence<byte> Buffer, out bool IsFinalBlock)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (reader.TryRead(out var result))
        {
            Buffer = result.Buffer;
            IsFinalBlock = result.IsCompleted;
            return true;
        }
        Buffer = ReadOnlySequence<byte>.Empty;
        IsFinalBlock = false;
        return false;
    }

    public void AdvanceTo(SequencePosition consumed)
        => reader?.AdvanceTo(consumed);

    public void AdvanceTo(SequencePosition consumed, SequencePosition examined)
        => reader?.AdvanceTo(consumed, examined);

    public void Complete()
        => reader?.Complete();

    //async Task ReadPipeAsync(PipeReader reader, CancellationToken token)
    //{
    //    long prevBufferLength = 0;
    //    while (true) {
    //        ReadResult result = await reader.ReadAsync(token);

    //        ReadOnlySequence<byte> buffer = result.Buffer;
    //        //SequencePosition? position = null;

    //        //do {
    //        //    // Look for a EOL in the buffer
    //        //    position = buffer.PositionOf((byte)'\n');

    //        //    if (position != null) {
    //        //        // Process the line
    //        //        ProcessLine(buffer.Slice(0, position.Value));

    //        //        // Skip the line + the \n character (basically position)
    //        //        buffer = buffer.Slice(buffer.GetPosition(1, position.Value));
    //        //    }
    //        //}
    //        //while (position != null);
    //        if (stop) {
    //            break;
    //        }
    //        var consumed = ProcessBuffer(buffer, result.IsCompleted);

    //        // Tell the PipeReader how much of the buffer we have consumed
    //        reader.AdvanceTo(buffer.GetPosition(consumed), buffer.End);

    //        // Stop reading if there's no more data coming
    //        if (result.IsCompleted) {
    //            if (buffer.Length == prevBufferLength)
    //                // TODO: maybe throw if buffer.Length > 0
    //                break;
    //            prevBufferLength = buffer.Length;
    //        }
    //    }

    //    // Mark the PipeReader as complete
    //    reader.Complete();
    //}

    public void Finish()
    {
        //stop = true;
        writer?.Complete();
        reader?.Complete();
    }
}

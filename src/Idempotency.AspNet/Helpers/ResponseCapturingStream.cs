namespace Idempotency.AspNet.Helpers;

/// <summary>
/// A stream wrapper that captures all writes to the response body while still writing to the original stream.
/// </summary>
public sealed class ResponseCapturingStream : Stream
{
    private readonly Stream _innerStream;
    private readonly MemoryStream _captureStream;
    private readonly long _bufferLimit;
    private bool _disposed;

    public ResponseCapturingStream(Stream innerStream, long bufferLimit)
    {
        ArgumentNullException.ThrowIfNull(innerStream);
        _innerStream = innerStream;
        _captureStream = new MemoryStream();
        _bufferLimit = bufferLimit;
    }

    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => _innerStream.CanSeek;
    public override bool CanWrite => _innerStream.CanWrite;
    public override long Length => _innerStream.Length;

    public override long Position
    {
        get => _innerStream.Position;
        set => _innerStream.Position = value;
    }

    public string GetCapturedContent()
    {
        _captureStream.Position = 0;
        using var reader = new StreamReader(_captureStream, leaveOpen: true);
        return reader.ReadToEnd();
    }

    public byte[] GetCapturedBytes()
    {
        return _captureStream.ToArray();
    }

    public override void Flush()
    {
        _innerStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return _innerStream.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return _innerStream.Read(buffer, offset, count);
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return _innerStream.ReadAsync(buffer, cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _innerStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _innerStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_captureStream.Length + count <= _bufferLimit)
        {
            _captureStream.Write(buffer, offset, count);
        }

        _innerStream.Write(buffer, offset, count);
    }

    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_captureStream.Length + count <= _bufferLimit)
        {
            await _captureStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_captureStream.Length + buffer.Length <= _bufferLimit)
        {
            await _captureStream.WriteAsync(buffer, cancellationToken);
        }

        await _innerStream.WriteAsync(buffer, cancellationToken);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        if (_captureStream.Length + buffer.Length <= _bufferLimit)
        {
            _captureStream.Write(buffer);
        }
        _innerStream.Write(buffer);
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _captureStream.Dispose();
                // Don't dispose the inner stream - asp.net will handle that
            }
            _disposed = true;
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await _captureStream.DisposeAsync();
            // Don't dispose the inner stream - asp.net will handle that
            _disposed = true;
        }
        await base.DisposeAsync();
    }
}
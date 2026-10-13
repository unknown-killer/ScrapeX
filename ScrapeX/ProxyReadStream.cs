using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ScrapeX
{
    // Read ahead only; writes remain immediate so SOCKS/TLS handshakes cannot deadlock on an output buffer.
    internal sealed class ProxyReadStream : Stream
    {
        private readonly Stream inner; private readonly byte[] buffer = new byte[4096]; private int offset, count;
        public ProxyReadStream(Stream inner) { this.inner = inner; }
        public async Task<string> ReadHeaderAsync(CancellationToken token)
        {
            var header = new StringBuilder(); int state = 0;
            while (header.Length < 16384)
            {
                if (offset == count) { count = await inner.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false); offset = 0; if (count == 0) throw new IOException("Connection closed before response"); }
                while (offset < count && header.Length < 16384)
                {
                    char ch = (char)buffer[offset++]; header.Append(ch);
                    state = ch == (state == 0 || state == 2 ? '\r' : '\n') ? state + 1 : ch == '\r' ? 1 : 0;
                    if (state == 4) return header.ToString();
                }
            }
            throw new IOException("Response headers exceed 16 KB");
        }
        public override int Read(byte[] bytes, int start, int length)
        { if (offset < count) { int take = Math.Min(length, count - offset); Buffer.BlockCopy(buffer, offset, bytes, start, take); offset += take; return take; } return inner.Read(bytes, start, length); }
        public override Task<int> ReadAsync(byte[] bytes, int start, int length, CancellationToken token)
        { token.ThrowIfCancellationRequested(); if (offset < count) return Task.FromResult(Read(bytes, start, length)); return inner.ReadAsync(bytes, start, length, token); }
        public override void Write(byte[] bytes, int start, int length)
        { try { inner.Write(bytes, start, length); } catch (NotSupportedException ex) { throw new IOException("Connection closed before write", ex); } }
        public override async Task WriteAsync(byte[] bytes, int start, int length, CancellationToken token)
        { token.ThrowIfCancellationRequested(); try { await inner.WriteAsync(bytes, start, length, token).ConfigureAwait(false); } catch (NotSupportedException ex) { token.ThrowIfCancellationRequested(); throw new IOException("Connection closed before write", ex); } }
        public override void Flush() { inner.Flush(); }
        public override Task FlushAsync(CancellationToken token) { return inner.FlushAsync(token); }
        public override bool CanRead => inner.CanRead;
        public override bool CanWrite => inner.CanWrite;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override long Seek(long value, SeekOrigin origin) { throw new NotSupportedException(); }
        public override void SetLength(long value) { throw new NotSupportedException(); }
        protected override void Dispose(bool disposing) { if (disposing) inner.Dispose(); base.Dispose(disposing); }
    }
}

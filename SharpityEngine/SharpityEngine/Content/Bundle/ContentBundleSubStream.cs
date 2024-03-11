
namespace SharpityEngine.Content
{
    internal sealed class ContentBundleSubStream : Stream
    {
        // Private
        private Stream baseStream = null;
        private long offset = 0;
        private long length = 0;
        private long position = 0;

        // Properties
        public override bool CanRead
        {
            get { return baseStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return baseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return length; }
        }

        public override long Position 
        {
            get { return position; }
            set { position = this.Seek(value, SeekOrigin.Begin); } 
        }

        // Constructor
        public ContentBundleSubStream(Stream stream, long offset, long length)
        {
            // Check for stream
            if (stream == null)
                throw new ArgumentNullException("Stream");

            if (length < 1) 
                throw new ArgumentException("Length must be greater than zero.");

            this.baseStream = stream;
            this.offset = offset;
            this.length = length;

            stream.Seek(offset, SeekOrigin.Begin);
        }

        // Methods
        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            long remaining = length - position;
            if (remaining <= 0) return 0;
            if (remaining < count) count = (int)remaining;
            int read = baseStream.Read(buffer, offset, count);
            position += read;
            return read;
        }

        private void CheckDisposed()
        {
            if (baseStream == null)
                throw new ObjectDisposedException(GetType().Name);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long pos = position;

            if (origin == SeekOrigin.Begin)
                pos = offset;
            else if (origin == SeekOrigin.End)
                pos = length + offset;
            else if (origin == SeekOrigin.Current)
                pos += offset;

            if (pos < 0) pos = 0;
            else if (pos >= length) pos = length - 1;

            position = baseStream.Seek(this.offset + pos, SeekOrigin.Begin) - this.offset;

            return pos;
        }
        
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
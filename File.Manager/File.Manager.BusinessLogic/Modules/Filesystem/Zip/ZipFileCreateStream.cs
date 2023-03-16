using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Manager.BusinessLogic.Modules.Filesystem.Zip
{
    public class ZipFileCreateStream : Stream, IStaticDataSource
    {
        // Private fields -----------------------------------------------------

        private readonly MemoryStream memoryStream = new();
        private readonly ZipFile zipFile;
        private readonly string name;

        // IStaticDataSource implementation -----------------------------------

        Stream IStaticDataSource.GetSource() => memoryStream;

        // Public methods -----------------------------------------------------

        public ZipFileCreateStream(ZipFile zipFile, string name)
        {
            this.zipFile = zipFile;
            this.name = name;
        }

        public override void Flush()
        {
            memoryStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return memoryStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return memoryStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            memoryStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            memoryStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            memoryStream.Seek(0, SeekOrigin.Begin);

            var entry = new ZipEntry(name);

            zipFile.BeginUpdate();
            zipFile.Add(this, entry);
            zipFile.CommitUpdate();

            memoryStream.Close();

            base.Close();
        }

        // Public properties --------------------------------------------------

        public override bool CanRead => memoryStream.CanRead;

        public override bool CanSeek => memoryStream.CanSeek;

        public override bool CanWrite => memoryStream.CanWrite;

        public override long Length => memoryStream.Length;

        public override long Position 
        { 
            get => memoryStream.Position; 
            set => memoryStream.Position = value; 
        }
    }
}

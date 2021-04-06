using System;
using System.IO;
using System.IO.Compression;
using System.Web.Mvc;

namespace WebZipItFramework.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ZipUnoptimized()
        {
            var contentPath = Server.MapPath("~/bots/");
            var files = Directory.GetFiles(contentPath);
            var memoryStream = new MemoryStream();
            using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Update, leaveOpen: true))
            {
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(Path.GetFileName(file));
                    using (var entryStream = entry.Open())
                    {
                        var fileBytes = System.IO.File.ReadAllBytes(file); // use bytearray to simulate code review situation
                        entryStream.Write(fileBytes, 0, fileBytes.Length);
                    }
                }
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, "application/octet-stream", "Bots.zip");
        }

        public void ZipOptimized()
        {
            Response.ContentType = "application/octet-stream";
            Response.Headers.Add("Content-Disposition", "attachment; filename=\"Bots.zip\"");
            Response.BufferOutput = false;

            var contentPath = Server.MapPath("~/bots/");
            var files = Directory.GetFiles(contentPath);
            using (ZipArchive archive = new ZipArchive(new PositionWrapperStream(Response.OutputStream), ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    var entry = archive.CreateEntry(Path.GetFileName(file));
                    using (var entryStream = entry.Open())
                    {
                        var fileBytes = System.IO.File.ReadAllBytes(file); // use bytearray to simulate code review situation
                        entryStream.Write(fileBytes, 0, fileBytes.Length);
                    }
                }
            }
        }
    }

    // from https://stackoverflow.com/a/21513194/2919731
    public class PositionWrapperStream : Stream
    {
        private readonly Stream wrapped;

        private long pos = 0;

        public PositionWrapperStream(Stream wrapped)
        {
            this.wrapped = wrapped;
        }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override long Position
        {
            get { return pos; }
            set { throw new NotSupportedException(); }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            pos += count;
            wrapped.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            wrapped.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            wrapped.Dispose();
            base.Dispose(disposing);
        }

        // all the other required methods can throw NotSupportedException

        public override bool CanRead => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
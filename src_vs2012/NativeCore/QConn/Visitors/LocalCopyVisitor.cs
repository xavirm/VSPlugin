﻿using System;
using System.IO;
using BlackBerry.NativeCore.Diagnostics;
using BlackBerry.NativeCore.QConn.Model;
using BlackBerry.NativeCore.QConn.Services;

namespace BlackBerry.NativeCore.QConn.Visitors
{
    /// <summary>
    /// Class saving the files received from target into local file system.
    /// </summary>
    public class LocalCopyVisitor : BaseVisitorMonitor, IFileServiceVisitor
    {
        private readonly string _outputPath;
        private string _basePath;
        private Stream _stream;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public LocalCopyVisitor(string outputPath)
        {
            if (string.IsNullOrEmpty(outputPath))
                throw new ArgumentNullException("outputPath");

            _outputPath = outputPath;
        }

        #region IFileServiceVisitor Implementation

        public bool IsCancelled
        {
            get;
            set;
        }

        public void Begin(TargetFile descriptor)
        {
            ResetWait();
            _basePath = GetInitialBasePath(descriptor, true);
            _stream = null;
        }

        public void End()
        {
            Close();
            NotifyCompleted();
        }

        public void FileOpening(TargetFile file)
        {
            if (_stream != null)
                throw new InvalidOperationException("Previous file was not correctly closed");

            // assumption is that directory entering, supposed to create whole directory structure,
            // was already called before on that visitor:

            var name = GetLocalPath(file);
            _stream = File.Create(name, TargetServiceFile.DownloadUploadChunkSize, FileOptions.SequentialScan);
        }

        public void FileContent(TargetFile file, byte[] data, ulong totalRead)
        {
            if (_stream == null)
                throw new ObjectDisposedException("LocalCopyVisitor");

            _stream.Write(data, 0, data.Length);
        }

        public void FileClosing(TargetFile file)
        {
            if (_stream == null)
                throw new ObjectDisposedException("LocalCopyVisitor");

            _stream.Close();
            _stream = null;
        }

        public void DirectoryEntering(TargetFile folder)
        {
            var name = GetLocalPath(folder);

            try
            {
                Directory.CreateDirectory(name);
            }
            catch (Exception ex)
            {
                QTraceLog.WriteException(ex, "Unable to create folder: \"{0}\"", name);
            }
        }

        public void UnknownEntering(TargetFile descriptor)
        {
            // just create simple empty file:
            var name = GetLocalPath(descriptor);
            var stream = File.Create(name);
            stream.Close();
        }

        private string GetLocalPath(TargetFile descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException("descriptor");

            var name = descriptor.Path.StartsWith(_basePath) ? descriptor.Path.Substring(_basePath.Length) : descriptor.Path;
            name = name.Replace('/', Path.DirectorySeparatorChar);
            if (!string.IsNullOrEmpty(name) && name[0] == Path.DirectorySeparatorChar)
                name = name.Substring(1);

            return Path.Combine(_outputPath, name);
        }

        #endregion

        private void Close()
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }

            base.Dispose(disposing);
        }
    }
}

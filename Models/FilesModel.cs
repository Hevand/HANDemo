using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAN.Demo.Models
{
    public class FilesModel
    {
        public string Title { get; set; }
        public IEnumerable<FileDetails> Files { get; set; }
        public DateTime RequestedOn { get; set; }

        public TimeSpan GeneratedIn { get; set; }
    }

    public class FileDetails
    {
        public string Title { get; set; }
        public TimeSpan UploadStarted { get; set; }
        public TimeSpan UploadEnded{ get; set; }

        public TimeSpan Elapsed { get { return UploadEnded - UploadStarted; } }
    }
}

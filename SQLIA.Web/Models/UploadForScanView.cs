using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SQLIA.Web.Models
{
    public class UploadForScanView
    {
        [Required]
        [DisplayName("Select File to Upload")]
        public HttpPostedFileBase File { get; set; }

    }
}
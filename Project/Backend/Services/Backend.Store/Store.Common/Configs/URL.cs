using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Common.Configs
{
    public class StaticFileConfig
    {
        public string BaseUrl { get; set; } = string.Empty;
        public ImageUrlConfig ImageUrl { get; set; } = new ImageUrlConfig();
    }

    public class ImageUrlConfig
    {
        public string PhysicalPath { get; set; } = string.Empty;
        public string RequestPath { get; set; } = string.Empty;
    }
}

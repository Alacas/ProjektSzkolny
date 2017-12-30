using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Image_Recognition
{
    public class SampleImage
    {
        public System.Windows.Controls.Image Image { get; set; }
        public string Words { get; set; }
        public string Category { get; set; }
        public string ImageKey { get; set; }
        public SampleImage(System.Windows.Controls.Image image, string words, string category, string imageKey)
        {
            Words = words;
            Category = category;
            Image = image;
            ImageKey = imageKey;
        }
    }
}

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
        public string Word { get; set; }
        public string Category { get; set; }
        public SampleImage(System.Windows.Controls.Image image, string word, string category)
        {
            Word = word;
            Category = category;
            Image = image;
        }
    }
}

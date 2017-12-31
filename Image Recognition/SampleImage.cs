using Accord.Math;
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
        public string Words
        {
            get
            {
                if (Vector != null)
                    return Vector.ToString(DefaultArrayFormatProvider.InvariantCulture);
                else return null;
            }
        }
        public double[] Vector { get; set; }
        public string Category { get; set; }
        public string ExptectedCategory { get; set; }
        public string ImageKey { get; set; }
        public SampleImage(System.Windows.Controls.Image image, string category, string imageKey, string expectedCategory = null)
        {

            Category = category;
            Image = image;
            ImageKey = imageKey;
            ExptectedCategory = expectedCategory;
        }
    }
}

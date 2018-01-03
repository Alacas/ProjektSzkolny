using Accord.MachineLearning;
using Accord.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using Accord.Math;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.Math.Distances;
using Accord;

namespace Image_Recognition
{
    
    public partial class MainWindow : Window
    {
        MulticlassSupportVectorMachine<IKernel> ksvm;
        Dictionary<string, Bitmap> originalTrainingImages;
        Dictionary<string, Bitmap> originalTestImages;

        public List<SampleImage> TrainingImagesToView { get; set; }
        public ObservableCollection<SampleImage> TestImagesToView { get; set; }
        Dictionary<string, int> Categories = new Dictionary<string, int>();
        public MainWindow()
        {
            InitializeComponent();
        }
        IBagOfWords<Bitmap> bow;

        private void StartWordMatching_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)SurfRadio.IsChecked)
            {
                BinarySplit binarySplit = new BinarySplit(Int32.Parse(WordsNumber.Text));
                BagOfVisualWords surfBow = new BagOfVisualWords(binarySplit);
                bow = surfBow.Learn(originalTrainingImages.Values.ToArray());
            }
            else if (((bool)FreagRadio.IsChecked))
            {
                var kmodes = new KModes<byte>(Int32.Parse(WordsNumber.Text), new Hamming());
                var freak = new FastRetinaKeypointDetector();
                var freakBow = new BagOfVisualWords<FastRetinaKeypoint, byte[]>(freak, kmodes);
                bow = freakBow.Learn(originalTrainingImages.Values.ToArray());
            }
            else
            {
                BinarySplit binarySplit = new BinarySplit(Int32.Parse(WordsNumber.Text));
                var hog = new HistogramsOfOrientedGradients();
                var hogBow = BagOfVisualWords.Create(hog, binarySplit);
                bow = hogBow.Learn(originalTrainingImages.Values.ToArray());

            }

            foreach (var item in TrainingImagesToView)
            {               
                Bitmap image = originalTrainingImages[item.ImageKey] as Bitmap;
                item.Vector = (bow as ITransform<Bitmap, double[]>).Transform(image);
            }
            foreach (var item in TestImagesToView)
            {
                Bitmap image = originalTestImages[item.ImageKey] as Bitmap;
                item.Vector = (bow as ITransform<Bitmap, double[]>).Transform(image);
            }
            OutpusConsole.Text = "Done! \nNow select method of learning and press Start Traning button.";
            TestItemsList.Items.Refresh();
            TraningItemsList.Items.Refresh();
            StartTrainingButton.IsEnabled = true;
            EstimateGaussianButton.IsEnabled = true;
            EtimateComplexityButton.IsEnabled = true;
        }

        private void OnLoad()
        {
            var path = new DirectoryInfo(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources"));
            TrainingImagesToView = new List<SampleImage>();
            TestImagesToView = new ObservableCollection<SampleImage>();
            originalTestImages = new Dictionary<string, Bitmap>();
            originalTrainingImages = new Dictionary<string, Bitmap>();
            int currentClassLabel = 0;
            foreach (DirectoryInfo classFolder in path.EnumerateDirectories())
            {
                string name = classFolder.Name;
                Categories.Add(name, currentClassLabel);
                FileInfo[] files = GetFilesByExtensions(classFolder, ".jpg", ".png").ToArray();               
                Accord.Math.Vector.Shuffle(files);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i];
                    Bitmap image = (Bitmap)Bitmap.FromFile(file.FullName);
                    string imageKey = file.FullName;
                    if ((i / (double)files.Length) < 0.7)
                    {        
                        originalTrainingImages.Add(imageKey, image);
                        System.Windows.Controls.Image obrazek = new System.Windows.Controls.Image();
                        obrazek.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                              image.GetHbitmap(),
                              IntPtr.Zero,
                              System.Windows.Int32Rect.Empty,
                              BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));                  
                        TrainingImagesToView.Add(new SampleImage(obrazek, name, imageKey));
                    }
                    else
                    {
                        originalTestImages.Add(imageKey, image);
                        System.Windows.Controls.Image obrazek = new System.Windows.Controls.Image();
                        obrazek.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                              image.GetHbitmap(),
                              IntPtr.Zero,
                              System.Windows.Int32Rect.Empty,
                              BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));
                        TestImagesToView.Add(new SampleImage(obrazek, "", imageKey, name));
                    }
                }

                currentClassLabel++;
            }
        }
        public static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OnLoad();
            OutpusConsole.Text = "Select number of words and press Start Word Matching button. \nThis may take a (very) significant amount of time...";


            TraningItemsList.ItemsSource = TrainingImagesToView;
            TestItemsList.ItemsSource = TestImagesToView;


        }

        private void StartTraning_Click(object sender, RoutedEventArgs e)
        {
            IKernel kernel = new ChiSquare();
            double complexity = 1;
            double tolerance = 0.01;
            int cacheSize = 500;
            Enum.TryParse(StrategyComboBox.SelectedItem.ToString(), out SelectionStrategy strategy);
            var teacher = new MulticlassSupportVectorLearning<IKernel>()
            {
                Kernel = kernel,
                Learner = (param) =>
                {
                    return new SequentialMinimalOptimization<IKernel>()
                    {
                        Kernel = kernel,
                        Complexity = complexity,
                        Tolerance = tolerance,
                        CacheSize = cacheSize,
                        Strategy = strategy,
                    };
                }
            };
            double[][] inputs;
            int[] outputs;
            GetData(out inputs, out outputs);
            this.ksvm = teacher.Learn(inputs, outputs);
            StartClassifyingButton.IsEnabled = true;
            OutpusConsole.Text = "Training is finished. Now you can press Start Classifying Button.";
        }
        private void GetData(out double[][] inputs, out int[] outputs)
        {
            List<double[]> inputList = new List<double[]>();
            List<int> outputList = new List<int>();
            foreach (var item in TrainingImagesToView)
            {
                inputList.Add(item.Vector);
                int categoryName;
                Categories.TryGetValue(item.Category, out categoryName);
                outputList.Add(categoryName);
            }
            inputs = inputList.ToArray();
            outputs = outputList.ToArray();
        }

        private void StartClassifyingButton_Click(object sender, RoutedEventArgs e)
        {
            int errors = 0;
            foreach (var item in TestImagesToView)
            {
                int actual = ksvm.Decide(item.Vector);
                item.Category = Categories.FirstOrDefault(x => x.Value == actual).Key;
                if (item.Category != item.ExptectedCategory)
                {
                    errors++;

                }
            }
            int percentOfErrors = ((TestImagesToView.Count - errors) * 100 / TestImagesToView.Count);
            OutpusConsole.Text = "Efficiency: " + percentOfErrors + "%.";
            TestItemsList.Items.Refresh();

        }

        private IKernel getKernel()
        {
            if ((bool)GaussianRadio.IsChecked)
            {
                return new Gaussian(Double.Parse(GoussianSigma.Text));
            }
            else if ((bool)PolynomialRadio.IsChecked)
            {
                if (Int32.Parse(DegreeTextBox.Text) == 1)
                    return new Linear(Double.Parse(ContantTextBox.Text));
                else
                    return new Polynomial(Int32.Parse(DegreeTextBox.Text), Double.Parse(ContantTextBox.Text));
            }
            else if ((bool)ChiRadio.IsChecked)
            {
                return new ChiSquare();
            }
            else if ((bool)HistogramRadio.IsChecked)
            {
                return new HistogramIntersection(1, 1);
            }

            throw new Exception();
        }

        private void EstimateGaussianButton_Click(object sender, RoutedEventArgs e)
        {
            double[][] inputs;
            int[] outputs;
            GetData(out inputs, out outputs);

            DoubleRange range;
            Gaussian g = Gaussian.Estimate(inputs, inputs.Length, out range);

            GoussianSigma.Text = g.Sigma.ToString("0.##");
        }

        private void EtimateComplexityButton_Click(object sender, RoutedEventArgs e)
        {
            double[][] inputs;
            int[] outputs;
            GetData(out inputs, out outputs);
            IKernel kernel = getKernel();
            ComplexityTextBox.Text = kernel.EstimateComplexity(inputs).ToString("0.#");
        }
    }
}

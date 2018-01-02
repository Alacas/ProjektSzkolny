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

    //TODO zmienic nazwę okna
    //TODO sprawdzić czy da się łatwo zrobić zakładki
    //TODO wyswietlanie w jednym oknie danych treningowych wraz ze slowami, a w drugim oknie testowych z rpzypisanymi klasami

    public partial class MainWindow : Window
    {
        MulticlassSupportVectorMachine<IKernel> ksvm;
        Dictionary<string, Bitmap> originalTrainingImages;
        Dictionary<string, Bitmap> originalTestImages;


        public List<SampleImage> TrainingImagesToView { get; set; }
        public ObservableCollection<SampleImage> TestImagesToView { get; set; }


        //TODO wywalic
        Dictionary<string, Bitmap> originalImages;
        Dictionary<string, Bitmap> displayImages;
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
                // Get item image
                Bitmap image = originalTrainingImages[item.ImageKey] as Bitmap;

                // Get a feature vector representing this image
                item.Vector = (bow as ITransform<Bitmap, double[]>).Transform(image);




            }
            foreach (var item in TestImagesToView)
            {
                // Get item image
                Bitmap image = originalTestImages[item.ImageKey] as Bitmap;

                // Get a feature vector representing this image
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

            // Create image list to load images into
            originalImages = new Dictionary<string, Bitmap>();
            displayImages = new Dictionary<string, Bitmap>();
            TrainingImagesToView = new List<SampleImage>();
            TestImagesToView = new ObservableCollection<SampleImage>();

            originalTestImages = new Dictionary<string, Bitmap>();
            originalTrainingImages = new Dictionary<string, Bitmap>();

            //tu ma być wyświetlane
            //ImageList imageList = new ImageList();
            //imageList.ImageSize = new Size(64, 64);
            //imageList.ColorDepth = ColorDepth.Depth32Bit;
            //listView1.LargeImageList = imageList;

            int currentClassLabel = 0;

            //TODO złapać wyjątek gdy nie ma potrzebnego foledru, albo jest pusty
            foreach (DirectoryInfo classFolder in path.EnumerateDirectories())
            {
                string name = classFolder.Name;
                Categories.Add(name, currentClassLabel);

                // Create two list view groups for each class.  Use 70%
                // of training instances and the remaining 30% as testing.
                //ListViewGroup trainingGroup = listView1.Groups.Add(name + ".train", name + ".train");
                //ListViewGroup testingGroup = listView1.Groups.Add(name + ".test", name + ".test");

                // Load the images from the directory that contains images for each class
                FileInfo[] files = GetFilesByExtensions(classFolder, ".jpg", ".png").ToArray();

                // Shuffle the samples
                Accord.Math.Vector.Shuffle(files);

                // For each file in the class folder
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i];

                    Bitmap image = (Bitmap)Bitmap.FromFile(file.FullName);


                    string imageKey = file.FullName;

                    //imageList.Images.Add(imageKey, image);
                    originalImages.Add(imageKey, image); //po co to??
                    displayImages.Add(imageKey, image);
                    //TODO robiz testowe i treningowe dane
                    //ListViewItem item;
                    if ((i / (double)files.Length) < 0.7)
                    {
                        // Put the first 70% in training set
                        //item = new ListViewItem(trainingGroup);
                        originalTrainingImages.Add(imageKey, image);

                        System.Windows.Controls.Image obrazek = new System.Windows.Controls.Image();
                        obrazek.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                              image.GetHbitmap(),
                              IntPtr.Zero,
                              System.Windows.Int32Rect.Empty,
                              BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));
                        //TrainingItemsList.Items.Add(obrazek);
                        TrainingImagesToView.Add(new SampleImage(obrazek, name, imageKey));


                    }
                    else
                    {
                        // Put the restant 30% in test set
                        //item = new ListViewItem(testingGroup);
                        //TODO jesli to mozliwe wywalic imagekey
                        originalTestImages.Add(imageKey, image);
                        System.Windows.Controls.Image obrazek = new System.Windows.Controls.Image();
                        obrazek.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                              image.GetHbitmap(),
                              IntPtr.Zero,
                              System.Windows.Int32Rect.Empty,
                              BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));
                        //TrainingItemsList.Items.Add(obrazek);
                        TestImagesToView.Add(new SampleImage(obrazek, "", imageKey, name));
                    }

                    //item.ImageKey = imageKey;
                    //item.Name = shortName;
                    //item.Text = shortName;
                    // Use the tag object to store the class label of each image 
                    // - we will recover it from here later
                    //item.Tag = new Tuple<double[], int>(null, currentClassLabel);

                    //listView1.Items.Add(item);
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
            // Get the chosen kernel 
            IKernel kernel = new ChiSquare(); //new Gaussian(6.2);

            // Extract training parameters from the interface
            //TODO pobrac to z UI
            double complexity = 1;
            double tolerance = 0.01;
            int cacheSize = 500;
            Enum.TryParse(StrategyComboBox.SelectedItem.ToString(), out SelectionStrategy strategy);


            // Create the support vector machine learning algorithm
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

            // Get the input and output data
            double[][] inputs;
            int[] outputs;
            GetData(out inputs, out outputs);

            // Prepare to start learning
            //lbStatus.Text = "Training the classifiers. This may take a (very) significant amount of time...";
            //Application.DoEvents();



            // Train the machines. It should take a while.
            this.ksvm = teacher.Learn(inputs, outputs);

            //sw.Stop();

            // Compute the training error (accuracy, also known as zero-one-loss)
            //double error = new ZeroOneLoss(outputs).Loss(ksvm.Decide(inputs));

            //lbStatus.Text = String.Format(
            //    "Training complete ({0}ms, {1}er). Click Classify to test the classifiers.",
            //    sw.ElapsedMilliseconds, error);

            //btnClassifyElimination.Enabled = true;

            //// Populate the information tab with the machines
            //dgvMachines.Rows.Clear();
            //for (int i = 0, k = 1; i < ksvm.NumberOfOutputs; i++)
            //{
            //    for (int j = 0; j < i; j++, k++)
            //    {
            //        SupportVectorMachine<IKernel> machine = ksvm[i, j];

            //        int numberOfSupportVectors = machine.SupportVectors == null ?
            //            0 : machine.SupportVectors.Length;

            //        int rowIndex = dgvMachines.Rows.Add(k, i + "-vs-" + j, numberOfSupportVectors, machine.Threshold);
            //        dgvMachines.Rows[rowIndex].Tag = machine;
            //    }
            //}

            //// approximate size in bytes = 
            ////   number of support vectors * number of doubles in a support vector * size of double
            //int bytes = ksvm.SupportVectorUniqueCount * ksvm.NumberOfInputs * sizeof(double);
            //double megabytes = bytes / (1024.0 * 1024.0);
            //lbSize.Text = String.Format("{0} ({1} MB)", ksvm.SupportVectorUniqueCount, megabytes);
            StartClassifyingButton.IsEnabled = true;
            OutpusConsole.Text = "Training is finished. Now you can press Start Classifying Button.";


        }
        private void GetData(out double[][] inputs, out int[] outputs)
        {
            List<double[]> inputList = new List<double[]>();
            List<int> outputList = new List<int>();


            //przechowywać vector jako string i jako double[], przechowywac jakos tablice typow

            foreach (var item in TrainingImagesToView)
            {
                // Recover the class label and the feature vector
                // that had been stored in the rows' Tag objects
                //var info = item.Words as Tuple<double[], int>;
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

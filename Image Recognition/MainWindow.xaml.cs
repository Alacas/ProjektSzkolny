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

namespace Image_Recognition
{

    //TODO zmienic nazwę okna
    //TODO sprawdzić czy da się łatwo zrobić zakładki
    //TODO wyswietlanie w jednym oknie danych treningowych wraz ze slowami, a w drugim oknie testowych z rpzypisanymi klasami

    public partial class MainWindow : Window
    {

        Dictionary<string, Bitmap> originalTrainImages;
        Dictionary<string, Bitmap> originalTestImages;
        public List<String> TetowaLista { get; set; }
        

        public List<SampleImage> TrainingImagesToView { get; set; }

        Dictionary<string, Bitmap> originalImages;
        Dictionary<string, Bitmap> displayImages;
        public MainWindow()
        {
            InitializeComponent();
        }
        IBagOfWords<Bitmap> bow;

        private void StartWordMatching_Click(object sender, RoutedEventArgs e)
        {
          
            BinarySplit binarySplit = new BinarySplit(30);
            BagOfVisualWords surfBow = new BagOfVisualWords(binarySplit);
            bow = surfBow.Learn(originalTrainImages.Values.ToArray());
        }
        
        //TODO zrobić to przez event onloaded
        private void OnLoad()
        {
            TetowaLista = new List<String>();
            var path = new DirectoryInfo(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources"));

            // Create image list to load images into
            originalImages = new Dictionary<string, Bitmap>();
            displayImages = new Dictionary<string, Bitmap>();
            TrainingImagesToView = new List<SampleImage>();

            originalTestImages = new Dictionary<string, Bitmap>();
            originalTrainImages = new Dictionary<string, Bitmap>();

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

                    string shortName = file.Name;
                    string imageKey = file.FullName;

                    //imageList.Images.Add(imageKey, image);
                    originalImages.Add(imageKey, image);
                    displayImages.Add(imageKey, image);
                    //TODO robiz testowe i treningowe dane
                    //ListViewItem item;
                    if ((i / (double)files.Length) < 0.7)
                    {
                        // Put the first 70% in training set
                        //item = new ListViewItem(trainingGroup);
                        originalTrainImages.Add(imageKey, image);
                       
                        System.Windows.Controls.Image obrazek = new System.Windows.Controls.Image();
                        obrazek.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                              image.GetHbitmap(),
                              IntPtr.Zero,
                              System.Windows.Int32Rect.Empty,
                              BitmapSizeOptions.FromWidthAndHeight(50, 50));
                        //TrainingItemsList.Items.Add(obrazek);
                        TrainingImagesToView.Add(new SampleImage(obrazek, "", name));
                       
                        TetowaLista.Add(name);

                    }
                    else
                    {
                        // Put the restant 30% in test set
                        //item = new ListViewItem(testingGroup);
                        originalTestImages.Add(imageKey, image);
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
            var test = 1;
            


        }
        public static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }
        List<TodoItem> items = new List<TodoItem>();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            OnLoad();
            
            items.Add(new TodoItem() { Title = "Complete this WPF tutorial", Completion = 45 });
            items.Add(new TodoItem() { Title = "Learn C#", Completion = 80 });
            items.Add(new TodoItem() { Title = "Wash the car", Completion = 0 });

            lbTodoList.ItemsSource = items;
            TestItemsList.ItemsSource = TrainingImagesToView;


        }
        public class TodoItem
        {
            public string Title { get; set; }
            public int Completion { get; set; }
        }
    }
}

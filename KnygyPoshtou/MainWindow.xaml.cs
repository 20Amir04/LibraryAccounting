using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using Microsoft.Win32;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace KnygyPoshtou
{
    public partial class MainWindow : Window
    {
        private List<Order> orders = new List<Order>();
        public MainWindow()
        {
            InitializeComponent();
            dataGrid1.ItemsSource = orders;

        }


        //private static List<Order> GetOrders()
        //{
        //    return new List<Order>()
        //    {
        //        new Order("code1", "author1", "title1", DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), 1.0m, "reader1", DateOnly.FromDateTime(new DateTime(2004, 10, 10)), "None", "Dom1"),
        //        new Order("code2", "author2", "title2", DateOnly.FromDateTime(DateTime.Now.AddDays(-2)), 2.0m, "reader2", DateOnly.FromDateTime(new DateTime(2004, 10, 12)), "None2", "Dom2"),
        //        new Order("code3", "author3", "title3", DateOnly.FromDateTime(DateTime.Now.AddDays(-3)), 3.0m, "reader3", DateOnly.FromDateTime(new DateTime(2004, 10, 13)), "None3", "Dom3"),
        //        new Order("code4", "author4", "title4", DateOnly.FromDateTime(DateTime.Now.AddDays(-4)), 4.0m, "reader4", DateOnly.FromDateTime(new DateTime(2004, 10, 14)), "None4", "Dom4"),
        //        new Order("code5", "author5", "title5", DateOnly.FromDateTime(DateTime.Now.AddDays(-5)), 5.0m, "reader5", DateOnly.FromDateTime(new DateTime(2004, 10, 15)), "None5", "Dom5"),
        //    };
        //}


        private void filterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string filter = filterTextBox.Text;
            if (string.IsNullOrWhiteSpace(filter))
            {
                ClearFilter();
                return;
            }

            Filter(filter);
        }

        private void Filter(string filter)
        {

            dataGrid1.IsReadOnly = true;

            dataGrid1.ItemsSource = orders
                .Where(order => Filter(order, filter))
                .ToList();
        }


        private void ClearFilter()
        {
            dataGrid1.IsReadOnly = false;
            dataGrid1.ItemsSource = orders;
        }

        private static bool Filter(Order order, string filter)
        {
            if (order == null) return false;

            if (!string.IsNullOrWhiteSpace(order.BookCode)
                && order.BookCode.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(order.Author)
             && order.Author.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(order.Title)
             && order.Title.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (order.PublishYear.ToString().Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (order.Price.ToString().Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }


            if (!string.IsNullOrWhiteSpace(order.ReaderName)
             && order.ReaderName.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }


            if (order.ReaderDob.ToString().Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(order.ReaderEducation)
             && order.ReaderEducation.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(order.ReaderAddress)
             && order.ReaderAddress.Contains(filter, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            var result = dialog.ShowDialog();

            if (result.Value != true)
            {
                return;
            }

            File.WriteAllText(dialog.FileName, JsonConvert.SerializeObject(orders));
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            var result = dialog.ShowDialog();

            if (result.Value != true)
            {
                return;
            }

            var text = File.ReadAllText(dialog.FileName);
            orders = JsonConvert.DeserializeObject<List<Order>>(text);

            dataGrid1.ItemsSource = orders;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var years = orders
                .Select(order => (DateTime.Now.Year - order.ReaderDob.Year))
                .GroupBy(year => year);

            readerAgeChart.Series = years
                .Select(yearGroup => new PieSeries<int>
                {
                    Name = yearGroup.Key.ToString(),
                    Values = new int[] { yearGroup.Count() },
                    DataLabelsPaint = new SolidColorPaint(SKColors.Black),
                    DataLabelsSize = 15,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => yearGroup.Key.ToString() + "y/o" + ": " + point.PrimaryValue.ToString()
                });

            var popularBooks = orders
                .Select(order => order.Title)
                .GroupBy(title => title);

            popularBookChart.Series = new ObservableCollection<ISeries>
            {
                new ColumnSeries<int>
                {
                    Name = "",
                    Values = new ObservableCollection<int>(popularBooks.Select(x=> x.Count())),
                }
            };

            popularBookChart.XAxes = new List<Axis>()
            {
                new Axis()
                {
                    Labels = popularBooks.Select(x=> x.Key).ToArray()
                }
            };


            var popularAuthor = orders
                .Select(order => order.Author)
                .GroupBy(title => title);

            popularAuthorChart.Series = new ObservableCollection<ISeries>
            {
                new ColumnSeries<int>
                {
                    Name = "",
                    Values = new ObservableCollection<int>(popularAuthor.Select(x=> x.Count())),
                }
            };

            popularAuthorChart.XAxes = new List<Axis>()
            {
                new Axis()
                {
                    Labels = popularAuthor.Select(x=> x.Key).ToArray()
                }
            };


            var earnedPerYear = orders
                .GroupBy(x => x.SaleDate.Year, x => x.Price)
                .OrderBy(x => x.Key);

            earnedChart.Series = new ObservableCollection<ISeries>
            {
                new LineSeries<decimal>
                {
                    Name = "",
                    Values = new ObservableCollection<decimal>(earnedPerYear.Select(x=> x.Sum())),
                    TooltipLabelFormatter= x => "$" + x.PrimaryValue.ToString("N2")
                }
            };

            earnedChart.XAxes = new List<Axis>()
            {
                new Axis()
                {
                    Labels = earnedPerYear.Select(x=> x.Key.ToString()).ToArray(),
                }
            };

            earnedChart.YAxes = new List<Axis>()
            {
                new Axis()
                {
                    Labeler = Labelers.Currency
                }
            };

        }

        private void dataGrid1_SelectionChanged()
        {

        }
    }

    public class Order
    {
        public Order()
        {
        }

        public Order(
            string bookCode,
            string author,
            string title,
            DateOnly publishYear,
            decimal price,
            string readerName,
            DateOnly readerDob,
            string readerEducation,
            string readerAddress,
            DateTime saleDate)
        {
            BookCode = bookCode;
            Author = author;
            Title = title;
            PublishYear = publishYear.ToDateTime(new TimeOnly());
            Price = price;
            ReaderName = readerName;
            ReaderDob = readerDob.ToDateTime(new TimeOnly());
            ReaderEducation = readerEducation;
            ReaderAddress = readerAddress;
            SaleDate = saleDate;
        }

        public string BookCode { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public DateTime PublishYear { get; set; }
        public decimal Price { get; set; }
        public string ReaderName { get; set; }
        public DateTime ReaderDob { get; set; }
        public string ReaderEducation { get; set; }
        public string ReaderAddress { get; set; }
        public DateTime SaleDate { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Charting
{
    /// <summary>
    /// Interaction logic for SpectrumChart.xaml
    /// </summary>
    public partial class SpectrumChart : UserControl
    {
        ObservableCollection<Point> _viewablePoints;
        public ObservableCollection<Point> ViewablePoints
        {
            get { return _viewablePoints; }
            set
            {
                if (_viewablePoints != value)
                {
                    _viewablePoints = value;
                }
            }
        }
        public SpectrumChart()
        {
            InitializeComponent();

            ViewablePoints = new ObservableCollection<Point>();
            for (int i = 0; i < 100; i++)
            {
                ViewablePoints.Add(new Point(i, i));
            }
        }
    }
}

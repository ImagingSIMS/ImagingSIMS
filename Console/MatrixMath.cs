using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using ImagingSIMS.Common;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ConsoleApp
{
    partial class Program
    {
        private static void main_MatrixMath()
        {
            var inputFixedImage = ImageGenerator.Instance.FromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\inputFixedImage.bmp");
            var inputMovingImage = ImageGenerator.Instance.FromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\inputMovingImage.bmp");
            var fixedPointList = PointSet.PointSetFromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\fixedPointList.pts");
            var movingPointList = PointSet.PointSetFromFile(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\movingPointsList.pts");

            var matrixFixedImage = ImageGenerator.Instance.ConvertToData3D(inputFixedImage);
            var matrixMovingImage = ImageGenerator.Instance.ConvertToData3D(inputMovingImage);

            double[,] fixedPointsNorm = new double[fixedPointList.Count, 2];
            double[,] movingPointsNorm = new double[movingPointList.Count, 2];

            double[,] fixedPoints = new double[fixedPointList.Count, 2];
            double[,] movingPoints = new double[movingPointList.Count, 2];
            for (int i = 0; i < fixedPointList.Count; i++)
            {
                fixedPointsNorm[i, 0] = fixedPointList[i].X;
                fixedPointsNorm[i, 1] = fixedPointList[i].Y;

                movingPointsNorm[i, 0] = movingPointList[i].X;
                movingPointsNorm[i, 1] = movingPointList[i].Y;

                fixedPoints[i, 0] = fixedPointList[i].X * matrixFixedImage.Width;
                fixedPoints[i, 1] = fixedPointList[i].Y * matrixFixedImage.Height;

                movingPoints[i, 0] = movingPointList[i].X * matrixMovingImage.Width;
                movingPoints[i, 1] = movingPointList[i].Y * matrixMovingImage.Height;
            }

            // http://math.stackexchange.com/questions/296794/finding-the-transform-matrix-from-4-projected-points-with-javascript
            // [row, col]

            var numPoints = fixedPointList.Count;

            var uv_mat = new double[3, 3];
            var uv_vec = new double[3];
            var xy_mat = new double[3, 3];
            var xy_vec = new double[3];

            for (int i = 0; i < 3; i++)
            {
                xy_mat[0, i] = fixedPointList[i].X;
                xy_mat[1, i] = fixedPointList[i].Y;
                xy_mat[2, i] = 1;

                uv_mat[0, i] = movingPointList[i].X;
                uv_mat[1, i] = movingPointList[i].Y;
                uv_mat[2, i] = 1;
            }

            xy_vec[0] = fixedPointList[3].X;
            xy_vec[1] = fixedPointList[3].Y;
            xy_vec[2] = 1;

            uv_vec[0] = movingPointList[3].X;
            uv_vec[1] = movingPointList[3].Y;
            uv_vec[2] = 1;

            // lambda, mu, tao
            var xy_coeffs = xy_mat.Solve(xy_vec);
            var uv_coeffs = uv_mat.Solve(uv_vec);

            var xy_basisMat = new double[3, 3];
            var uv_basisMat = new double[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    xy_basisMat[i, j] = xy_coeffs[j] * xy_mat[i, j];
                    uv_basisMat[i, j] = uv_coeffs[j] * uv_mat[i, j];
                }
            }

            var combined = uv_basisMat.DotWithTransposed(xy_basisMat);

            var dFixed = ImageGenerator.Instance.ConvertToData3D(inputFixedImage);
            var dMoving = ImageGenerator.Instance.ConvertToData3D(inputMovingImage);

            var transformed = new Data3D(dFixed.Width, dFixed.Height, 4);
            for (int x = 0; x < dFixed.Width; x++)
            {
                for (int y = 0; y < dFixed.Height; y++)
                {
                    var inputCoord = new double[] { x, y, 1 };
                    var transformedCoord = combined.Dot(inputCoord);

                    double tx = transformedCoord[0] * matrixFixedImage.Width / transformedCoord[2];
                    double ty = transformedCoord[1] * matrixFixedImage.Height / transformedCoord[2];

                    int xx = (int)tx;
                    int yy = (int)ty;

                    if (xx < 0 || xx >= dFixed.Width - 1 || yy < 0 || yy >= dFixed.Height)
                        continue;

                    for (int z = 0; z < 3; z++)
                    {
                        transformed[xx, yy, z] = dFixed[x, y, z];
                    }

                    //double xDiff = tx - xx;
                    //double yDiff = ty - yy;

                    //for (int z = 0; z < 4; z++)
                    //{

                    //}
                }
            }

            var bsTransformed = ImageGenerator.Instance.Create(transformed);
            bsTransformed.Save(@"C:\Users\taro148\AppData\Roaming\ImagingSIMS\plugins\imageregistration\transfer\transformed.bmp");
        }
    }
}

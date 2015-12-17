using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSIMS.Data.Analysis
{
    public static class Colocalization
    {       
        const int saturation = 255;

        public static Data2D Correlate(Data2D Matrix1, Data2D Matrix2, int Threshold1 = 0, int Threshold2 = 0)
        {
            if (Matrix1.Width != Matrix2.Width || Matrix1.Height != Matrix2.Height) 
                throw new ArgumentException("Image dimensions do not match.");

            int width = Matrix1.Width;
            int height = Matrix1.Height;

            int plotSizeX = 256;
            int plotSizeY = 256;

            Data2D result = new Data2D(plotSizeX, plotSizeY);

	        //sum[0][]...sumX
	        //sum[1][]...sumXY
	        //sum[2][]...sumXX
	        //sum[3][]...sumYY
	        //sum[4][]...sumY	        
            double[,] sum = new double[5, 5];
            
	        //n[0]...( R==sat || R==0 || G==sat || G==0)
	        //n[1]...( 0<R<p && 0<G<q)
	        //n[2]...( R>p &&  0<G<q )
	        //n[3]...( 0<R<p && G>q ) 
	        //n[4]...( R>p && G>q ) 	        
            int[] n = new int[5];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int z1 = (int)Matrix1[x, y];
                    int z3 = (int)Matrix2[x, y];

                    int k = 0;
                    if (z1 == 0 || z3 == 0 || z1 == saturation || z3 == saturation) k = 0;
                    else if ((z1 > 0 && z1 <= Threshold1) && (z3 > 0 && z3 <= Threshold2)) k = 1;
                    else if ((z1 > Threshold1 && z1 < saturation) && (z3 > 0 && z3 <= Threshold2)) k = 2;
                    else if ((z1 > 0 && z1 <= Threshold1) && (z3 > Threshold2 && z3 < saturation)) k = 3;
                    else if ((z1 > Threshold1 && z1 < saturation) && (z3 > Threshold2 && z3 < saturation)) k = 4;

                    n[k]++;

                    sum[0, k] += z1;
                    sum[1, k] += (z1 * z3);
                    sum[2, k] += (z1 * z1);
                    sum[3, k] += (z3 * z3);
                    sum[4, k] += z3;

                    int z4 = plotSizeY - 1 - z3;

                    //float count = result[z1, z4];
                    //if (count < 65535) count++;
                    //result[z1, z4] = count;
                    result[z1, z4]++;
                }
            }

            //SUM[][0], N[0], r[0]... total
            //SUM[][1], N[1], r[1]... ( 0<R<sat ) && ( 0<G<sat )
            //SUM[][2], N[2], r[2]... ( p<R<sat && 0<G<sat )
            //SUM[][3], N[3], r[3]... ( 0<R<sat && q<G<sat )
            //SUM[][4], N[4], r[4]... ( p<R<sat || q<G<sat )
            double[,] SUM = new double[5, 5];
            double[] N = new double[5];

            N[0] = n[0] + n[1] + n[2] + n[3] + n[4];
            N[1] = n[1] + n[2] + n[3] + n[4];
            N[2] = n[2] + n[4];
            N[3] = n[3] + n[4];
            N[4] = n[2] + n[3] + n[4];

            for (int i = 0; i <= 4; i++) SUM[i, 0] = sum[i, 0] + sum[i, 1] + sum[i, 2] + sum[i, 3] + sum[i, 4];
            for (int i = 0; i <= 4; i++) SUM[i, 1] = sum[i, 1] + sum[i, 2] + sum[i, 3] + sum[i, 4];
            for (int i = 0; i <= 4; i++) SUM[i, 2] = sum[i, 2] + sum[i, 4];
            for (int i = 0; i <= 4; i++) SUM[i, 3] = sum[i, 3] + sum[i, 4];
            for (int i = 0; i <= 4; i++) SUM[i, 4] = sum[i, 2] + sum[i, 3] + sum[i, 4];

            double[] pearsons1 = new double[5];
	        double[] pearsons2 = new double[5];
	        double[] pearsons3 = new double[5];
		
	        double[] meanX = new double[5];
	        double[] meanY = new double[5];
	        double[] covXY = new double[5];
	        double[] sdX = new double[5];
	        double[] sdY = new double[5];
	        double[] r = new double[5];
	
	        double[] overlapC = new double[5];
	        double M1p = 0;
            double M2q = 0;

            for (int i = 0; i <= 4; i++)
            {
                meanX[i] = SUM[0,i] / N[i];// mean of red
                meanY[i] = SUM[4,i] / N[i];// mean of green
                covXY[i] = (SUM[1,i] - (SUM[0,i] * SUM[4,i] / N[i])) / N[i];// covariance of red and green
                sdX[i] = Math.Sqrt((SUM[2,i] - (SUM[0,i] * SUM[0,i] / N[i])) / N[i]);// standard deviation of red
                sdY[i] = Math.Sqrt((SUM[3,i] - (SUM[4,i] * SUM[4,i] / N[i])) / N[i]);// standard deviation of green
                r[i] = covXY[i] / (sdX[i] * sdY[i]);// Pearson's r for red and green

                overlapC[i] = SUM[1,i] / Math.Sqrt(SUM[2,i] * SUM[3,i]); //Manders' overlap coefficient
            }

            M1p = (sum[0,3] + sum[0,4]) / (sum[0,1] + sum[0,2] + sum[0,3] + sum[0,4]);
            M2q = (sum[4,2] + sum[4,4]) / (sum[4,1] + sum[4,2] + sum[4,3] + sum[4,4]);// Manders' colocalization coefficients
            // Following points are modified from the original calculation (Manders et al., 1992).
            // This modification is reasonable to apply the coefficients to our analysis.
            // (i)  This calculation exclude pixels that have 0 or saturated intensity (since they have no linear information).
            // (ii) Thershold value p and q is used, instead of 0 intensity, to determin positive pixels.
            // Thus, if p and q are both 0, M1p and M2q are always both 1.0.

            double[] numeratorICQ = new double[5];

            // ICQ calcuration for each fraction

          for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int z1 = (int)Matrix1[x,y]; // z-value of Threshold1ixel (x,y)in red image	
                    int z3 = (int)Matrix2[x,y]; // z-value of pixel (x,y)in green image

                        if (z1 == 0 || z3 == 0 || z1 == saturation || z3 == saturation)
                        {
                            //k=0;
                            if ((z1 - meanX[0]) * (z3 - meanY[0]) > 0) numeratorICQ[0]++;
                            else if ((z1 - meanX[0]) * (z3 - meanY[0]) < 0) numeratorICQ[0]--;
                        }
                        else if ((z1 > 0 && z1 <= Threshold1) && (z3 > 0 && z3 <= Threshold2))
                        {
                            //k=1;
                            if ((z1 - meanX[0]) * (z3 - meanY[0]) > 0) numeratorICQ[0]++;
                            else if ((z1 - meanX[0]) * (z3 - meanY[0]) < 0) numeratorICQ[0]--;
                            if ((z1 - meanX[1]) * (z3 - meanY[1]) > 0) numeratorICQ[1]++;
                            else if ((z1 - meanX[1]) * (z3 - meanY[1]) < 0) numeratorICQ[1]--;
                        }
                        else if ((z1 > Threshold1 && z1 < saturation) && (z3 > 0 && z3 <= Threshold2))
                        {
                            //k=2;
                            if ((z1 - meanX[0]) * (z3 - meanY[0]) > 0) numeratorICQ[0]++;
                            else if ((z1 - meanX[0]) * (z3 - meanY[0]) < 0) numeratorICQ[0]--;
                            if ((z1 - meanX[1]) * (z3 - meanY[1]) > 0) numeratorICQ[1]++;
                            else if ((z1 - meanX[1]) * (z3 - meanY[1]) < 0) numeratorICQ[1]--;
                            if ((z1 - meanX[2]) * (z3 - meanY[2]) > 0) numeratorICQ[2]++;
                            else if ((z1 - meanX[2]) * (z3 - meanY[2]) < 0) numeratorICQ[2]--;
                            if ((z1 - meanX[4]) * (z3 - meanY[4]) > 0) numeratorICQ[4]++;
                            else if ((z1 - meanX[4]) * (z3 - meanY[4]) < 0) numeratorICQ[4]--;
                        }
                        else if ((z1 > 0 && z1 <= Threshold1) && (z3 > Threshold2 && z3 < saturation))
                        {
                            //k=3;
                            if ((z1 - meanX[0]) * (z3 - meanY[0]) > 0) numeratorICQ[0]++;
                            else if ((z1 - meanX[0]) * (z3 - meanY[0]) < 0) numeratorICQ[0]--;
                            if ((z1 - meanX[1]) * (z3 - meanY[1]) > 0) numeratorICQ[1]++;
                            else if ((z1 - meanX[1]) * (z3 - meanY[1]) < 0) numeratorICQ[1]--;
                            if ((z1 - meanX[3]) * (z3 - meanY[3]) > 0) numeratorICQ[3]++;
                            else if ((z1 - meanX[3]) * (z3 - meanY[3]) < 0) numeratorICQ[3]--;
                            if ((z1 - meanX[4]) * (z3 - meanY[4]) > 0) numeratorICQ[4]++;
                            else if ((z1 - meanX[4]) * (z3 - meanY[4]) < 0) numeratorICQ[4]--;
                        }
                        else if ((z1 > Threshold1 && z1 < saturation) && (z3 > Threshold2 && z3 < saturation))
                        {
                            //k=4;
                            if ((z1 - meanX[0]) * (z3 - meanY[0]) > 0) numeratorICQ[0]++;
                            else if ((z1 - meanX[0]) * (z3 - meanY[0]) < 0) numeratorICQ[0]--;
                            if ((z1 - meanX[1]) * (z3 - meanY[1]) > 0) numeratorICQ[1]++;
                            else if ((z1 - meanX[1]) * (z3 - meanY[1]) < 0) numeratorICQ[1]--;
                            if ((z1 - meanX[2]) * (z3 - meanY[2]) > 0) numeratorICQ[2]++;
                            else if ((z1 - meanX[2]) * (z3 - meanY[2]) < 0) numeratorICQ[2]--;
                            if ((z1 - meanX[3]) * (z3 - meanY[3]) > 0) numeratorICQ[3]++;
                            else if ((z1 - meanX[3]) * (z3 - meanY[3]) < 0) numeratorICQ[3]--;
                            if ((z1 - meanX[4]) * (z3 - meanY[4]) > 0) numeratorICQ[4]++;
                            else if ((z1 - meanX[4]) * (z3 - meanY[4]) < 0) numeratorICQ[4]--;
                        }
                        /*
                        n[k]++;// n[k]=n[k]+1
					
                        sum[0][k] += z1;// sum[0][k] = sum[0][k] + z1
                        sum[1][k] += (z1 * z3);
                        sum[2][k] += (z1 * z1);
                        sum[3][k] += (z3 * z3);
                        sum[4][k] += z3;
                        */

                }// for (int x=0; x<width; x++) 
            }//	for (int y=0; y<height; y++)

            double[] ICQ = new double[5];
	
	        ICQ[0] = numeratorICQ[0]/(2*N[0]);
	        ICQ[1] = numeratorICQ[1]/(2*N[1]);
	        ICQ[2] = numeratorICQ[2]/(2*N[2]);
	        ICQ[3] = numeratorICQ[3]/(2*N[3]);
	        ICQ[4] = numeratorICQ[4]/(2*N[4]);

            return result;
        }
    }
}

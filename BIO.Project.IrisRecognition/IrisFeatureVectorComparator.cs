﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BIO.Framework.Core.Comparator;
using BIO.Framework.Extensions.Emgu.FeatureVector;
using BIO.Framework.Extensions.Standard.Template;
using Emgu.CV.Structure;
using Emgu.CV;
using Emgu.CV.UI;
using BIO.Framework.Core;
using System.Collections;


namespace BIO.Project.IrisRecognition {
    
    class IrisFeatureVectorComparator : IFeatureVectorComparator<EmguGrayImageFeatureVector, EmguGrayImageFeatureVector> {

        public MatchingScore computeMatchingScore(EmguGrayImageFeatureVector extracted, EmguGrayImageFeatureVector templated) {
            Image<Gray, byte> m1 = extracted.FeatureVector.Clone();
            Image<Gray, byte> m2 = templated.FeatureVector.Clone();
            var rotatedM2 = m2.Rotate(90, new Gray(255), false);
            var rotatedM1 = m1.Rotate(90, new Gray(255), false);
            var rotatedM1Original = m1.Rotate(90, new Gray(255), false);

            //m1 = m1.AbsDiff(m2);
            
            double sum = 0;
            byte[,,] data = m1.Rotate(90, new Gray(255), false).Data;

            Matrix<byte> transaltionMatrix = new Matrix<byte>(1,1);
            transaltionMatrix.SetZero();

            /*
             * bool firstBit = bits.Get(0);
            
            for(int i = 1; i < bits.Length; i++)
            {
                bits[i - 1] = bits[i];
            }
            bits[bits.Length-1] = firstBit;
            return bits;
             */


            //for and shifting
            /*double maxSum = m1.DotProduct(m2) / (m1.Norm * m2.Norm) * 1000; 
            for (int i = 1; i < m1.Cols; i++)
            {
                Array.Copy(m1.Data, 0, m1.Data, 1, m1.Cols * m1.Rows -1 );
                Array.Copy(m1.Data, m1.Cols * m1.Rows - 1, m1.Data, 0, 1);

                sum = m1.DotProduct(m2) / (m1.Norm * m1.Norm) * 1000;
                if (sum < maxSum)
                    maxSum = sum;


                //m1.Save(@"C:\Users\archie\Desktop\CASIA-IrisV1\SHIFTED" +i +".jpg");
                i = i;
            }*/

            BitArray bitsExtracted = new BitArray(m1.Bytes);
            BitArray bitsXored = new BitArray(m1.Bytes);
            BitArray bitsTemplated = new BitArray(m2.Bytes);

            bitsXored.Xor(bitsTemplated);
            double maxSum = countOfBitsSet(bitsXored);
            sum = maxSum;
            for (int i = 1; i < m1.Cols; i++)
            {
                Array.Copy(m1.Data, 0, m1.Data, 1, m1.Cols * m1.Rows -1 );
                Array.Copy(m1.Data, m1.Cols * m1.Rows - 1, m1.Data, 0, 1);

                bitsXored = new BitArray(m1.Bytes);
                bitsXored.Xor(bitsTemplated);
                sum = countOfBitsSet(bitsXored);

                if (sum < maxSum)
                    maxSum = sum;

                //m1.Save(@"C:\Users\archie\Desktop\CASIA-IrisV1\SHIFTED" +i +".jpg");
                i = i;
            }
            

            return new MatchingScore(maxSum);

            /************************************************************/
            String extractedIrisCode = this.getIrisCode(extracted);
            String templatedIrisCode = this.getIrisCode(templated);
            

            double minimalHamming = 1000.0;
            double actualHamming = 0.0;
            double suma = 0.0;
            for(int i = 0; i <= extractedIrisCode.Length; i++)
            {
                actualHamming = 0.0;
                suma = 0.0;
                for(int j = 0; j < templatedIrisCode.Length; j++)
                {
                    //XOR
                    if((extractedIrisCode[j] == '1' && templatedIrisCode[j] == '0') || (extractedIrisCode[j] == '0' && templatedIrisCode[j] == '1'))
                    {
                        suma++;
                    }
                }
                actualHamming = suma / templatedIrisCode.Length;
                if(actualHamming < minimalHamming)
                {
                    minimalHamming = actualHamming;
                }
                extractedIrisCode = extractedIrisCode.Substring(1, extractedIrisCode.Length - 1) + extractedIrisCode.Substring(0, 1);
            }
            //Console.WriteLine(minimalHamming);
            return new MatchingScore(minimalHamming);

            /***********************************************************/

            //draw.ROI = new System.Drawing.Rectangle();
            //ImageViewer.Show(draw, sum.ToString());

            return new MatchingScore(sum);
        }

        private BitArray createFeatureVectorBits(Image<Gray, Byte> m1)
        {
            IntPtr complexImage = CvInvoke.cvCreateImage(m1.Size, Emgu.CV.CvEnum.IPL_DEPTH.IPL_DEPTH_32F, 2);
            CvInvoke.cvSetZero(complexImage);
            CvInvoke.cvSetImageCOI(complexImage, 1);
            CvInvoke.cvCopy(m1.Convert<Gray, float>(), complexImage, IntPtr.Zero);
            CvInvoke.cvSetImageCOI(complexImage, 0);
            Matrix<float> dft = new Matrix<float>(m1.Rows, m1.Cols, 2);
            CvInvoke.cvDFT(complexImage, dft, Emgu.CV.CvEnum.CV_DXT.CV_DXT_FORWARD, 0);

            Image<Gray, float> outReal = new Image<Gray, float>(m1.Size);
            Image<Gray, float> outImg = new Image<Gray, float>(m1.Size);

            CvInvoke.cvSplit(dft, outReal, outImg, IntPtr.Zero, IntPtr.Zero);

            //FOr cely obrazok, zistit znamienko img a real podla toho urob 1,2,3,4
            BitArray fVector = new BitArray(m1.Rows * m1.Cols * 2);

            float[,,] dataImg = outReal.Data;
            float[,,] dataReal = outImg.Data;

            for (int i = 0; i < m1.Rows; i++)
            {

                for (int j = 0; j < m1.Cols; j++)
                {
                    if (dataImg[i, j, 0] < 0)
                        fVector.Set(2 * (i * m1.Rows + j), false);
                    else
                        fVector.Set(2 * (i * m1.Rows + j), true);
                    if (dataReal[i, j, 0] < 0)
                        fVector.Set(2 * (i * m1.Rows + j) + 1, false);
                    else
                        fVector.Set(2 * (i * m1.Rows + j) + 1, true);
                }
            }
            return fVector;
        }

        public static Int32 countOfBitsSet(BitArray bitArray)
        {

            Int32[] ints = new Int32[(bitArray.Count >> 5) + 1];

            bitArray.CopyTo(ints, 0);

            Int32 count = 0;

            // fix for not truncated bits in last integer that may have been set to true with SetAll()
            ints[ints.Length - 1] &= ~(-1 << (bitArray.Count % 32));

            for (Int32 i = 0; i < ints.Length; i++)
            {

                Int32 c = ints[i];

                // magic (http://graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel)
                unchecked
                {
                    c = c - ((c >> 1) & 0x55555555);
                    c = (c & 0x33333333) + ((c >> 2) & 0x33333333);
                    c = ((c + (c >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
                }

                count += c;

            }

            return count;

        }


        public BitArray rotateBits(BitArray bits)
        {
            bool firstBit = bits.Get(0);
            
            for(int i = 1; i < bits.Length; i++)
            {
                bits[i - 1] = bits[i];
            }
            bits[bits.Length-1] = firstBit;
            return bits;
        }

        private String getIrisCode(EmguGrayImageFeatureVector vector)
        {
            Image<Gray, byte> rotatedPolar = vector.FeatureVector.Clone();
            //Iris code generation
            String irisCode = "";
            Image<Gray, Byte> area = rotatedPolar.Copy();
            String pixelValue = "0";
            int blackCounter = 0;
            int whiteCounter = 0;
            for (int i = 0; i < 70; i++)
            {
                blackCounter = 0;
                whiteCounter = 0;
                CvInvoke.cvSetImageROI(rotatedPolar, new System.Drawing.Rectangle(new System.Drawing.Point(4 * i, 0), new System.Drawing.Size(4, area.Height)));
                CvInvoke.cvCopy(rotatedPolar, rotatedPolar, new IntPtr(0));
                area = rotatedPolar.Copy();
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < area.Height; y++)
                    {
                        pixelValue = area.Data[y, x, 0].ToString();
                        if (Int32.Parse(pixelValue) > 170)
                        {
                            blackCounter++;
                        }
                        else
                        {
                            whiteCounter++;
                        }
                    }
                }
                if (blackCounter > whiteCounter)
                {
                    irisCode += "0";
                }
                else
                {
                    irisCode += "1";
                }
                CvInvoke.cvResetImageROI(rotatedPolar);
            }
            return irisCode;
        }
        
    }

    

       
}

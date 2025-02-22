using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.NetworkInformation;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.ImageFile;
using INIOperationClass;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
using MvCamCtrl.NET;
using System.Runtime.InteropServices;


namespace WindowsFormsApplication2
{
   public class HKTOVP
    {
        MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList;
        private MyCamera m_pMyCamera;
        bool m_bGrabbing;
        byte[] m_pDataForRed = new byte[20 * 1024 * 1024];
        byte[] m_pDataForGreen = new byte[20 * 1024 * 1024];
        byte[] m_pDataForBlue = new byte[20 * 1024 * 1024];
        UInt32 g_nPayloadSize = 0;
        UInt32 m_nRowStep = 0;
        public string 相机属性="";
        public ICogImage HKImage;
        public  int HKNum;
        bool camstation = false;
        public bool pictuer = false;
      
        #region 海康相机
        public  void DeviceListAcq()//查找设备
        {
            相机属性 = "";
            int nRet;
            System.GC.Collect();
            nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Enum Devices Fail");
                return;
            }
            HKNum =Convert.ToInt16( m_pDeviceList.nDeviceNum);
            // ch:在窗体列表中显示设备名 || Display the device'name on window's list
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));


                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                    MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                    if (gigeInfo.chUserDefinedName != "")
                    {
                        相机属性="GigE: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")";
                    }
                    else
                    {
                        相机属性="GigE: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")";
                    }
                }
                else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                {
                    IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                    MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                    if (usbInfo.chUserDefinedName != "")
                    {
                        相机属性="USB: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")";
                    }
                    else
                    {
                        相机属性="USB: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")";
                    }
                }
            }
        }

        public  void ConnectHK(int num)
        {

            if (m_pDeviceList.nDeviceNum == 0)
            {
                MessageBox.Show("No device,please select");
                return;
            }
            int nRet = -1;
           
            //ch:获取选择的设备信息 | en:Get selected device information
            MyCamera.MV_CC_DEVICE_INFO device =
                (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[num],
                                                              typeof(MyCamera.MV_CC_DEVICE_INFO));
            m_pMyCamera = new MyCamera();
            nRet = m_pMyCamera.MV_CC_CreateDevice_NET(ref device);
            if (MyCamera.MV_OK != nRet)
            {
                return;
            }

            // ch:打开设备 | en:Open device
            nRet = m_pMyCamera.MV_CC_OpenDevice_NET();
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Open Device Fail");
                return;
            }

            // ch:获取包大小 || en: Get Payload Size
            MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
            nRet = m_pMyCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Get PayloadSize Fail");
                return;
            }
            g_nPayloadSize = stParam.nCurValue;

            // ch:获取高 || en: Get Height
            nRet = m_pMyCamera.MV_CC_GetIntValue_NET("Height", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Get Height Fail");
                return;
            }
            uint nHeight = stParam.nCurValue;

            // ch:获取宽 || en: Get Width
            nRet = m_pMyCamera.MV_CC_GetIntValue_NET("Width", ref stParam);
            if (MyCamera.MV_OK != nRet)
            {
                MessageBox.Show("Get Width Fail");
                return;
            }
            uint nWidth = stParam.nCurValue;

            // ch:获取步长 || en: Get nRowStep
            m_nRowStep = nWidth * nHeight;

            // ch:设置触发模式为off || en:set trigger mode as off
            m_pMyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", 2);//ExposureAuto
            //GainAuto
            m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);

        }
        public void start()
        {
           /* int nRet1 = MyCamera.MV_OK;
            nRet1 = m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 1);
            if (nRet1 != MyCamera.MV_OK)
            {
                MessageBox.Show("Set TriggerMode Fail");
                return;
            }*/
            int nRet2;
            // ch:开启抓图 | en:start grab
            nRet2 = m_pMyCamera.MV_CC_StartGrabbing_NET();
            if (MyCamera.MV_OK != nRet2)
            {
                MessageBox.Show("Start Grabbing Fail");
                return;
            }
            m_bGrabbing = true;
            Thread hReceiveImageThreadHandle = new Thread(ReceiveImageWorkThread);
            hReceiveImageThreadHandle.IsBackground = true;
            hReceiveImageThreadHandle.Start(m_pMyCamera);
           // ReceiveImageWorkThread(m_pMyCamera);
        }
       
        public void ReceiveImageWorkThread(object obj)
        {
                int nRet = MyCamera.MV_OK;
                MyCamera device = obj as MyCamera;
                MyCamera.MV_FRAME_OUT_INFO_EX pFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
                IntPtr pData = Marshal.AllocHGlobal((int)g_nPayloadSize);
                if (pData == IntPtr.Zero)
                {
                    return;
                }
                IntPtr pImageBuffer = Marshal.AllocHGlobal((int)m_nRowStep * 3);
                if (pImageBuffer == IntPtr.Zero)
                {
                    return;
                }

                IntPtr pTemp = IntPtr.Zero;
                Byte[] byteArrImageData = new Byte[m_nRowStep * 3];
               
                while (m_bGrabbing)
                {

                    nRet = device.MV_CC_GetOneFrameTimeout_NET(pData, g_nPayloadSize, ref pFrameInfo, 1000);
                    camstation = true;
                    if (MyCamera.MV_OK == nRet)
                    {
                        MyCamera.MvGvspPixelType pixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
                        if (IsColorPixelFormat(pFrameInfo.enPixelType))    // 彩色图像处理
                        {
                            if (pFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed)
                            {
                                pTemp = pData;
                            }
                            else
                            {
                                // 其他格式彩色图像转为RGB
                                nRet = ConvertToRGB(obj, pData, pFrameInfo.nHeight, pFrameInfo.nWidth, pFrameInfo.enPixelType, pImageBuffer);
                                if (MyCamera.MV_OK != nRet)
                                {
                                    return;
                                }
                                pTemp = pImageBuffer;
                            }

                            // Packed转Plane
                            unsafe
                            {
                                byte* pBufForSaveImage = (byte*)pTemp;

                                UInt32 nSupWidth = (pFrameInfo.nWidth + (UInt32)3) & 0xfffffffc;

                                for (int nRow = 0; nRow < pFrameInfo.nHeight; nRow++)
                                {
                                    for (int col = 0; col < pFrameInfo.nWidth; col++)
                                    {
                                        byteArrImageData[nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col)];
                                        byteArrImageData[pFrameInfo.nWidth * pFrameInfo.nHeight + nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col + 1)];
                                        byteArrImageData[pFrameInfo.nWidth * pFrameInfo.nHeight * 2 + nRow * nSupWidth + col] = pBufForSaveImage[nRow * pFrameInfo.nWidth * 3 + (3 * col + 2)];
                                    }
                                }
                                pTemp = Marshal.UnsafeAddrOfPinnedArrayElement(byteArrImageData, 0);
                            }
                        }
                        else if (IsMonoPixelFormat(pFrameInfo.enPixelType))    // Mono图像处理
                        {
                            if (pFrameInfo.enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                            {
                                pTemp = pData;
                            }
                            else
                            {
                                // 其他格式Mono转为Mono8
                                nRet = ConvertToMono8(device, pData, pImageBuffer, pFrameInfo.nHeight, pFrameInfo.nWidth, pFrameInfo.enPixelType);
                                if (MyCamera.MV_OK != nRet)
                                {
                                    return;
                                }
                                pTemp = pImageBuffer;
                            }
                            pixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                        }
                        else
                        {
                            continue;
                        }
                        VisionProDisplay(pFrameInfo.nHeight, pFrameInfo.nWidth, pTemp, pixelType);
                    }
                    else
                    {
                        continue;
                    }
                }
                if (pData != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pData);
                }
                if (pImageBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pImageBuffer);
                }
                return;
        }

        private bool IsColorPixelFormat(MyCamera.MvGvspPixelType enType)
        {
            switch (enType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BGR8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGBA8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BGRA8_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsMonoPixelFormat(MyCamera.MvGvspPixelType enType)
        {
            switch (enType)
            {
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                    return true;
                default:
                    return false;
            }
        }

        public Int32 ConvertToMono8(object obj, IntPtr pInData, IntPtr pOutData, ushort nHeight, ushort nWidth, MyCamera.MvGvspPixelType nPixelType)
        {
            if (IntPtr.Zero == pInData || IntPtr.Zero == pOutData)
            {
                return MyCamera.MV_E_PARAMETER;
            }

            int nRet = MyCamera.MV_OK;
            MyCamera device = obj as MyCamera;
            MyCamera.MV_PIXEL_CONVERT_PARAM stPixelConvertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();

            stPixelConvertParam.pSrcData = pInData;//源数据
            if (IntPtr.Zero == stPixelConvertParam.pSrcData)
            {
                return -1;
            }

            stPixelConvertParam.nWidth = nWidth;//图像宽度
            stPixelConvertParam.nHeight = nHeight;//图像高度
            stPixelConvertParam.enSrcPixelType = nPixelType;//源数据的格式
            stPixelConvertParam.nSrcDataLen = (uint)(nWidth * nHeight * ((((uint)nPixelType) >> 16) & 0x00ff) >> 3);

            stPixelConvertParam.nDstBufferSize = (uint)(nWidth * nHeight * ((((uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed) >> 16) & 0x00ff) >> 3);
            stPixelConvertParam.pDstBuffer = pOutData;//转换后的数据
            stPixelConvertParam.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
            stPixelConvertParam.nDstBufferSize = (uint)(nWidth * nHeight * 3);

            nRet = device.MV_CC_ConvertPixelType_NET(ref stPixelConvertParam);//格式转换
            if (MyCamera.MV_OK != nRet)
            {
                return -1;
            }

            return nRet;
        }

        public Int32 ConvertToRGB(object obj, IntPtr pSrc, ushort nHeight, ushort nWidth, MyCamera.MvGvspPixelType nPixelType, IntPtr pDst)
        {
            if (IntPtr.Zero == pSrc || IntPtr.Zero == pDst)
            {
                return MyCamera.MV_E_PARAMETER;
            }

            int nRet = MyCamera.MV_OK;
            MyCamera device = obj as MyCamera;
            MyCamera.MV_PIXEL_CONVERT_PARAM stPixelConvertParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();

            stPixelConvertParam.pSrcData = pSrc;//源数据
            if (IntPtr.Zero == stPixelConvertParam.pSrcData)
            {
                return -1;
            }

            stPixelConvertParam.nWidth = nWidth;//图像宽度
            stPixelConvertParam.nHeight = nHeight;//图像高度
            stPixelConvertParam.enSrcPixelType = nPixelType;//源数据的格式
            stPixelConvertParam.nSrcDataLen = (uint)(nWidth * nHeight * ((((uint)nPixelType) >> 16) & 0x00ff) >> 3);

            stPixelConvertParam.nDstBufferSize = (uint)(nWidth * nHeight * ((((uint)MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed) >> 16) & 0x00ff) >> 3);
            stPixelConvertParam.pDstBuffer = pDst;//转换后的数据
            stPixelConvertParam.enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
            stPixelConvertParam.nDstBufferSize = (uint)nWidth * nHeight * 3;

            nRet = device.MV_CC_ConvertPixelType_NET(ref stPixelConvertParam);//格式转换
            if (MyCamera.MV_OK != nRet)
            {
                return -1;
            }

            return MyCamera.MV_OK;
        }

        public void VisionProDisplay(UInt32 nHeight, UInt32 nWidth, IntPtr pImageBuf, MyCamera.MvGvspPixelType enPixelType)
        {
            // ch: 显示 || display
            try
            {
                if (enPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                {
                    CogImage8Root cogImage8Root = new CogImage8Root();
                    cogImage8Root.Initialize((Int32)nWidth, (Int32)nHeight, pImageBuf, (Int32)nWidth, null);

                    CogImage8Grey cogImage8Grey = new CogImage8Grey();
                    cogImage8Grey.SetRoot(cogImage8Root);
                    HKImage = cogImage8Grey.ScaleImage((int)nWidth, (int)nHeight);
                    pictuer = true;
                    System.GC.Collect();
                }
                else
                {
                    CogImage8Root image0 = new CogImage8Root();
                    IntPtr ptr0 = new IntPtr(pImageBuf.ToInt64());
                    image0.Initialize((int)nWidth, (int)nHeight, ptr0, (int)nWidth, null);

                    CogImage8Root image1 = new CogImage8Root();
                    IntPtr ptr1 = new IntPtr(pImageBuf.ToInt64() + m_nRowStep);
                    image1.Initialize((int)nWidth, (int)nHeight, ptr1, (int)nWidth, null);

                    CogImage8Root image2 = new CogImage8Root();
                    IntPtr ptr2 = new IntPtr(pImageBuf.ToInt64() + m_nRowStep * 2);
                    image2.Initialize((int)nWidth, (int)nHeight, ptr2, (int)nWidth, null);

                    CogImage24PlanarColor colorImage = new CogImage24PlanarColor();
                    colorImage.SetRoots(image0, image1, image2);

                    HKImage = colorImage.ScaleImage((int)nWidth, (int)nHeight);
                    pictuer = true;
                    System.GC.Collect();
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return;
            }
            return;
        }

        public void RunOnce(int 相机曝光) //
        {
            if (!m_bGrabbing)
            {
                m_pMyCamera.MV_CC_StopGrabbing_NET();
                m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 1);
                start();
                while (!camstation)
                {
                    Thread.Sleep(20);
                }
                m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerSource", 7);
            }
            pictuer = false;
            Thread.Sleep(50);
            m_pMyCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);
            m_pMyCamera.MV_CC_SetFloatValue_NET("ExposureTime", 相机曝光);
            m_pMyCamera.MV_CC_SetCommandValue_NET("TriggerSoftware");
        }

        public void CloseHK()//关闭相机
        {
            try
            {
                m_pMyCamera.MV_CC_StopGrabbing_NET();
                m_pMyCamera.MV_CC_CloseDevice_NET();
            }
            catch (Exception ex)
            {

            }
          
        }
        public void RunContinuing()
        {
            m_pMyCamera.MV_CC_StopGrabbing_NET();
            int nRet = MyCamera.MV_OK;
            nRet = m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);
            start();
        }
        public void stopRun()
        {
           
            int nRet = -1;
            nRet = m_pMyCamera.MV_CC_StopGrabbing_NET();
            m_bGrabbing = false;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
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
using Microsoft.Kinect;
namespace Kinect
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        KinectSensor miKinect;
        public int[][] mapa = new int[480][];
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                MessageBox.Show("No se detecta ningun kinect", "Visor de Camara");
                Application.Current.Shutdown();
            }
            try
            {
                //Console.WriteLine("Inicio");
                miKinect = KinectSensor.KinectSensors.FirstOrDefault();
                miKinect.DepthStream.Enable();
                miKinect.Start();
                //Console.WriteLine("Start");
                miKinect.DepthFrameReady += miKinect_DepthFrameReady;
                //Console.WriteLine("+=");

            }
            catch { }
        }

        short[] datosDistancia = null;
        byte[] colorImagenDistancia = null;
        WriteableBitmap bitmapImagenDistancia = null;

        void miKinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        //void miKinect_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame framesDistancia = e.OpenDepthImageFrame())
            {
                if (framesDistancia == null)
                {
                    Console.WriteLine("Return");
                    return;
                }
                if (datosDistancia == null)
                    datosDistancia = new short[framesDistancia.PixelDataLength];
               // Console.WriteLine("framesDistancia.PixelDataLength: " + framesDistancia.PixelDataLength);
                if (colorImagenDistancia == null)
                    colorImagenDistancia = new byte[framesDistancia.PixelDataLength * 4];
                //Console.WriteLine("Inicio");
                framesDistancia.CopyPixelDataTo(datosDistancia);
                int posColorImagenDistancia = 0;
                mapa[0] = new int[640];
                int j = 0;
                int k = 0;
                for (int i = 0; i < framesDistancia.PixelDataLength; i++)
                {
                    int valorDistancia = datosDistancia[i] >> 3;
                    // Console.WriteLine("valorDistancia: " + valorDistancia);
                    if (valorDistancia == miKinect.DepthStream.UnknownDepth)
                     {
                         colorImagenDistancia[posColorImagenDistancia++] = 0;
                         colorImagenDistancia[posColorImagenDistancia++] = 0;
                         colorImagenDistancia[posColorImagenDistancia++] = 255; //Rojo
                     }
                     else if (valorDistancia == miKinect.DepthStream.TooFarDepth)
                     {
                         colorImagenDistancia[posColorImagenDistancia++] = 255; //Azul
                         colorImagenDistancia[posColorImagenDistancia++] = 0;
                         colorImagenDistancia[posColorImagenDistancia++] = 0;
                     }
                     else if (valorDistancia == miKinect.DepthStream.TooNearDepth)
                     {
                         colorImagenDistancia[posColorImagenDistancia++] = 0;
                         colorImagenDistancia[posColorImagenDistancia++] = 255;//Verde
                         colorImagenDistancia[posColorImagenDistancia++] = 0;
                     }
                     else
                     {
                         byte byteDistancia = (byte)(255 - (valorDistancia >> 5));
                         //Console.WriteLine("byteDistancia: " + byteDistancia);
                        // System.Threading.Thread.Sleep(1000);
                         colorImagenDistancia[posColorImagenDistancia++] = byteDistancia;
                         colorImagenDistancia[posColorImagenDistancia++] = byteDistancia;
                         colorImagenDistancia[posColorImagenDistancia++] = byteDistancia;
                     }

                    /*if(valorDistancia < 800 && valorDistancia != -1)
                    {
                        colorImagenDistancia[posColorImagenDistancia++] = 0;
                        colorImagenDistancia[posColorImagenDistancia++] = 255;//Verde
                        colorImagenDistancia[posColorImagenDistancia++] = 0;
                    }
                    else
                    {
                     
                            byte byteDistancia = (byte)(255 - (valorDistancia >> 5));
                            //Console.WriteLine("byteDistancia: " + byteDistancia);
                            // System.Threading.Thread.Sleep(1000);
                            colorImagenDistancia[posColorImagenDistancia++] = byteDistancia;
                            colorImagenDistancia[posColorImagenDistancia++] = byteDistancia;
                            colorImagenDistancia[posColorImagenDistancia++] = byteDistancia;
                        
                    }*/
                    mapa[j][k] = valorDistancia; //j=480 x k=640
                    //Calculo de j,k
                    k++;
                    if ((i + 1) % 640 == 0)
                    {
                        j++;
                        if(j != 480)
                            mapa[j] = new int[640];
                        k = 0;  
                    }
                    if(j % 480 == 0)
                    {
                        j = 0;
                    }
                    //Fin Calculo de j,k
                    posColorImagenDistancia++;

                }
                
                int x = 0;
                int y = 0;
                int w = 1;
                bool encontro = false;
                while (y< 480 && !encontro)//480
                {                   
                    while (x < 640 && !encontro)
                    {
                        Punto centro = new Punto
                        {
                            z = mapa[y][x],
                            x = x,
                            y = y
                        };
                        Punto externo = new Punto();
                        if ((x + w) < 640)
                        {
                            externo.x = x + w;
                            externo.y = y;
                            externo.z = mapa[externo.y][externo.x];
                        }
                        int diferenciaAlCentro = externo.z - centro.z;
                        while ((x + w) < 640 && diferenciaAlCentro >= 0 && diferenciaAlCentro <= 8)
                        {

                            externo.x = x + w;
                            externo.y = y;
                            externo.z = mapa[externo.y][externo.x];
                            /*
                            if (diferenciaAlCentro >= 8)
                            {
                                //if (recorrerDerecha(original, centro, mapa))
                                // Console.WriteLine("ENCONTRO");

                                //Console.WriteLine("ENCONTRO X");
                                if (recorrerArriba(centro, mapa) && recorrerAbajo(centro, mapa))
                                {
                                    Console.WriteLine("ENCONTRO Y");
                                    encontro = true;
                                }

                                // if (recorrerDerecha(original, centro, mapa) && recorrerArriba(original, centro, mapa))
                                 //   Console.WriteLine("ENCONTRO");
                            }*/
                            w++;
                            diferenciaAlCentro = externo.z - centro.z;
                        }
                        if (diferenciaAlCentro > 8 && diferenciaAlCentro <17)
                        {
                            int pixeles = Math.Abs(externo.x - centro.x);
                            if (recorrerArriba(centro, mapa, pixeles) && recorrerAbajo (centro, mapa, pixeles) && recorrerIzquierda(centro,mapa, pixeles) && recorrerArribaDerecha(centro, mapa, pixeles) && recorrerArribaIzquierda(centro, mapa, pixeles) && recorrerAbajoIzquirda(centro,mapa,pixeles))
                           //if (recorrerIzquierda(centro, mapa, pixeles))
                            {
                                Console.WriteLine("ENCONTRO");
                                encontro = true;
                               // PrintText();
                            }
                        }
                        w = 1;
                        x++;
                    }
                    x = 0;
                    y++;
                }
                

                if (bitmapImagenDistancia == null)
                {
                    this.bitmapImagenDistancia = new WriteableBitmap(
                        framesDistancia.Width,
                        framesDistancia.Height,
                        96,
                        96,
                        PixelFormats.Bgr32,
                        null);
                    DistanciaKinect.Source = bitmapImagenDistancia;
                }

                this.bitmapImagenDistancia.WritePixels(
                    new Int32Rect(0, 0, framesDistancia.Width, framesDistancia.Height),
                    colorImagenDistancia,
                    framesDistancia.Width * 4,
                    0
                    );
                }
        }

        public Boolean recorrerIzquierda(Punto centro, int[][] mapa, int pixeles)
        {
            Punto extremo = new Punto();
            if (centro.x - 1 < 0)
                return false;

            extremo.x = centro.x - 1;
            extremo.y = centro.y;
            extremo.z = mapa[extremo.y][extremo.x];

            int diferenciaAlCentro = extremo.z - centro.z;
            int pixelesLocal = Math.Abs(extremo.x - centro.x);
            int diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);
            // while(anterior.z < proximo.z && proximo.z - centro.z <=23)
            while (diferenciaAlCentro >= 0 && diferenciaAlCentro <= 8 && diferenciaEntrePixeles <2)
            {

                if (extremo.x - 1 < 0)
                    return false;

                extremo.x--;
                extremo.z = mapa[extremo.y][extremo.x];
                pixelesLocal = Math.Abs(extremo.x - centro.x);
                diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);

            }
            if (diferenciaAlCentro >= 8 && diferenciaAlCentro <= 17 && diferenciaEntrePixeles < 2)
                return true;

            return false;
        }

        public Boolean recorrerArriba(Punto centro, int[][] mapa, int pixeles)
        {
            Punto externo = new Punto();
            if (centro.y - 1 < 0)
                return false;

            externo.x = centro.x;
            externo.y = centro.y-1;
            externo.z = mapa[externo.y][externo.x];

            int diferenciaAlCentro = externo.z - centro.z;
            int pixelesLocal = Math.Abs(externo.x - centro.x);
            int diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);
            //while (anterior.z < proximo.z && proximo.z - centro.z <= 23)
            while (diferenciaAlCentro >= 0 && diferenciaAlCentro <= 8 && diferenciaEntrePixeles < 2)
            {
                if (externo.y - 1 < 0)
                    return false;

                externo.y--;
                externo.z = mapa[externo.y][externo.x];
                diferenciaAlCentro = externo.z - centro.z;
                pixelesLocal = Math.Abs(externo.x - centro.x);
                diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);

            }

            if (diferenciaAlCentro >= 8 && diferenciaAlCentro <= 17 && diferenciaEntrePixeles < 2)
                return true;

            return false;
        }

        public Boolean recorrerAbajo(Punto centro, int[][] mapa, int pixeles)
        {
            Punto externo = new Punto();
            if (centro.y + 1 > 479)
                return false;

            externo.x = centro.x;
            externo.y = centro.y + 1;
            externo.z = mapa[externo.y][externo.x];

            int diferenciaAlCentro = externo.z - centro.z;
            int pixelesLocal = Math.Abs(externo.x - centro.x);
            int diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);
            //while (anterior.z < proximo.z && proximo.z - centro.z <= 23)
            while (diferenciaAlCentro >= 0 && diferenciaAlCentro <= 8 && diferenciaEntrePixeles < 2)
            {

                if (externo.y + 1 > 479)
                    return false;

                externo.y++;
                externo.z = mapa[externo.y][externo.x];
                diferenciaAlCentro = externo.z - centro.z;
                pixelesLocal = Math.Abs(externo.x - centro.x);
                diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);

            }

            if (diferenciaAlCentro >= 8 && diferenciaAlCentro <= 17 && diferenciaEntrePixeles < 2)
                return true;

            return false;
        }

        public Boolean recorrerArribaDerecha(Punto centro, int[][] mapa, int pixeles)
        {
            Punto externo = new Punto();
            if (centro.y - 1 < 0 || centro.x + 1 > 639)
                return false;

            externo.x = centro.x + 1;
            externo.y = centro.y - 1;
            externo.z = mapa[externo.y][externo.x];

            int diferenciaAlCentro = externo.z - centro.z;
            int pixelesLocal = Math.Abs(externo.x - centro.x);
            int diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);
            //while (anterior.z < proximo.z && proximo.z - centro.z <= 23)
            while (diferenciaAlCentro >= 0 && diferenciaAlCentro <= 8 && diferenciaEntrePixeles < 2)
            {
                if (externo.y - 1 < 0 || externo.x + 1 > 639)
                    return false;

                externo.y--;
                externo.x++;
                externo.z = mapa[externo.y][externo.x];
                diferenciaAlCentro = externo.z - centro.z;
                pixelesLocal = Math.Abs(externo.x - centro.x);
                diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);

            }

            if (diferenciaAlCentro >= 8 && diferenciaAlCentro <= 17 && diferenciaEntrePixeles < 2)
                return true;

            return false;
        }

        public Boolean recorrerArribaIzquierda(Punto centro, int[][] mapa, int pixeles)
        {
            Punto externo = new Punto();
            if (centro.y - 1 < 0 || centro.x - 1 < 0)
                return false;

            externo.x = centro.x - 1;
            externo.y = centro.y - 1;
            externo.z = mapa[externo.y][externo.x];

            int diferenciaAlCentro = externo.z - centro.z;
            int pixelesLocal = Math.Abs(externo.x - centro.x);
            int diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);
            //while (anterior.z < proximo.z && proximo.z - centro.z <= 23)
            while (diferenciaAlCentro >= 0 && diferenciaAlCentro <= 8 && diferenciaEntrePixeles < 2)
            {
                if (externo.y - 1 < 0 || externo.x - 1 < 0)
                    return false;

                externo.y--;
                externo.x--;
                externo.z = mapa[externo.y][externo.x];
                diferenciaAlCentro = externo.z - centro.z;
                pixelesLocal = Math.Abs(externo.x - centro.x);
                diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);

            }

            if (diferenciaAlCentro >= 8 && diferenciaAlCentro <= 17 && diferenciaEntrePixeles < 2)
                return true;

            return false;
        }
        public Boolean recorrerAbajoIzquirda(Punto centro, int[][] mapa, int pixeles)
        {
            Punto externo = new Punto();
            if (centro.y + 1 > 479 || centro.x - 1 < 0)
                return false;

            externo.x = centro.x - 1;
            externo.y = centro.y + 1;
            externo.z = mapa[externo.y][externo.x];

            int diferenciaAlCentro = externo.z - centro.z;
            int pixelesLocal = Math.Abs(externo.x - centro.x);
            int diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);
            //while (anterior.z < proximo.z && proximo.z - centro.z <= 23)
            while (diferenciaAlCentro >= 0 && diferenciaAlCentro <= 8 && diferenciaEntrePixeles < 2)
            {
                if (externo.y + 1 > 479 || externo.x - 1 < 0)
                    return false;

                externo.y++;
                externo.x--;
                externo.z = mapa[externo.y][externo.x];
                diferenciaAlCentro = externo.z - centro.z;
                pixelesLocal = Math.Abs(externo.x - centro.x);
                diferenciaEntrePixeles = Math.Abs(pixelesLocal - pixeles);

            }

            if (diferenciaAlCentro >= 8 && diferenciaAlCentro <= 17 && diferenciaEntrePixeles < 2)
                return true;

            return false;
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(@"Mapa.txt", true))
            {
                int j = 0;
                while (j < 480)
                {
                    string linea = "";
                    for (int i = 0; i < 640; i++)
                    {
                        if(mapa[j][i] < 1000)
                        {
                            linea += "xxxx" + " ";
                        }
                        else
                        {
                            linea += mapa[j][i] + " ";
                        }
                        
                       
                    }
                    file.WriteLine(linea);
                    j++;
                }
            }
        }
    

    private void PrintText()
    {
        using (System.IO.StreamWriter file =
        new System.IO.StreamWriter(@"Mapa.txt", false))
        {
            int j = 0;
            while (j < 480)
            {
                string linea = "";
                for (int i = 0; i < 640; i++)
                {
                    if (mapa[j][i] < 1000)
                    {
                        linea += "xxxx" + " ";
                    }
                    else
                    {
                        linea += mapa[j][i] + " ";
                    }


                }
                file.WriteLine(linea);
                j++;
            }
        }
    }
}


public class Punto
    {
        public int x = 0;
        public int y = 0;
        public int z = 0;
    }
}

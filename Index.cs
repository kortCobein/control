using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace control
{
    public partial class Index : System.Windows.Forms.Form
    {
        // Relación de aspecto deseada (16:9) para el área interna (ClientSize)
        private const double AspectRatio = 16.0 / 9.0;

        // Constantes de Windows para interceptar el redimensionamiento nativo en tiempo real
        private const int WM_SIZING = 0x0214;

        private const int WMSZ_LEFT = 1;
        private const int WMSZ_RIGHT = 2;
        private const int WMSZ_TOP = 3;
        private const int WMSZ_TOPLEFT = 4;
        private const int WMSZ_TOPRIGHT = 5;
        private const int WMSZ_BOTTOM = 6;
        private const int WMSZ_BOTTOMLEFT = 7;
        private const int WMSZ_BOTTOMRIGHT = 8;

        public Index()
        {
            InitializeComponent();

            // Optimización de renderizado para evitar parpadeos y actualizar al redimensionar
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            // Asegura que la imagen de fondo siempre rellene todo el formulario
            this.BackgroundImageLayout = ImageLayout.Stretch;

            FormBorderStyle = FormBorderStyle.Sizable;

            // Tamaño inicial de la parte interna (960 x 540 = exactamente 16:9)
            ClientSize = new Size(960, 540);
        }

        // Estilo de ventana que activa doble buffer compuesto en Windows (evita parpadeo de controles)
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    cp.ExStyle |= 0x02000000; // WS_EX_COMPOSITED
                }
                return cp;
            }
        }

        // Intercepta los mensajes nativos de Windows antes de pintar en pantalla
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SIZING)
            {
                // Obtenemos el rectángulo propuesto por el movimiento del ratón
                RECT rect = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));

                // Calculamos el grosor de los bordes y la barra de título de la ventana
                int borderW = this.Width - this.ClientSize.Width;
                int borderH = this.Height - this.ClientSize.Height;

                int proposedClientW = (rect.Right - rect.Left) - borderW;
                int proposedClientH = (rect.Bottom - rect.Top) - borderH;

                // Límite mínimo para mantener la ventana utilizable
                if (proposedClientW < 640)
                {
                    proposedClientW = 640;
                    proposedClientH = (int)Math.Round(640 / AspectRatio);
                }

                int edge = m.WParam.ToInt32();

                switch (edge)
                {
                    case WMSZ_LEFT:
                    case WMSZ_RIGHT:
                        // Si se arrastra horizontalmente, el ancho determina el alto
                        proposedClientH = (int)Math.Round(proposedClientW / AspectRatio);
                        rect.Bottom = rect.Top + proposedClientH + borderH;
                        break;

                    case WMSZ_TOP:
                    case WMSZ_BOTTOM:
                        // Si se arrastra verticalmente, el alto determina el ancho
                        proposedClientW = (int)Math.Round(proposedClientH * AspectRatio);
                        rect.Right = rect.Left + proposedClientW + borderW;
                        break;

                    case WMSZ_BOTTOMRIGHT:
                        if (proposedClientW / AspectRatio > proposedClientH)
                        {
                            proposedClientH = (int)Math.Round(proposedClientW / AspectRatio);
                            rect.Bottom = rect.Top + proposedClientH + borderH;
                        }
                        else
                        {
                            proposedClientW = (int)Math.Round(proposedClientH * AspectRatio);
                            rect.Right = rect.Left + proposedClientW + borderW;
                        }
                        break;

                    case WMSZ_BOTTOMLEFT:
                        if (proposedClientW / AspectRatio > proposedClientH)
                        {
                            proposedClientH = (int)Math.Round(proposedClientW / AspectRatio);
                            rect.Bottom = rect.Top + proposedClientH + borderH;
                        }
                        else
                        {
                            proposedClientW = (int)Math.Round(proposedClientH * AspectRatio);
                            rect.Left = rect.Right - proposedClientW - borderW;
                        }
                        break;

                    case WMSZ_TOPLEFT:
                        if (proposedClientW / AspectRatio > proposedClientH)
                        {
                            proposedClientH = (int)Math.Round(proposedClientW / AspectRatio);
                            rect.Top = rect.Bottom - proposedClientH - borderH;
                        }
                        else
                        {
                            proposedClientW = (int)Math.Round(proposedClientH * AspectRatio);
                            rect.Left = rect.Right - proposedClientW - borderW;
                        }
                        break;

                    case WMSZ_TOPRIGHT:
                        if (proposedClientW / AspectRatio > proposedClientH)
                        {
                            proposedClientH = (int)Math.Round(proposedClientW / AspectRatio);
                            rect.Top = rect.Bottom - proposedClientH - borderH;
                        }
                        else
                        {
                            proposedClientW = (int)Math.Round(proposedClientH * AspectRatio);
                            rect.Right = rect.Left + proposedClientW + borderW;
                        }
                        break;
                }

                // Escribimos de vuelta el rectángulo ajustado a Windows
                Marshal.StructureToPtr(rect, m.LParam, true);
                m.Result = (IntPtr)1;
                return;
            }

            base.WndProc(ref m);
        }

        private void Index_Load(object sender, EventArgs e)
        {

        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}

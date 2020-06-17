using Gwen.Control;
using System;

namespace Gwen.UnitTest
{
    public class ImagePanel : GUnit
    {
        public ImagePanel(Base parent)
            : base(parent)
        {
            /* Normal */
            {
                Control.ImagePanel img = new Control.ImagePanel(this)
                {
                    ImageName = "gwen.png"
                };
                img.SetPosition(10, 10);
                img.SetSize(100, 100);
            }

            /* Missing */
            {
                Control.ImagePanel img = new Control.ImagePanel(this)
                {
                    ImageName = "missingimage.png"
                };
                img.SetPosition(120, 10);
                img.SetSize(100, 100);
            }

            /* Clicked */
            {
                Control.ImagePanel img = new Control.ImagePanel(this)
                {
                    ImageName = "gwen.png"
                };
                img.SetPosition(10, 120);
                img.SetSize(100, 100);
                img.Clicked += Image_Clicked;
            }
        }

        private void Image_Clicked(Base control, EventArgs args)
        {
            UnitPrint("Image: Clicked");
        }
    }
}

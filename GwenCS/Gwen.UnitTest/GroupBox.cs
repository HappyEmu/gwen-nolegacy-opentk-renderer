using Gwen.Control;

namespace Gwen.UnitTest
{
    public class GroupBox : GUnit
    {
        public GroupBox(Base parent) : base(parent)
        {
            {
                Control.GroupBox gb = new Control.GroupBox(this)
                {
                    Text = "Group Box (centered)"
                };
                gb.SetBounds(10, 10, 200, 100);
                //Align.Center(gb);
            }

            {
                Control.GroupBox gb = new Control.GroupBox(this)
                {
                    AutoSizeToContents = true,
                    Text = "With Label (autosized)"
                };
                gb.SetPosition(250, 10);
                Control.Label label = new Control.Label(gb)
                {
                    Text = "I'm a label"
                };
            }

            {
                Control.GroupBox gb = new Control.GroupBox(this)
                {
                    AutoSizeToContents = true,
                    Text = "With Label (autosized)"
                };
                gb.SetPosition(250, 50);
                Control.Label label = new Control.Label(gb)
                {
                    Text = "I'm a label. I'm a really long label!"
                };
            }

            {
                Control.GroupBox gb = new Control.GroupBox(this)
                {
                    AutoSizeToContents = true,
                    Text = "Two docked Labels (autosized)"
                };
                gb.SetPosition(250, 100);
                Control.Label label1 = new Control.Label(gb)
                {
                    Text = "I'm a label",
                    Dock = Pos.Top
                };
                Control.Label label2 = new Control.Label(gb)
                {
                    Text = "I'm a label. I'm a really long label!",
                    Dock = Pos.Top
                };
            }

            {
                Control.GroupBox gb = new Control.GroupBox(this)
                {
                    AutoSizeToContents = true,
                    Text = "Empty (autosized)"
                };
                gb.SetPosition(10, 150);
            }

            {
                Control.GroupBox gb1 = new Control.GroupBox(this)
                {
                    //Control.Label gb1 = new Control.Label(this);
                    Padding = Padding.Five,
                    Text = "Yo dawg,"
                };
                gb1.SetPosition(10, 200);
                gb1.SetSize(350, 200);
                //gb1.AutoSizeToContents = true;

                Control.GroupBox gb2 = new Control.GroupBox(gb1)
                {
                    Text = "I herd",
                    Dock = Pos.Left,
                    Margin = Margin.Three,
                    Padding = Padding.Five
                };
                //gb2.AutoSizeToContents = true;

                Control.GroupBox gb3 = new Control.GroupBox(gb1)
                {
                    Text = "You like",
                    Dock = Pos.Fill
                };

                Control.GroupBox gb4 = new Control.GroupBox(gb3)
                {
                    Text = "Group Boxes,",
                    Dock = Pos.Top,
                    AutoSizeToContents = true
                };

                Control.GroupBox gb5 = new Control.GroupBox(gb3)
                {
                    Text = "So I put Group",
                    Dock = Pos.Fill
                };
                //gb5.AutoSizeToContents = true;

                Control.GroupBox gb6 = new Control.GroupBox(gb5)
                {
                    Text = "Boxes in yo",
                    Dock = Pos.Left,
                    AutoSizeToContents = true
                };

                Control.GroupBox gb7 = new Control.GroupBox(gb5)
                {
                    Text = "Boxes so you can",
                    Dock = Pos.Top
                };
                gb7.SetSize(100, 100);

                Control.GroupBox gb8 = new Control.GroupBox(gb7)
                {
                    Text = "Group Box while",
                    Dock = Pos.Top,
                    Margin = Gwen.Margin.Five,
                    AutoSizeToContents = true
                };

                Control.GroupBox gb9 = new Control.GroupBox(gb7)
                {
                    Text = "u Group Box",
                    Dock = Pos.Bottom,
                    Padding = Gwen.Padding.Five,
                    AutoSizeToContents = true
                };


            }


            // at the end to apply to all children
            DrawDebugOutlines = true;
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

using System.Runtime.InteropServices;

namespace MaterialWinforms.Controls
{

    public class MaterialPanel : UserControl, IMaterialControl
    {
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
        [Browsable(false)]
        public MouseState MouseState { get; set; }

        public new Boolean AutoScroll
        {
            get
            {
                return MainPanel.AutoScroll;
            }
            set
            {
                MainPanel.AutoScroll = value;
                VerticalScrollbar.Visible = value;
                HorizontalScrollbar.Visible = value;
            }
        }

        private MaterialScrollBar VerticalScrollbar, HorizontalScrollbar;
        private Boolean VerticalScrollbarAdded, HorizontalScrollbarAdded;
        private MaterialDisplayingPanel MainPanel;

        private bool ignoreResize = true;
        public override Color BackColor { get { return SkinManager.GetCardsColor(); } }

        public new ControlCollection Controls
        {
            get
            {
                return MainPanel.Controls;
            }
        }

        public MaterialPanel()
        {

            Size = new Size(50, 50);
            DoubleBuffered = true;
            VerticalScrollbar = new MaterialScrollBar(MaterialScrollOrientation.Vertical);
            VerticalScrollbar.Scroll += Scrolled;
            HorizontalScrollbar = new MaterialScrollBar(MaterialScrollOrientation.Horizontal);
            HorizontalScrollbar.Scroll += Scrolled;

            MainPanel = new MaterialDisplayingPanel();
            MainPanel.Resize += MainPanel_Resize;
            MainPanel.Location = new Point(0, 0);

            MainPanel.Size = new Size(Width - VerticalScrollbar.Width, Height - HorizontalScrollbar.Height);
            MainPanel.Anchor = ((AnchorStyles)AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right|AnchorStyles.Top);

            VerticalScrollbar.Location = new Point(Width - VerticalScrollbar.Width, 0);
            VerticalScrollbar.Size = new Size(VerticalScrollbar.Width, Height - HorizontalScrollbar.Height);
            VerticalScrollbar.Anchor = ((AnchorStyles)AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right);

            HorizontalScrollbar.Location = new Point(0, Height - HorizontalScrollbar.Height);
            HorizontalScrollbar.Size = new Size(Width - VerticalScrollbar.Width, HorizontalScrollbar.Height);
            HorizontalScrollbar.Anchor = ((AnchorStyles)AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right);

            base.Controls.Add(MainPanel);
            base.Controls.Add(VerticalScrollbar);
            base.Controls.Add(HorizontalScrollbar);
            MainPanel.ControlAdded += MaterialPanel_ControlsChanged;
            MainPanel.ControlRemoved += MaterialPanel_ControlsChanged;
            MainPanel.onScrollBarChanged += MainPanel_onScrollBarChanged;
            AutoScroll = true;

            Controls.Add(new MaterialCard()
            {
                Height = 1000,
                Width = 1000
            });

            ignoreResize = false;
        }

        void MainPanel_onScrollBarChanged(Orientation pScrollOrientation, bool pVisible)
        {
            UpdateScrollbars();
        }

        void Scrolled(object sender, ScrollEventArgs e)
        {
            MainPanel.AutoScrollPosition = new Point(HorizontalScrollbar.Value, VerticalScrollbar.Value);
        }

        void MaterialPanel_ControlsChanged(object sender, ControlEventArgs e)
        {
            UpdateScrollbars();
        }

        void MainPanel_Resize(object sender, EventArgs e)
        {
            UpdateScrollbars();
        }


        protected override void OnResize(EventArgs eventargs)
        {

            base.OnResize(eventargs);
            UpdateScrollbars();

        }


        private void UpdateScrollbars()
        {
            if(ignoreResize)
            {
                return;
            }
            VerticalScrollbar.Minimum = MainPanel.VerticalScroll.Minimum;
            VerticalScrollbar.Maximum = MainPanel.VerticalScroll.Maximum;
            VerticalScrollbar.LargeChange = MainPanel.VerticalScroll.LargeChange;
            VerticalScrollbar.SmallChange = MainPanel.VerticalScroll.SmallChange;

            HorizontalScrollbar.Minimum = MainPanel.HorizontalScroll.Minimum;
            HorizontalScrollbar.Maximum = MainPanel.HorizontalScroll.Maximum;
            HorizontalScrollbar.LargeChange = MainPanel.HorizontalScroll.LargeChange;
            HorizontalScrollbar.SmallChange = MainPanel.HorizontalScroll.SmallChange;

            if (MainPanel.VerticalScroll.Visible && !VerticalScrollbarAdded)
            {
                VerticalScrollbarAdded = true;
                VerticalScrollbar.Visible = true;
            }
            else if (!MainPanel.VerticalScroll.Visible && VerticalScrollbarAdded)
            {
                VerticalScrollbarAdded = false;
                VerticalScrollbar.Visible = false;
            }
            if (MainPanel.HorizontalScroll.Visible && !HorizontalScrollbarAdded)
            {
                HorizontalScrollbarAdded = true;
                HorizontalScrollbar.Visible = true;
            }
            else if (!MainPanel.HorizontalScroll.Visible && HorizontalScrollbarAdded)
            {
                HorizontalScrollbarAdded = false;
                HorizontalScrollbar.Visible = false;
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

        }
    }


    internal class MaterialDisplayingPanel : Panel, IMaterialControl
    {
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
        [Browsable(false)]
        public MouseState MouseState { get; set; }
        public override Color BackColor { get { return SkinManager.GetApplicationBackgroundColor(); } }

        public delegate void ScrollbarChanged(Orientation pScrollOrientation, Boolean pVisible);

        public event ScrollbarChanged onScrollBarChanged;
        public MaterialDisplayingPanel()
        {
            DoubleBuffered = true;
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);

        private enum ScrollBarDirection
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            if (onScrollBarChanged != null)
            {
                onScrollBarChanged(Orientation.Horizontal, HorizontalScroll.Visible);
                onScrollBarChanged(Orientation.Vertical, VerticalScroll.Visible);
            }
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (onScrollBarChanged != null)
            {
                onScrollBarChanged(Orientation.Horizontal, HorizontalScroll.Visible);
                onScrollBarChanged(Orientation.Vertical, VerticalScroll.Visible);
            }
            ShowScrollBar(this.Handle, (int)ScrollBarDirection.SB_HORZ, false);
            ShowScrollBar(this.Handle, (int)ScrollBarDirection.SB_VERT, false);
            base.WndProc(ref m);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

            foreach (Control objChild in Controls)
            {
                if (typeof(IShadowedMaterialControl).IsAssignableFrom(objChild.GetType()))
                {
                    IShadowedMaterialControl objCurrent = (IShadowedMaterialControl)objChild;
                    DrawHelper.drawShadow(e.Graphics, objCurrent.ShadowBorder, objCurrent.Elevation, SkinManager.GetApplicationBackgroundColor());
                }

            }
        }
    }
}

